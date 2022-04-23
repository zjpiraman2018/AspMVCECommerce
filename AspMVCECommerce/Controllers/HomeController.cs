using AspMVCECommerce.Models;
using AspMVCECommerce.Utility;
using AspMVCECommerce.ViewModel;
using PagedList;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;



namespace AspMVCECommerce.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private CheckOut checkOutObj { get; set; }
        [Authorize(Roles = "Customer")]
        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            if (Cancel != null)
                if (Cancel.ToLower().Trim() == "true")
                {
                    string homeUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/Home/Index";
                    return Content($"<script>window.location = '{homeUrl}';</script>");
                }

            if (Request.Form.Count > 0)
            {

                checkOutObj = new CheckOut();
                checkOutObj.FirstName = Request.Form.GetValues("FirstName").FirstOrDefault().ToUpper();
                checkOutObj.LastName = Request.Form.GetValues("LastName").FirstOrDefault().ToUpper();
                checkOutObj.Email = Request.Form.GetValues("Email").FirstOrDefault();
                checkOutObj.Mobile = Request.Form.GetValues("MobilePhone").FirstOrDefault();

                checkOutObj.CustomAddress = new CustomAddress();
                checkOutObj.CustomAddress.Line1 = Request.Form.GetValues("CALine1").FirstOrDefault().ToUpper();
                checkOutObj.CustomAddress.Line2 = Request.Form.GetValues("CALine2").FirstOrDefault().ToUpper();
                checkOutObj.CustomAddress.City = Request.Form.GetValues("CACity").FirstOrDefault().ToUpper();
                checkOutObj.CustomAddress.PostalCode = Request.Form.GetValues("CAPostalCode").FirstOrDefault();
                checkOutObj.CustomAddress.Province = Request.Form.GetValues("CAProvince").FirstOrDefault();
                checkOutObj.CustomAddress.Phone = checkOutObj.Mobile;

                checkOutObj.ShippingAddress = new ShippingAddress2();
                checkOutObj.ShippingAddress.Line1 = Request.Form.GetValues("SALine1").FirstOrDefault().ToUpper();
                checkOutObj.ShippingAddress.Line2 = Request.Form.GetValues("SALine2").FirstOrDefault().ToUpper();
                checkOutObj.ShippingAddress.City = Request.Form.GetValues("SACity").FirstOrDefault().ToUpper();
                checkOutObj.ShippingAddress.PostalCode = Request.Form.GetValues("SAPostalCode").FirstOrDefault();
                checkOutObj.ShippingAddress.Province = Request.Form.GetValues("SAProvince").FirstOrDefault();
                checkOutObj.ShippingAddress.Phone = checkOutObj.Mobile;
                checkOutObj.ShippingAddress.FirstName = Request.Form.GetValues("RFirstName").FirstOrDefault().Replace(" ", "").ToUpper();
                checkOutObj.ShippingAddress.LastName = Request.Form.GetValues("RLastName").FirstOrDefault().Replace(" ", "").ToUpper();

                checkOutObj.ShippingAddress.RecipientName = checkOutObj.ShippingAddress.FirstName + " " + checkOutObj.ShippingAddress.LastName;
            }
            // get Start (paging start index) and length (page size for paging)

            string tempPaymentId = "";
            //getting the apiContext  
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Home/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    this.HttpContext.Session["TempPaymentId"] = createdPayment.id;
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);



                    //createdPayment.payer.payer_info.billing_address
                    //return Redirect(paypalRedirectUrl);

                    return Content($"<script>window.location = '{paypalRedirectUrl}';</script>");
                }
                else
                {

                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);





                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }


                    // Zaldy custom paypal code
                    tempPaymentId = (string)this.HttpContext.Session["TempPaymentId"];
                    var paymentExecution = new PaymentExecution() { payer_id = payerId };
                    this.payment = new Payment() { id = tempPaymentId };
                    var paymentInfo = this.payment.Execute(apiContext, paymentExecution);
               
                    // var creditCard = GetCreditCard().Create(apiContext);


                    SaveCheckOutDetails(paymentInfo);
                    //this.RecordConnectionDetails();

                    //Assert.IsNotNull(retrievedCreditCard);
                    //Assert.IsNotNull(retrievedCreditCard.billing_address
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View("FailureView");
            }



            //on successful payment, show success page to user.  


            return RedirectToAction("Order");
            //return View("SuccessView");
        }


        private CheckOut UpdateCheckOutDetails(Payment payment, CheckOut checkOut)
        {

            if(payment.payer.payer_info.billing_address != null)
            {
                checkOut.CustomAddress.City = payment.payer.payer_info.billing_address.city;
                checkOut.CustomAddress.Line1 = payment.payer.payer_info.billing_address.line1;

                checkOut.CustomAddress.Line2 = payment.payer.payer_info.billing_address.line2;
                checkOut.CustomAddress.Phone = payment.payer.payer_info.billing_address.phone;
                checkOut.CustomAddress.PostalCode = payment.payer.payer_info.billing_address.postal_code;
                checkOut.CustomAddress.CountryCode = payment.payer.payer_info.billing_address.country_code;
                checkOut.CustomAddress.Province = payment.payer.payer_info.billing_address.state;
            }
            else
            {
                checkOut.CustomAddress.City = "";
                checkOut.CustomAddress.Line1 = "";
                checkOut.CustomAddress.Line2 = "";
                checkOut.CustomAddress.Phone = "";
                checkOut.CustomAddress.PostalCode = "";
                checkOut.CustomAddress.CountryCode = "";
                checkOut.CustomAddress.Province = "";
            }


            if (!string.IsNullOrEmpty(payment.payer.payer_info.shipping_address.recipient_name))
            {
                if (payment.payer.payer_info.shipping_address.recipient_name.Trim() != "")
                {

                    if (payment.payer.payer_info.shipping_address.country_code.Trim().ToUpper() != "PH")
                    {
                        throw new ArgumentException("Shipping address is invalid\nWe cannot shipped product outside the philippines. your payment transaction will be refunded within 24hours.\nKindly save your invoice no#: " + (string)this.HttpContext.Session["INVOICENO"] + " for reference");
                    }
                    checkOut.ShippingAddress.City = payment.payer.payer_info.shipping_address.city;
                    checkOut.ShippingAddress.Line1 = payment.payer.payer_info.shipping_address.line1;
                    checkOut.ShippingAddress.Line2 = payment.payer.payer_info.shipping_address.line2;
                    checkOut.ShippingAddress.Phone = payment.payer.payer_info.shipping_address.phone;
                    checkOut.ShippingAddress.PostalCode = payment.payer.payer_info.shipping_address.postal_code;
                    checkOut.ShippingAddress.CountryCode = payment.payer.payer_info.shipping_address.country_code;
                    checkOut.ShippingAddress.RecipientName = payment.payer.payer_info.shipping_address.recipient_name;
                    checkOut.ShippingAddress.Province = payment.payer.payer_info.shipping_address.state;
                }
                else
                {
                    checkOut.ShippingAddress = null;
                }
            }
            else
            {
                checkOut.ShippingAddress = null;
            }


            checkOut.FirstName = payment.payer.payer_info.first_name;
            checkOut.LastName = payment.payer.payer_info.last_name;
            checkOut.Mobile = payment.payer.payer_info.phone;
            checkOut.Email = payment.payer.payer_info.email;

            return checkOut;
        }


        private void SaveCheckOutDetails(Payment payment)
        {
            string userId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
            int shoppingCartId = ShoppingCartUtility.GetShoppingCartId(userId, db);
           

            var checkOut = UpdateCheckOutDetails(payment, (CheckOut)this.HttpContext.Session["CheckOutDetails"]);
            checkOut.ShoppingCartId = shoppingCartId;


            CheckOutUtility.AddCheckOut(checkOut, db);

            var tempLineItems = GetLineItemsForCheckOut();
            int totalItems = tempLineItems.Count;
            int totalAmount = tempLineItems.Sum(l => l.TotalPrice);

            OrderUtility.AddOrder( shoppingCartId, totalItems, totalAmount, userId, db);

            ShoppingCartUtility.AddShoppingCart(userId, db);
        }


        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var randomProfileName = "PROFILENAME-" + DateTime.Now.ToString("MMddyyyyhhmmssffftt");
            // Create the web experience profile
            var profile = new WebProfile
            {
                name = randomProfileName,
                //presentation = new Presentation
                //{
                //    brand_name = "My brand name",
                //    locale_code = "US",
                //    logo_image = ""
                //},
                input_fields = new InputFields
                {
                    no_shipping = 0
                }
            };

            var createdProfile = profile.Create(apiContext);

            this.payment = new Payment()
            {
                id = paymentId,
                experience_profile_id = createdProfile.id
            };




            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            //this.payment = new Payment()
            //{
            //    id = paymentId
            //};

          


            return this.payment.Execute(apiContext, paymentExecution);
        }
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {

            var lineitems1 = GetLineItemsForCheckOut();
            ShippingAddress shippingAddress = null;
            ItemList itemList = null;

            if (!string.IsNullOrEmpty(checkOutObj.ShippingAddress.RecipientName.Trim()))
            {
                shippingAddress = new ShippingAddress();

                shippingAddress.city = checkOutObj.ShippingAddress.City;
                shippingAddress.country_code = checkOutObj.ShippingAddress.CountryCode;
                shippingAddress.line1 = checkOutObj.ShippingAddress.Line1;
                shippingAddress.phone = checkOutObj.ShippingAddress.Phone;
                shippingAddress.postal_code = checkOutObj.ShippingAddress.PostalCode;
                shippingAddress.state = checkOutObj.ShippingAddress.Province;
                shippingAddress.line2 = checkOutObj.ShippingAddress.Line2;
                shippingAddress.default_address = false; //checkOutObj.ShippingAddress.DefaultAddress;
                shippingAddress.preferred_address = false; //checkOutObj.ShippingAddress.PreferredAddress;
                shippingAddress.recipient_name = checkOutObj.ShippingAddress.RecipientName;
                shippingAddress.status = checkOutObj.ShippingAddress.Status;
                shippingAddress.type = checkOutObj.ShippingAddress.Type;

                //with shipping address create itemlist and add item objects to it  
                itemList = new ItemList()
                {
                    items = new List<Item>(),
                    shipping_address = shippingAddress
                };
                //Adding Item Details like name, currency, price etc  
            }
            else
            {
                // without shipping address create itemlist and add item objects to it  
                itemList = new ItemList()
                {
                    items = new List<Item>()
                };
                //Adding Item Details like name, currency, price etc  
            }


            foreach (var lineitem in lineitems1)
            {
                bool isPromo = (lineitem.Product.PromoSaleOFF > 0 ? (DateTime.Now >= lineitem.Product.PromoSaleStartDateTime && DateTime.Now <= lineitem.Product.PromoSaleEndDateTime) : false);

                string tempPrice = isPromo ? lineitem.Product.DiscountedPrice.ToString() : lineitem.Product.OriginalPrice.ToString();

                itemList.items.Add(new Item()
                {
                    name = lineitem.Product.Name.ToUpper(),
                    currency = "PHP",
                    //price = lineitem.TotalPrice.ToString(),
                    price = tempPrice,
                    quantity = lineitem.Quantity.ToString(),
                    sku = lineitem.ProductId.ToString()
                });

            }
            //itemList.items.Add(new Item()
            //{
            //    name = "Item Name comes here",
            //    currency = "PHP",
            //    price = "1",
            //    quantity = "1",
            //    sku = "sku"
            //});


            var payer = new Payer()
            {
                payment_method = "paypal"
            };


            payer.payer_info = new PayerInfo();


            Address customAddress = new Address();

            customAddress.city = checkOutObj.CustomAddress.City;
            customAddress.country_code = checkOutObj.CustomAddress.CountryCode;
            customAddress.line1 = checkOutObj.CustomAddress.Line1;
            customAddress.phone = checkOutObj.CustomAddress.Phone; 
            customAddress.postal_code = checkOutObj.CustomAddress.PostalCode;
            customAddress.state = checkOutObj.CustomAddress.Province;
            customAddress.line2 = checkOutObj.CustomAddress.Line2;
            customAddress.status = checkOutObj.CustomAddress.Status;
            customAddress.type = checkOutObj.CustomAddress.Type;


            payer.payer_info.billing_address = customAddress;
            
            //remove zaldy for testing
            payer.payer_info.billing_address = null;
            
            if (shippingAddress != null)
            {
                payer.payer_info.shipping_address = shippingAddress;
            }

            payer.payer_info.country_code = checkOutObj.CountryCode;
            payer.payer_info.email = checkOutObj.Email;
            payer.payer_info.first_name = checkOutObj.FirstName;
            payer.payer_info.last_name = checkOutObj.LastName;
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = lineitems1.Sum(l => l.TotalPrice).ToString()
            };
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "PHP",
                total = lineitems1.Sum(l => l.TotalPrice).ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };


            string cartid = lineitems1.Count > 0 ? lineitems1[0].ShoppingCartId.ToString() : "";
            var randomInvoiceNo = "INVOICENO-" + cartid + DateTime.Now.ToString("MMddyyyyhhmmssffftt");
            
            
            this.HttpContext.Session["INVOICENO"] = randomInvoiceNo;

            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            transactionList.Add(new Transaction()
            {
                description = "Zaldy's Ecommerce Checkout",

                //invoice_number = "your generated invoice number", //Generate an Invoice No  
                invoice_number = randomInvoiceNo,

                amount = amount,
                item_list = itemList

            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls

            };


            this.HttpContext.Session["CheckOutDetails"] = checkOutObj;
            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }


        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category).Include(p => p.Images).ToList();
            ViewBag.SelectedNavCategory = "Home";
            return View(products);
        }

        private int GetMaxPriceRange(int price)
        {
            string stringDivident = "";
            decimal wholeNumberPrice = 0;
            int maxPrice = 0;

            if(price >= 10000)
            {
       
                stringDivident = ((double)price / 10000).ToString();
                wholeNumberPrice = Math.Truncate(decimal.Parse(stringDivident));
                maxPrice = (int.Parse(wholeNumberPrice.ToString()) + (stringDivident.IndexOf(".") > -1 ? 1 : 0)) * 10000;

            }
            else if(price < 10000 && price >= 1000)
            {
                stringDivident = ((double)price / 1000).ToString();
                wholeNumberPrice = Math.Truncate(decimal.Parse(stringDivident));
                maxPrice = (int.Parse(wholeNumberPrice.ToString()) + (stringDivident.IndexOf(".") > -1 ? 1 : 0)) * 1000;
            }
            else if (price < 1000)
            {
                stringDivident = ((double)price / 100).ToString();
                wholeNumberPrice = Math.Truncate(decimal.Parse(stringDivident));
                maxPrice = (int.Parse(wholeNumberPrice.ToString()) + (stringDivident.IndexOf(".") > -1 ? 1 : 0)) * 100;
            }
            else
            {
                maxPrice = 100;
            }

            return maxPrice;
        }

        public ActionResult Store(string pageSort, int? page, string pageSize, string selectedCategory, string notSelectedBrand, int? minPrice, int? maxPrice, int? maxPriceRange, string selectedNavCategory, string searchProducts)
        {
            var products = db.Products.Include(p => p.Category).Include(p => p.Images);
            var categoryIdList = new List<int>();
            var notSelectedBrandIdList = new List<int>();
            ViewBag.CurrentSort = pageSort;

            ViewBag.SelectedNavCategory = selectedNavCategory;
            
            switch (pageSort)
            {
                case "ProductName":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "CreatedDate":
                    products = products.OrderBy(p => p.CreatedDateTime);
                    break;
                case "Popular":
                    // PENDING CHECK OUT PAGE NEEDED
                    products = products.OrderBy(p => p.Name);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }


            // -- FILTER BY PROMO SALES ---
            if (!string.IsNullOrEmpty(selectedNavCategory))
            {
                if (selectedNavCategory == "Hot Deals")
                {
                    products = products.Where(p => p.PromoSaleOFF > 0 ? (DateTime.Now >= p.PromoSaleStartDateTime && DateTime.Now <= p.PromoSaleEndDateTime) : false);
                }

            }
            // -- END OF FILTER BY PROMO SALES ---




            // -- FILTER BY SEARCH PRODUCTS ---
            if (!string.IsNullOrEmpty(searchProducts))
            {
                products = products.Where(p => p.Name.Contains(searchProducts));
            }

            // -- END OF FILTER BY SEARCH PRODUCTS ---




            // Get product by selected category
            if (!string.IsNullOrEmpty(selectedCategory))
            {
                categoryIdList = selectedCategory.Split(',').ToList().Select(c => Int32.Parse(c)).ToList();
                products = products.Where(p => categoryIdList.Contains(p.CategoryId));

                // Get product by price range
                int _maxPriceRange = products.Count() == 0 ? 100 : GetMaxPriceRange(products.Max(p => p.OriginalPrice));

                if (maxPrice == null)        
                {
                    minPrice = 1;
                    maxPrice = _maxPriceRange;
                    maxPriceRange= _maxPriceRange;
                }
                else
                {
                    if (categoryIdList.Count == 1 && maxPrice == 100 && maxPriceRange == 100)
                    {
                        maxPrice = _maxPriceRange;
                    }
                    else
                    {


                        if (maxPrice == maxPriceRange)
                        {
                            maxPrice = _maxPriceRange;
                            maxPriceRange = _maxPriceRange;
                        }


                        if (maxPrice > _maxPriceRange)
                        {
                            maxPrice = _maxPriceRange;
                        }


                        if (minPrice >= maxPrice)
                        {
                            minPrice = 1;
                        }
                    }

                    maxPriceRange = _maxPriceRange;
                }

                products = products.Where(p => p.OriginalPrice >= minPrice && p.OriginalPrice <= maxPrice);
            }
            else
            {
                maxPriceRange = 100;
                maxPrice = maxPriceRange;
                minPrice = 1;
                products = new List<Product>().AsQueryable();
            }




            ViewBag.MaxPrice = maxPrice;
            ViewBag.MaxPriceRange = maxPriceRange;
            ViewBag.MinPrice = minPrice;



            // -- GET CATEGORIES WITH PRODUCT COUNT -----
            List<CategoryViewModel> categories = new List<CategoryViewModel>();

            if (!string.IsNullOrEmpty(selectedNavCategory))
            {
                if (selectedNavCategory == "Hot Deals")
                {
                    // GET CATEGORIES THAT HAVE PROMO SALES
                    categories = db.Products.Include(p => p.Category)
                            .Where(p => p.PromoSaleOFF > 0 ? (DateTime.Now >= p.PromoSaleStartDateTime && DateTime.Now <= p.PromoSaleEndDateTime) : false)
                            .GroupBy(c => c.CategoryId)
                            .Select(p => new CategoryViewModel
                            {
                                CategoryId = p.FirstOrDefault().CategoryId,
                                Name = p.FirstOrDefault().Category.Name,
                                ProductCount = p.Count(),
                            }).ToList();

                    // END OF GET CATEGORIES THAT HAVE PROMO SALES
                }
                else
                {

                    if (!string.IsNullOrEmpty(searchProducts))
                    {

                        // GET CATEGORIES THAT CONTAIN THE PRODUCT SEARCH
                        categories = db.Products.Include(p => p.Category)
                                .Where(p => p.Name.Contains(searchProducts))
                                .GroupBy(c => c.CategoryId)
                                .Select(p => new CategoryViewModel
                                {
                                    CategoryId = p.FirstOrDefault().CategoryId,
                                    Name = p.FirstOrDefault().Category.Name,
                                    ProductCount = p.Count(),
                                }).ToList();

                        // END OF GET CATEGORIES THAT CONTAIN THE PRODUCT SEARCH
                    }
                    else
                    {
                        categories = db.Products.Include(p => p.Category)
                            .GroupBy(c => c.CategoryId)
                            .Select(p => new CategoryViewModel
                            {
                                CategoryId = p.FirstOrDefault().CategoryId,
                                Name = p.FirstOrDefault().Category.Name,
                                ProductCount = p.Count(),
                            }).ToList();
                    }

                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchProducts))
                {
                    
                    // GET CATEGORIES THAT CONTAIN THE PRODUCT SEARCH
                    categories = db.Products.Include(p => p.Category)
                            .Where(p => p.Name.Contains(searchProducts))
                            .GroupBy(c => c.CategoryId)
                            .Select(p => new CategoryViewModel
                            {
                                CategoryId = p.FirstOrDefault().CategoryId,
                                Name = p.FirstOrDefault().Category.Name,
                                ProductCount = p.Count(),
                            }).ToList();
                    // END OF GET CATEGORIES THAT CONTAIN THE PRODUCT SEARCH
                }
                else
                {
                    categories = db.Products.Include(p => p.Category)
                        .GroupBy(c => c.CategoryId)
                        .Select(p => new CategoryViewModel
                        {
                            CategoryId = p.FirstOrDefault().CategoryId,
                            Name = p.FirstOrDefault().Category.Name,
                            ProductCount = p.Count(),
                        }).ToList();
                }
            }


            var idList = categories.Select(x => x.CategoryId).ToList();


            var zeroCategories = db.Categories
                .Where(c => !idList.Contains(c.CategoryId))
                .Select(c => new CategoryViewModel()
                {
                  CategoryId =   c.CategoryId,
                    Name=   c.Name,
                    ProductCount = 0
                }).ToList();


            categories.AddRange(zeroCategories);
            // -- END OF GET CATEGORIES WITH PRODUCT COUNT -----



            // -- GET BRANDS WITH PRODUCT COUNT -----

                
            var brands = products
                .GroupBy(c => c.BrandId)
                .Select(p => new BrandViewModel
                {
                    BrandId = p.FirstOrDefault().BrandId,
                    Name = p.FirstOrDefault().Brand.Name,
                    ProductCount = p.Count(),
                }).ToList();


            //if (string.IsNullOrEmpty(selectedBrand))
            //{
            //    selectedBrand = "";
            //    foreach (var brand in brands)
            //    {
            //        selectedBrand = selectedBrand == "" ? brand.BrandId.ToString() : selectedBrand + "," + brand.BrandId.ToString();
            //    }
            //    brandIdList = brands.Select(x => x.BrandId).ToList();
            //}
            if (!string.IsNullOrEmpty(notSelectedBrand))
            {
                notSelectedBrandIdList = notSelectedBrand.Split(',').ToList().Select(c => Int32.Parse(c)).ToList();
            }


            string selectedBrand = "";
            foreach (var brand in brands)
            {

                if (!notSelectedBrandIdList.Contains(brand.BrandId))
                {
                    selectedBrand = selectedBrand == "" ? brand.BrandId.ToString() : selectedBrand + "," + brand.BrandId.ToString();
                }
              
            }
            //brandIdList = brands.Select(x => x.BrandId).ToList();


            if (!string.IsNullOrEmpty(notSelectedBrand))
            {
                products = products.Where(p => !notSelectedBrandIdList.Contains(p.BrandId));
            }



            //if (brandIdList.Count > 0)
            //{
            //    var brandIdList2 = brands.Select(x => x.BrandId).ToList();
            //    var zeroBrands = db.Brands
            //                    .Where(b => brandIdList.Contains(b.BrandId) && !brandIdList2.Contains(b.BrandId))
            //                    .Select(c => new BrandViewModel()
            //                    {
            //                        BrandId = c.BrandId,
            //                        Name = c.Name,
            //                        ProductCount = 0
            //                    }).ToList();
            //    brands.AddRange(zeroBrands);
            //}



            // -- END OF GET BRANDS WITH PRODUCT COUNT -----






            ViewBag.NotSelectedBrand = notSelectedBrand;

                    ViewBag.SelectedCategory = selectedCategory;
            ViewBag.Categories = categories;

            ViewBag.SelectedBrand = selectedBrand;
            ViewBag.Brands = brands;
            ViewBag.TotalResult = products.Count();
            ViewBag.SearchProducts = searchProducts;

            int _pageSize = string.IsNullOrEmpty(pageSize) ? 20 : Int32.Parse(pageSize);
            ViewBag.CurrentItemsPerPage = _pageSize;
            int pageNumber = (page ?? 1);


            return View(products.ToPagedList(pageNumber, _pageSize));

        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public ActionResult Product(int? productId, string selectedNavCategory)
        {
            if (productId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Include(p => p.Category).Include(p => p.Brand).Include(p => p.Images).Single(p => p.ProductId == productId);
            product.Description = WebUtility.HtmlDecode(product.Description).Replace("'", "\\'").Replace("\r\n", "");
            product.Details = WebUtility.HtmlDecode(product.Details).Replace("'", "\\'").Replace("\r\n", "");
            if (product == null)
            {
                return HttpNotFound();
            }

            IQueryable<Product> products = null;

            // -- FILTER BY PROMO SALES ---
            if (!string.IsNullOrEmpty(selectedNavCategory))
            {
                if (selectedNavCategory == "Hot Deals")
                {
                    products = db.Products.Include(p => p.Category).Include(p => p.Images).Where(p => p.CategoryId == product.CategoryId && p.ProductId != productId && (p.PromoSaleOFF > 0 ? (DateTime.Now >= p.PromoSaleStartDateTime && DateTime.Now <= p.PromoSaleEndDateTime) : false)).Take(4);
                }
                else
                {
                    products = db.Products.Include(p => p.Category).Include(p => p.Images).Where(p => p.CategoryId == product.CategoryId && p.ProductId != productId).Take(4);
                }
            }
            else
            {
                products = db.Products.Include(p => p.Category).Include(p => p.Images).Where(p => p.CategoryId == product.CategoryId && p.ProductId != productId).Take(4);
            }
            // -- END OF FILTER BY PROMO SALES ---

            ViewBag.Sizes = new SelectList(db.Sizes.Where(s => s.ProductId == productId).ToList(), "SizeId", "Name");
            ViewBag.Colors = new SelectList(db.Colors.Where(c => c.ProductId == productId).ToList(), "ColorId", "Name");

            ViewBag.SelectedNavCategory = selectedNavCategory;




            ViewBag.RelatedProduct = products;
  


            return View(product);
        }

        public ActionResult ShoppingCart()
        {
            //if (productId == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}

            string userId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
            int shoppingCartId = ShoppingCartUtility.GetShoppingCartId(userId, db); 

            var lineItems = db.LineItems.Include(l => l.Product).Include(l => l.Product.Images).Include(l=>l.Size).Include(l=>l.Color).Where(l=>l.ShoppingCartId == shoppingCartId).ToList();
            //ViewBag.Sizes = new SelectList(db.Sizes.Where(s => s.ProductId == productId).ToList(), "SizeId", "Name");
 
            //ViewBag.RelatedProduct = products;


            return View(lineItems);
        }

        [Authorize(Roles = "Customer")]
        public ActionResult CheckOut()
        {
            return View(GetLineItemsForCheckOut());
        }


        private List<LineItem> GetLineItemsForCheckOut()
        {
            string userId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
            int shoppingCartId = ShoppingCartUtility.GetShoppingCartId(userId, db); ;

            var lineItems = db.LineItems.Include(l => l.Product).Where(l => l.ShoppingCartId == shoppingCartId).ToList();

            return lineItems;
        }

        public ActionResult Order()
        {
            // 1. Create new Shopping Cart
            // 2. Save Checkout Details
            // 3. Save Custome Address
            // 4. Save Shippinh Address
            // 5. Create new Order
            // 6. Display List of order


            string userId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
            //int shoppingCartId = ShoppingCartUtility.GetShoppingCartId(userId, db); ;
         
           var orders =  db.Orders.Include(o => o.ShoppingCart).Where(l => l.ShoppingCart.CustomerId == userId).ToList();
            return View(orders);
        }

        public ActionResult OrderDetails(int orderId)
        {
            var order = db.Orders.Where(o=>o.OrderId == orderId).FirstOrDefault();
            var checkout = db.CheckOuts.Include(c => c.ShippingAddress).Include(c => c.CustomAddress).Where(c => c.ShoppingCartId == order.ShoppingCartId).FirstOrDefault();
            var lineItems = db.LineItems.Include(l => l.Product).Where(l => l.ShoppingCartId == order.ShoppingCartId).ToList();

            var orderDetailVM = new OrderDetailViewModel()
            {
                Order = order,
                CheckOut = checkout,
                LineItems = lineItems
            };

            return View(orderDetailVM);
        }
    }
}