using AspMVCECommerce.Models;
using AspMVCECommerce.Utility;
using AspMVCECommerce.ViewModel;
using Hangfire;
using Hangfire.Storage;
using HtmlAgilityPack;
using PagedList;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;



namespace AspMVCECommerce.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public HomeController()
        {

        }

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

                    SaveCheckOutDetails(executedPayment);



                    // zaldy custom paypal code for testing
                        //tempPaymentId = (string)this.HttpContext.Session["TempPaymentId"];
                        //var paymentExecution = new PaymentExecution() { payer_id = payerId };
                        //this.payment = new Payment() { id = tempPaymentId };
                        //var paymentInfo = this.payment.Execute(apiContext, paymentExecution);
                        //// var creditCard = GetCreditCard().Create(apiContext);
                        //SaveCheckOutDetails(paymentInfo);
                        //Payment payment = Payment.Get(apiContext, tempPaymentId);
                        //var xxx = payment.payment_instruction;
                        //ShippingAddress shippingAddress = payment.pa.getPayerInfo().getShippingAddress();
                        //Address billingAddress = payment.getPayer().getPayerInfo().getBillingAddress();
                        //this.RecordConnectionDetails();
                        //Assert.IsNotNull(retrievedCreditCard);
                        //Assert.IsNotNull(retrievedCreditCard.billing_address
                    // end of zaldy custom paypal code for testing
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

            OrderUtility.AddOrder((string)this.HttpContext.Session["INVOICENO"], shoppingCartId, totalItems, totalAmount, userId, db);
            
            UpdateProductsSoldByLineItems(tempLineItems);

            ShoppingCartUtility.AddShoppingCart(userId, db);
        }


        private void UpdateProductsSoldByLineItems(List<LineItem> lineItems)
        {
            foreach (var item in lineItems)
            {
                ProductUtility.UpdateProductSold(item.ProductId, item.Quantity, db);
            }
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
                  flow_config = new FlowConfig() { landing_page_type = "Billing"},
                input_fields = new InputFields
                {
                    no_shipping = 1,
                    address_override = 1, 
                    allow_note = true
                     
                },

                temporary = false 
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
            ViewBag.WistList = null;


            if (Request.IsAuthenticated)
            {
                if (User.IsInRole("Customer"))
                {
                    string CustomerId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
     
                    var WistListList = db.WishLists.Where(w => w.CustomerId == CustomerId).ToList();
                    ViewBag.WistList = WistListList;

                }
            }

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
                    products = products.OrderBy(p => p.Sold);
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


            ViewBag.WistList = null;


            if (Request.IsAuthenticated)
            {
                if (User.IsInRole("Customer"))
                {
                    string CustomerId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);

                    var WistListList = db.WishLists.Where(w => w.CustomerId == CustomerId).ToList();
                    ViewBag.WistList = WistListList;
                }
            }




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
            ViewBag.WistList = null;

            if (Request.IsAuthenticated)
            {
                if (User.IsInRole("Customer"))
                {
                     string CustomerId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
                     ViewBag.WistList = db.WishLists.Where(w => w.CustomerId == CustomerId).ToList();
                }
            }

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

        //public ActionResult Order()
        //{
        //    // 1. Create new Shopping Cart
        //    // 2. Save Checkout Details
        //    // 3. Save Custome Address
        //    // 4. Save Shippinh Address
        //    // 5. Create new Order
        //    // 6. Display List of order


        //    string userId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
        //    //int shoppingCartId = ShoppingCartUtility.GetShoppingCartId(userId, db); ;

        //   var orders =  db.Orders.Include(o => o.ShoppingCart).Where(l => l.ShoppingCart.CustomerId == userId).ToList();
        //    return View(orders);
        //}

        [Authorize(Roles = "Customer")]
        // GET: Student
        public ActionResult Order(string currentFilter, string searchString, int? page, string pageSize)
        {
            //ViewBag.CurrentSort = sortOrder;

            //ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            //ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            string userId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);

            var shoppingCartIdList = db.ShoppingCarts.Where(s => s.CustomerId == userId).Select(s=>s.ShoppingCartId).ToList();

            var orders = from s in db.Orders
                         where shoppingCartIdList.Contains(s.ShoppingCartId)  
                         select s;

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            if (!String.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(s => s.InvoiceNo.Contains(searchString));
            }

            //switch (sortOrder)
            //{
            //    case "name_desc":
            //        students = students.OrderByDescending(s => s.LastName);
            //        break;
            //    case "Date":
            //        students = students.OrderBy(s => s.EnrollmentDate);
            //        break;
            //    case "date_desc":
            //        students = students.OrderByDescending(s => s.EnrollmentDate);
            //        break;
            //    default:
            //        students = students.OrderBy(s => s.LastName);
            //        break;
            //}

            orders = orders.OrderByDescending(s => s.CreatedDate);

            int _pageSize = string.IsNullOrEmpty(pageSize) ? 10 : Int32.Parse(pageSize);
            ViewBag.CurrentItemsPerPage = _pageSize;
            int pageNumber = (page ?? 1);
            return View(orders.ToPagedList(pageNumber, _pageSize));
        }


        [Authorize(Roles = "Customer")]
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


        public ActionResult SendEmail()
        {
        
            var randomProductIdList = db.Products
                                .SqlQuery("SELECT TOP 6 * FROM Products ORDER BY NEWID()")
                                .ToList<Product>().Select(p=>p.ProductId).ToList();


            var products = db.Products.Include(p => p.Category).Include(p => p.Images).Where(p=> randomProductIdList.Contains(p.ProductId));

            return View(products);
        }

        public ActionResult Unsubscribe(string email)
        {
            ViewBag.Email = email;
            try
            {
                var newsLetter = db.NewsLetters.Where(n => n.Email.Trim().ToUpper() == email.Trim().ToUpper()).SingleOrDefault();
                db.NewsLetters.Remove(newsLetter);
                db.SaveChanges();
                ViewBag.Result = "Success";
            }
            catch (Exception ex)
            {
                ViewBag.Result = ex.Message;
            }

            return View();
        }


        public ActionResult PaypalAccountGuide()
        {
            return View();
        }

        public ActionResult PaypalCreditCardGuide()
        {
            return View();
        }

        [Authorize(Roles = "Customer")]
        public ActionResult WishList(string pageSort, int? page, string pageSize, string selectedCategory,  string selectedNavCategory)
        {
         
            try
            {
        


                string CustomerId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
                var WistListIdList = db.WishLists.Where(w => w.CustomerId == CustomerId).Select(p=>p.ProductId).ToList();
                ViewBag.WistList = db.WishLists.Where(w => w.CustomerId == CustomerId).ToList();

                var products = db.Products.Include(p => p.Category).Include(p => p.Images);

                ViewBag.CurrentSort = pageSort;

                ViewBag.SelectedNavCategory = "Home";

                switch (pageSort)
                {
                    case "ProductName":
                        products = products.OrderBy(p => p.Name);
                        break;
                    case "CreatedDate":
                        products = products.OrderBy(p => p.CreatedDateTime);
                        break;
                    case "Popular":
                        products = products.OrderBy(p => p.Sold);
                        break;
                    default:
                        products = products.OrderBy(p => p.Name);
                        break;
                }



                // -- FILTER BY WISHLIST PRODUCTS ---
                products = products.Where(p => WistListIdList.Contains(p.ProductId));
                // -- END OF FILTER BY WISHLIST PRODUCTS ---

                ViewBag.SelectedCategory = selectedCategory;
      
                ViewBag.TotalResult = products.Count();

                int _pageSize = string.IsNullOrEmpty(pageSize) ? 20 : Int32.Parse(pageSize);
                ViewBag.CurrentItemsPerPage = _pageSize;
                int pageNumber = (page ?? 1);

                //ViewBag.WishListCount = WistListIdList.Count().ToString();


                return View(products.ToPagedList(pageNumber, _pageSize));

            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

        }

        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        public ActionResult ConfirmEmail(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var newsLetter = db.NewsLetters.Find(int.Parse(id));
            if(newsLetter != null)
            {
                newsLetter.IsComfirmed = true;
                db.Entry(newsLetter).State = EntityState.Modified;
                db.SaveChanges();

                HomeController homeController = new HomeController();
                homeController.HangFireSendEmail(newsLetter.Email);

                return View(newsLetter);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        private SmtpClient smtp = new SmtpClient();
        public ActionResult TermsAndCondition()
        {
            return View();
        }
        public ActionResult HangFireSendEmail(string newSubscriber = "")
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var randomProductIdList = db.Products
                       .SqlQuery("SELECT TOP 6 * FROM Products ORDER BY NEWID()")
                       .ToList<Product>().Select(p => p.ProductId).ToList();


            var products = db.Products.Include(p => p.Category).Include(p => p.Images).Where(p => randomProductIdList.Contains(p.ProductId));
            //var emailViewHtmlString = EmailUtility.ViewToStringRenderer.RenderViewToString(this.ControllerContext, "~/Views/Home/SendEmail.cshtml", products);
            List<NewsLetter> subscribers = new List<NewsLetter>();

            if(newSubscriber != "")
            {
                var newsLetter = new NewsLetter();
                newsLetter.Email = newSubscriber.Trim();
                subscribers.Add(newsLetter);
            }
            else
            {
                subscribers = db.NewsLetters.Where(n=>n.IsComfirmed == true).ToList();
            }
            
            // IF NO PRODUCTS RETURN NULL
            if (products.Count() == 0) return null;

            // IF NO SUBSCRIBER RETURN NULL
            if (subscribers.Count() ==0) return null;

            foreach (var subscriber in subscribers)
            {
                var emailViewHtmlString = CreateEmailHtmlTemplate(products, subscriber.Email);

                using (MailMessage msg = new MailMessage())
                {


                    HtmlAgilityPack.HtmlDocument htmldoc = new HtmlAgilityPack.HtmlDocument();
                    htmldoc.LoadHtml(emailViewHtmlString);


                    var imageTagNodes = (from action in htmldoc.DocumentNode.SelectNodes("//img").Cast<HtmlNode>()
                                         select action).ToList();


                    int imgIndex = 0;
                    foreach (var imgNode in imageTagNodes)
                    {

                        var src = imgNode.Attributes["src"].Value;
                        if (src != "" && !src.Contains("ci6.googleusercontent.com") && !src.Contains("ci3.googleusercontent.com") && !src.Contains("ci4.googleusercontent.com"))
                        {
                            imgIndex += 1;
                            emailViewHtmlString = emailViewHtmlString.Replace(src, "cid:img" + imgIndex.ToString());
                        }

                    }

                    // Create the HTML view
                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                                                                 emailViewHtmlString,
                                                                 Encoding.UTF8,
                                                                 MediaTypeNames.Text.Html);
                    // Create a plain text message for client that don't support HTML
                    AlternateView plainView = AlternateView.CreateAlternateViewFromString(
                                                                Regex.Replace(emailViewHtmlString,
                                                                              "<[^>]+?>",
                                                                              string.Empty),
                                                                Encoding.UTF8,
                                                                MediaTypeNames.Text.Plain);



                    imgIndex = 0;
                    foreach (var imgNode in imageTagNodes)
                    {

                        var src = imgNode.Attributes["src"].Value;
                        if (src != "" && !src.Contains("ci6.googleusercontent.com") && !src.Contains("ci3.googleusercontent.com") && !src.Contains("ci4.googleusercontent.com"))
                        {
                            imgIndex += 1;
                            string mediaType = MediaTypeNames.Image.Jpeg;

                            string path = System.Web.Hosting.HostingEnvironment.MapPath(src);

                            //string path = Server.MapPath(src); // my sample image is placed in images folder

                            LinkedResource img = new LinkedResource(path, mediaType);
                            img.ContentId = "img" + imgIndex.ToString();

                            img.ContentType.MediaType = mediaType;
                            img.TransferEncoding = TransferEncoding.Base64;
                            img.ContentType.Name = img.ContentId;
                            img.ContentLink = new Uri("cid:" + img.ContentId);
                            htmlView.LinkedResources.Add(img);
                        }
                    }


                    msg.From = new MailAddress("zaldys.ecommerce.demo@gmail.com", "Zaldy's Ecommerce");
                    //msg.Bcc.Add("zaldys.ecommerce.demo@gmail.com");
                    msg.To.Add(subscriber.Email);
                    msg.AlternateViews.Add(plainView);
                    msg.AlternateViews.Add(htmlView);

                    msg.IsBodyHtml = true;
                    msg.Subject = "Top Picks Of The Week! " + DateTime.Now.ToString("MMMM dd, yyyy");
                  

                    try
                    {
                        smtp = new SmtpClient();
                        lock (smtp)
                        {
                            smtp.Timeout = 980000;
                            smtp.Send(msg);
                        }
                    }
                    catch (Exception ex)
                    {
                        //CLEAR HANG FIRE RECURRINGJOBS
                        using (var connection = JobStorage.Current.GetConnection())
                        {
                            foreach (var recurringJob in connection.GetRecurringJobs())
                            {
                                RecurringJob.RemoveIfExists(recurringJob.Id);
                            }
                        }

                        HangfireUtility.ClearDatabase(db);
                        LogUtility.Write("Error", ex.Message, db);
                    }
                    finally
                    {
                        msg.Dispose();
                        smtp.Dispose();
                    }

                }
            }


            return Content("");
        }

        private static string Combine(string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');
            return string.Format("{0}/{1}", uri1, uri2);
        }

        //private  static string CreateEmailHtmlTemplate(IEnumerable<Product> products, string email)
        //{
        //    string baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
        //    StringBuilder sb = new StringBuilder();

        //    sb.AppendLine("World!");


        //    string htmlString = "";
        //    htmlString = "<html lang='en'>";
        //    htmlString += "<head>";
        //    htmlString += "    <title>Zaldy's Ecommerce</title>";
        //    htmlString += "</head>";
        //    htmlString += "<body>";

        //    htmlString += "    <table cellspacing='0' cellpadding='0' border='0' width='600' class='m_-4140746664795773952container' align='center'>";
        //    htmlString += "        <tbody>";
        //    htmlString += "           <tr>";
        //    htmlString += "                <td>";

        //    htmlString += "                    <table style=\"background-color:#ffffff;border:0px solid transparent;font-size:16px;font-family:Arial,helvetica,sans-serif;line-height:100%;color:#808080\"  cellspacing='0' cellpadding='0' bgcolor='#FFFFFF' width='100%'>";

        //    htmlString += "                        <tbody>";
        //    htmlString += "                            <tr>";
        //    htmlString += "                                <td align='center' valign='top'>";
        //    htmlString += "                                    <table align='left' border='0' cellpadding='0' cellspacing='0' width='100%'>";
        //    htmlString += "                                        <tbody>";
        //    htmlString += "                                            <tr>";

        //    htmlString += "                                                <td style='border:0px;padding:0px'>";

        //    htmlString += "                                                    <table border='0' cellpadding='0' cellspacing='0' width='100%'>";
        //    htmlString += "                                                        <tbody>";
        //    htmlString += "                                                            <tr>";
        //    htmlString += "                                                                <td align='left' class='m_-4140746664795773952header' valign='top'>";
        //    htmlString += "                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>";
        //    htmlString += "                                                                        <tbody>";
        //    htmlString += "                                                                            <tr>";
        //    htmlString += "                                                                               <td style='padding:0px 0px 10px'>";
        //    htmlString += "                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='background-color:transparent;min-width:100%;border-top:0px;border-right:0px;border-bottom:1px solid transparent;border-left:0px'>";
        //    htmlString += "                                                                                        <tbody>";
        //    htmlString += "                                                                                            <tr>";
        //    htmlString += "                                                                                                <td style='padding:0px'>";
        //    htmlString += "                                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>";
        //    htmlString += "                                                                                                        <tbody>";
        //    htmlString += "                                                                                                            <tr>";
        //    htmlString += "                                                                                                                <td style='padding:20px 0px 0px'>";
        //    htmlString += "                                                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='background-color:transparent;min-width:100%'>";
        //    htmlString += "                                                                                                                        <tbody>";
        //    htmlString += "                                                                                                                            <tr>";
        //    htmlString += "                                                                                                                                <td style='padding:0px'>";
        //    htmlString += "                                                                                                                                    <table width='100%' style='min-width:375px!important' cellspacing='0' cellpadding='0' align='center'>";
        //    htmlString += "                                                                                                                                        <tbody>";
        //    htmlString += "                                                                                                                                            <tr>";
        //    htmlString += "                                                                                                                                                <td width='100%' align='center'>";
        //    htmlString += "                                                                                                                                                    <a  target='_blank'  href='" + Combine(baseUrl, "/Home/Index") + "' width='1' height='1' class='CToWUd'><img src='~/FrontEnd/img/demo logo2.png' alt=\"Zaldy's Ecommerce\" style='display:block;height:126px;width:100%;text-align:center;padding:0px' width='100%' height='126' class='CToWUd'></a>";
        //    htmlString += "                                                                                                                                                </td>";
        //    htmlString += "                                                                                                                                            </tr>";
        //    htmlString += "                                                                                                                                        </tbody>";
        //    htmlString += "                                                                                                                                    </table>";
        //    htmlString += "                                                                                                                                    <img src='' width='1' height='1' class='CToWUd'>";
        //    htmlString += "                                                                                                                                </td>";
        //    htmlString += "                                                                                                                            </tr>";
        //    htmlString += "                                                                                                                        </tbody>";
        //    htmlString += "                                                                                                                    </table>";
        //    htmlString += "                                                                                                                </td>";
        //    htmlString += "                                                                                                            </tr>";
        //    htmlString += "                                                                                                        </tbody>";
        //    htmlString += "                                                                                                    </table><table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>";
        //    htmlString += "                                                                                                        <tbody>";
        //    htmlString += "                                                                                                            <tr>";
        //    htmlString += "                                                                                                                <td style='padding:5px 0px 0px'>";
        //    htmlString += "                                                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='background-color:transparent;min-width:100%'>";
        //    htmlString += "                                                                                                                        <tbody>";
        //    htmlString += "                                                                                                                            <tr>";
        //    htmlString += "                                                                                                                                <td style='padding:0px'>";
        //    htmlString += "                                                                                                                                    <div style=\"text-align:center;color:#000;font-family:'Roboto',arial,sans-serif\">";
        //    htmlString += "                                                                                                                                        <div style='width:100%'>";
        //    htmlString += "                                                                                                                                            <table align='center' border='0' cellpadding='5' cellspacing='0' style='width:600px'>";
        //    htmlString += "                                                                                                                                                <tbody>";
        //    htmlString += "                                                                                                                                                    <tr>";
        //    htmlString += "                                                                                                                                                        <td align='center'>";
        //    htmlString += "                                                                                                                                                            <span style='font-size:12px'><a href='" + Combine(baseUrl, "/Home/Store?selectedCategory=1&selectedNavCategory=Laptops") + "' style=\"font-weight:700;text-align:center;font-family:Roboto,arial,sans-serif;color:rgb(0,0,0);text-decoration:none\" title='' target='_blank' data-saferedirecturl='#'>LAPTOPS</a></span>";
        //    htmlString += "                                                                                                                                                        </td>";
        //    htmlString += "                                                                                                                                                        <td style='line-height:100%'>";
        //    htmlString += "                                                                                                                                                            <span style='font-size:12px'><span style=\"font-family:arial,helvetica,sans-serif\"><strong style='color:#000;text-align:center'>|</strong></span></span>";
        //    htmlString += "                                                                                                                                                        </td>";
        //    htmlString += "                                                                                                                                                        <td align='center'>";
        //    htmlString += "                                                                                                                                                           <span style='font-size:12px'><a href='" + Combine(baseUrl, "/Home/Store?selectedCategory=2&selectedNavCategory=Smartphones") + "' style=\"font-weight:700;text-align:center;font-family:Roboto,arial,sans-serif;color:rgb(0,0,0);text-decoration:none\" title='' target='_blank' data-saferedirecturl='#'>SMART PHONES</a></span>";
        //    htmlString += "                                                                                                                                                        </td>";
        //    htmlString += "                                                                                                                                                        <td style='line-height:100%'>";
        //    htmlString += "                                                                                                                                                            <span style='font-size:12px'><span style=\"font-family:arial,helvetica,sans-serif\"><strong style='color:#000;text-align:center'>|</strong></span></span>";
        //    htmlString += "                                                                                                                                                        </td>";
        //    htmlString += "                                                                                                                                                        <td align='center'>";
        //    htmlString += "                                                                                                                                                            <span style='font-size:12px'><a href='" + Combine(baseUrl, "/Home/Store?selectedCategory=3&selectedNavCategory=Accessories") + "' style=\"font-weight:700;text-align:center;font-family:Roboto,arial,sans-serif;color:rgb(0,0,0);text-decoration:none\" title='' target='_blank' data-saferedirecturl='#'>ACCESORIES</a></span>";
        //    htmlString += "                                                                                                                                                        </td>";
        //    htmlString += "                                                                                                                                                        <td style='line-height:100%'>";
        //    htmlString += "                                                                                                                                                            <span style='font-size:12px'><span style=\"font-family:arial,helvetica,sans-serif\"><strong style='color:#000;text-align:center'>|</strong></span></span>";
        //    htmlString += "                                                                                                                                                        </td>";
        //    htmlString += "                                                                                                                                                        <td align='center'>";
        //    htmlString += "                                                                                                                                                            <span style='font-size:12px'><a href='" + Combine(baseUrl, "/Home/Store?selectedCategory=4&selectedNavCategory=Headphones") + "' style=\"font-weight:700;text-align:center;font-family:Roboto,arial,sans-serif;color:rgb(0,0,0);text-decoration:none\" title='' target='_blank' data-saferedirecturl='#'>HEADPHONES</a></span>";
        //    htmlString += "                                                                                                                                                        </td>";
        //    htmlString += "                                                                                                                                                        <td style='line-height:100%'>";
        //    htmlString += "                                                                                                                                                            <span style='font-size:12px'><span style=\"font-family:arial,helvetica,sans-serif\"><strong style='color:#000;text-align:center'>|</strong></span></span>";
        //    htmlString += "                                                                                                                                                        </td>";
        //    htmlString += "                                                                                                                                                        <td align='center'>";
        //    htmlString += "                                                                                                                                                            <span style='font-size:12px'><a href='" + Combine(baseUrl, "/Home/Store?selectedCategory=6&selectedNavCategory=Tablet") + "' style=\"font-weight:700;text-align:center;font-family:Roboto,arial,sans-serif;color:rgb(0,0,0);text-decoration:none\" title='' target='_blank' data-saferedirecturl='#'>TABLETS</a></span>";
        //    htmlString += "                                                                                                                                                        </td>";
        //    htmlString += "                                                                                                                                                    </tr>";
        //    htmlString += "                                                                                                                                                </tbody>";
        //    htmlString += "                                                                                                                                            </table>";
        //    htmlString += "                                                                                                                                        </div>";
        //    htmlString += "                                                                                                                                    </div>";
        //    htmlString += "                                                                                                                                </td>";
        //    htmlString += "                                                                                                                            </tr>";
        //    htmlString += "                                                                                                                        </tbody>";
        //    htmlString += "                                                                                                                    </table>";
        //    htmlString += "                                                                                                                </td>";
        //    htmlString += "                                                                                                            </tr>";
        //    htmlString += "                                                                                                        </tbody>";
        //    htmlString += "                                                                                                    </table>";
        //    htmlString += "                                                                                                </td>";
        //    htmlString += "                                                                                            </tr>";
        //    htmlString += "                                                                                        </tbody>";
        //    htmlString += "                                                                                    </table>";
        //    htmlString += "                                                                                </td>";
        //    htmlString += "                                                                            </tr>";
        //    htmlString += "                                                                        </tbody>";
        //    htmlString += "                                                                    </table>";

        //    htmlString += "                                                                </td>";
        //    htmlString += "                                                            </tr>";
        //    htmlString += "                                                            <tr>";
        //    htmlString += "                                                                <td align='left' valign='top'>";
        //    htmlString += "                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='border:0px solid transparent;background-color:transparent;min-width:100%'>";
        //    htmlString += "                                                                        <tbody>";
        //    htmlString += "                                                                            <tr>";
        //    htmlString += "                                                                                <td style='padding:0px'>";

        //    htmlString += "                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>";
        //    htmlString += "                                                                                        <tbody>";
        //    htmlString += "                                                                                            <tr>";
        //    htmlString += "                                                                                                <td>";

        //    htmlString += "                                                                                                    <center>";
        //    htmlString += "                                                                                                        <table width='600px' cellspacing='0' cellpadding='0'>";
        //    htmlString += "                                                                                                            <tbody>";
        //    htmlString += "                                                                                                                <tr>";

        //    htmlString += "                                                                                                                    <td align='center' style='display:flex;'>";
        //    htmlString += "                                                                                                                        <h2 style='color: #d10024; display: block; background-color: #efefef; padding: 0px; margin: 0px; line-height: 90px; text-align: center; height: 90px; width: 100%; border: 0px'>TOP PICKS OF THE WEEK</h2><div class='a6S' dir='ltr' style='opacity: 0.01; left: 1004px; top: 1753px;'><div id=':p7' class='T-I J-J5-Ji aQv T-I-ax7 L3 a5q' title='Download' role='button' tabindex='0' aria-label='Download attachment ' data-tooltip-class='a1V'><div class='akn'><div class='aSK J-J5-Ji aYr'></div></div></div></div>";
        //    htmlString += "                                                                                                                    </td>";
        //    htmlString += "                                                                                                                </tr>";
        //    htmlString += "                                                                                                            </tbody>";
        //    htmlString += "                                                                                                        </table>";
        //    htmlString += "                                                                                                        <table border='0' width='600' cellspacing='0' cellpadding='0' bgcolor='#efefef'>";
        //    htmlString += "                                                                                                            <tbody>";


        //                                                                                                                        int prodCount = 0;
        //                                                                                                                       int prodIndex = 0;


        //                                                                                                                    foreach (var item in products)
        //                                                                                                                    {
        //                                                                                                                       prodCount += 1;
        //                                                                                                                        prodIndex += 1;

        //                                                                                                                        if (prodCount == 1)
        //                                                                                                                       {
        //    htmlString += "                                                                                                                        <tr>";

        //    htmlString += "                                                                                                                    <td bgcolor='#efefef' width='15px'>";
        //    htmlString += "                                                                                                                        &nbsp;";
        //    htmlString += "                                                                                                                    </td>";

        //    htmlString += "                                                                                                                    <td width='278'>";
        //    htmlString += "                                                                                                                        <!-- zaldy 1-->";
        //    htmlString += "                                                                                                                        <table border='0' cellspacing='0' cellpadding='0' bgcolor='white'>";
        //    htmlString += "                                                                                                                            <tbody>";
        //    htmlString += "                                                                                                                                <tr>";
        //    htmlString += "                                                                                                                                    <td colspan='2' width='278px' height='278px' style='padding:12px;'>";
        //    htmlString += "                                                                                                                                        <a href='" + Combine(baseUrl, "/Home/Product?productId=" + item.ProductId.ToString() + "&selectedNavCategory=Categories") + "' target='_blank' data-saferedirecturl='#'>";
        //                                                                                                                                               if (item.Images.Count > 0)
        //                                                                                                                                             {

        //                                                                                                                                                  var itemImages = item.Images;

        //                                                                                                                                                   foreach (var itemImage in itemImages)
        //                                                                                                                                                 {
        //                                                                                                                                                     if (itemImage.Default)
        //                                                                                                                                                        {

        //    htmlString += "                                                                                                                                                        <img style='width: 100%; height: 258px; object-fit: scale-down;' src='" + itemImage.ImagePath + "' alt='" + itemImage.Title + "' height='258px' class='CToWUd'>";
        //                                                                                                                                                          break;
        //                                                                                                                                                      }
        //                                                                                                                                                    }
        //                                                                                                                                          }

        //    htmlString += "                                                                                                                                        </a>";
        //    htmlString += "                                                                                                                                    </td>";
        //    htmlString += "                                                                                                                                </tr>";
        //    htmlString += "                                                                                                                                <tr>";
        //    htmlString += "                                                                                                                                    <td style='padding:8px' colspan='2' width='230'>";

        //    htmlString += "                                                                                                                                        <div style=\"float:left;color:#000000;font-weight:bold;font-family:'Roboto',arial,sans-serif!important;overflow:hidden;text-overflow:ellipsis;max-width:278px;height:32px;text-transform:capitalize;\">" + item.Name + "</div>";

        //    htmlString += "                                                                                                                                    </td>";
        //    htmlString += "                                                                                                                                </tr>";
        //    htmlString += "                                                                                                                                <tr>";


        //    htmlString += "                                                                                                                                    <!-- WITH PROMO -->";
        //    htmlString += "                                                                                                                                    <td style='padding:8px'>";

        //                                                                                                                                            if (item.PromoSaleOFF > 0)
        //                                                                                                                                         {
        //                                                                                                                                              if (DateTime.Now >= item.PromoSaleStartDateTime && DateTime.Now <= item.PromoSaleEndDateTime)
        //                                                                                                                                                {

        //    htmlString += "                                                                                                                                                <span  style=\"text-decoration:line-through;color:#bdbdbd;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.OriginalPrice).ToString("#,##0.00") + "</span>";
        //    htmlString += "                                                                                                                                                <br>";
        //    htmlString += "                                                                                                                                                <span  style=\"color:#D10024;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.DiscountedPrice).ToString("#,##0.00") + "</span>";


        //                                                                                                                                                }
        //                                                                                                                                               else
        //                                                                                                                                               {
        //    htmlString += "                                                                                                                                                <span  style=\"color:#D10024;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.OriginalPrice).ToString("#,##0.00") + "</span>";

        //                                                                                                                                                }
        //                                                                                                                                           }
        //                                                                                                                                          else
        //                                                                                                                                           {
        //    htmlString += "                                                                                                                                            <span  style=\"color:#D10024;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.OriginalPrice).ToString("#,##0.00") + "</span>";
        //                                                                                                                                           }


        //    htmlString += "                                                                                                                                        <br>";
        //    htmlString += "                                                                                                                                       <span  style=\"color:#000000;font-family:'Roboto',arial,sans-serif!important\">" + (item.Sold == null ? 0 : item.Sold)  + " sold</span>";
        //    htmlString += "                                                                                                                                    </td>";

        //                                                                                                                                       if (item.PromoSaleOFF > 0)
        //                                                                                                                                      {
        //                                                                                                                                           if (DateTime.Now >= item.PromoSaleStartDateTime && DateTime.Now <= item.PromoSaleEndDateTime)
        //                                                                                                                                           {

        //    htmlString += "                                                                                                                                            <td style='padding:8px' align='right' valign='bottom' width='45px'>";

        //    htmlString += "                                                                                                                                                <div style='padding-top: 5px; padding-bottom: 5px; background-color: #D10024; text-align: center'>";
        //    htmlString += "                                                                                                                                                    <span  style=\"color:#ffffff;font-family:'Roboto',arial,sans-serif!important\">" + (double.Parse(item.PromoSaleOFF.ToString()) * 100) + "</span>";
        //    htmlString += "                                                                                                                                                    <span  style=\"color: #ffffff; font-family: 'Roboto',arial,sans-serif !important \">%</span>";
        //    htmlString += "                                                                                                                                                    <br>";
        //    htmlString += "                                                                                                                                                    <span style=\"color:#ffffff;font-family:'Roboto',arial,sans-serif!important\">OFF</span>";
        //    htmlString += "                                                                                                                                                </div>";

        //    htmlString += "                                                                                                                                            </td>";

        //                                                                                                                                            }

        //                                                                                                                                        }
        //                                                                                                                                       else
        //                                                                                                                                       {
        //    htmlString += "                                                                                                                                        <!-- NO PROMO -->";
        //    htmlString += "                                                                                                                                        <td style='padding:8px'>";
        //    htmlString += "                                                                                                                                        </td>";
        //                                                                                                                                       }


        //    htmlString += "                                                                                                                                </tr>";
        //    htmlString += "                                                                                                                            </tbody>";
        //    htmlString += "                                                                                                                        </table>";
        //    htmlString += "                                                                                                                    </td>";
        //                                                                                                                     }

        //                                                                                                                    if (prodCount == 2)
        //                                                                                                                    {

        //    htmlString += "                                                                                                                    <td bgcolor='#efefef' width='14'>";
        //    htmlString += "                                                                                                                        &nbsp;";
        //    htmlString += "                                                                                                                    </td>";

        //    htmlString += "                                                                                                                    <td width='278'>";
        //    htmlString += "                                                                                                                        <!-- zaldy 1-->";
        //    htmlString += "                                                                                                                        <table border='0' cellspacing='0' cellpadding='0' bgcolor='white'>";
        //    htmlString += "                                                                                                                            <tbody>";
        //    htmlString += "                                                                                                                                <tr>";
        //    htmlString += "                                                                                                                                    <td colspan='2' width='278px' height='278px' style='padding:12px;'>";
        //    htmlString += "                                                                                                                                        <a href='" + Combine(baseUrl, "/Home/Product?productId=" + item.ProductId.ToString()  + "&selectedNavCategory=Categories") + "' target='_blank' data-saferedirecturl='#'>";
        //                                                                                                                                              if (item.Images.Count > 0)
        //                                                                                                                                               {

        //                                                                                                                                                   var itemImages = item.Images;

        //                                                                                                                                                    foreach (var itemImage in itemImages)
        //                                                                                                                                                   {
        //                                                                                                                                                       if (itemImage.Default)
        //                                                                                                                                                       {

        //    htmlString += "                                                                                                                                                        <img style='width: 100%; height: 258px; object-fit: scale-down;' src='" + itemImage.ImagePath + "' alt='" + itemImage.Title + "' height='258px'>";
        //                                                                                                                                                           break;
        //                                                                                                                                                      }
        //                                                                                                                                                   }
        //                                                                                                                                               }

        //    htmlString += "                                                                                                                                        </a>";
        //    htmlString += "                                                                                                                                    </td>";
        //    htmlString += "                                                                                                                               </tr>";
        //    htmlString += "                                                                                                                                <tr>";
        //    htmlString += "                                                                                                                                    <td style='padding:8px' colspan='2' width='230'>";

        //    htmlString += "                                                                                                                                       <div style=\"float:left;color:#000000;font-weight:bold;font-family:'Roboto',arial,sans-serif!important;overflow:hidden;text-overflow:ellipsis;max-width:278px;height:32px;text-transform:capitalize;\">" + item.Name + "</div>";

        //    htmlString += "                                                                                                                                    </td>";
        //    htmlString += "                                                                                                                                </tr>";
        //    htmlString += "                                                                                                                               <tr>";


        //    htmlString += "                                                                                                                                    <!-- WITH PROMO -->";
        //    htmlString += "                                                                                                                                    <td style='padding:8px'>";

        //                                                                                                                                          if (item.PromoSaleOFF > 0)
        //                                                                                                                                          {
        //                                                                                                                                              if (DateTime.Now >= item.PromoSaleStartDateTime && DateTime.Now <= item.PromoSaleEndDateTime)
        //                                                                                                                                               {

        //    htmlString += "                                                                                                                                                <span  style=\"text-decoration:line-through;color:#bdbdbd;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.OriginalPrice).ToString("#,##0.00") + "</span>";
        //    htmlString += "                                                                                                                                               <br>";
        //    htmlString += "                                                                                                                                                <span  style=\"color:#D10024;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.DiscountedPrice).ToString("#,##0.00") + "</span>";


        //                                                                                                                                                }
        //                                                                                                                                                else
        //                                                                                                                                               {
        //    htmlString += "                                                                                                                                                <span  style=\"color:#D10024;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.OriginalPrice).ToString("#,##0.00") + "</span>";

        //                                                                                                                                                }
        //                                                                                                                                            }
        //                                                                                                                                            else
        //                                                                                                                                            {
        //    htmlString += "                                                                                                                                            <span  style=\"color:#D10024;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.OriginalPrice).ToString("#,##0.00") + "</span>";
        //                                                                                                                                           }


        //    htmlString += "                                                                                                                                        <br>";
        //    htmlString += "                                                                                                                                        <span style=\"color:#000000;font-family:'Roboto',arial,sans-serif!important\">" + (item.Sold == null ? 0 : item.Sold) + " sold</span>";
        //    htmlString += "                                                                                                                                    </td>";

        //                                                                                                                                       if (item.PromoSaleOFF > 0)
        //                                                                                                                                        {
        //                                                                                                                                            if (DateTime.Now >= item.PromoSaleStartDateTime && DateTime.Now <= item.PromoSaleEndDateTime)
        //                                                                                                                                            {

        //    htmlString += "                                                                                                                                            <td style='padding:8px' align='right' valign='bottom' width='45px'>";

        //    htmlString += "                                                                                                                                                <div style='padding-top: 5px; padding-bottom: 5px; background-color: #D10024; text-align: center'>";
        //    htmlString += "                                                                                                                                                    <span style=\"color:#ffffff;font-family:'Roboto',arial,sans-serif!important\">" + (double.Parse(item.PromoSaleOFF.ToString()) * 100)  + "</span>";
        //    htmlString += "                                                                                                                                                    <span  style=\"color: #ffffff; font-family: 'Roboto',arial,sans-serif !important \">%</span>";
        //    htmlString += "                                                                                                                                                    <br>";
        //    htmlString += "                                                                                                                                                    <span  style=\"color:#ffffff;font-family:'Roboto',arial,sans-serif!important\">OFF</span>";
        //    htmlString += "                                                                                                                                                </div>";

        //    htmlString += "                                                                                                                                            </td>";

        //                                                                                                                                            }

        //                                                                                                                                        }
        //                                                                                                                                        else
        //                                                                                                                                        {
        //    htmlString += "                                                                                                                                        <!-- NO PROMO -->";
        //    htmlString += "                                                                                                                                        <td style='padding:8px'>";
        //    htmlString += "                                                                                                                                        </td>";
        //                                                                                                                                       }


        //    htmlString += "                                                                                                                                </tr>";
        //    htmlString += "                                                                                                                            </tbody>";
        //    htmlString += "                                                                                                                        </table>";
        //    htmlString += "                                                                                                                    </td>";
        //    htmlString += "                                                                                                                    <td bgcolor='#efefef' width='15'>";
        //    htmlString += "                                                                                                                        &nbsp;";
        //    htmlString += "                                                                                                                    </td>";


        //    htmlString += "                                                                                                                   </tr><tr bgcolor='#efefef'><td colspan='3' bgcolor='#efefef' height='15px'>&nbsp;</td></tr>'";


        //                                                                                                                        prodCount = 0;
        //                                                                                                                   }



        //                                                                                                                    if ((products.Count() % 2) > 0 && prodIndex == products.Count())
        //                                                                                                                    {
        //    htmlString += "                                                                                                                    </tr><tr bgcolor='#efefef'><td colspan='3' bgcolor='#efefef' height='15px'>&nbsp;</td></tr>";
        //                                                                                                                    }

        //                                                                                                                }

        //    htmlString += "</tbody>";
        //    htmlString += "                                                                                                        </table>";
        //    htmlString += "                                                                                                        <table width='600px' cellspacing='0' cellpadding='0' bgcolor='#efefef'>";
        //    htmlString += "                                                                                                            <tbody>";
        //    htmlString += "                                                                                                                <tr>";
        //    htmlString += "                                                                                                                    <td align='center' style='line-height:100px;height:100px;'>";
        //    htmlString += "                                                                                                                        <a href='" + Combine(baseUrl, "/Home/Store?selectedCategory=1%2C2%2C3%2C4%2C5%2C6&selectedNavCategory=Categories") + "' target='_blank' data-saferedirecturl='#'><img src='~/FrontEnd/img/shop now button.png' width='194' style='display:block;text-align:center;height:auto;width:194px;border:0px' class='CToWUd'></a>";
        //    htmlString += "                                                                                                                    </td>";
        //    htmlString += "                                                                                                                </tr>";
        //    htmlString += "                                                                                                            </tbody>";
        //    htmlString += "                                                                                                        </table>";

        //    htmlString += "                                                                                                        <table>";
        //    htmlString += "                                                                                                            <tbody>";
        //    htmlString += "                                                                                                                <tr>";
        //    htmlString += "                                                                                                                    <td>";
        //    htmlString += "                                                                                                                        <table cellpadding='0' cellspacing='0' border='0' align='center' width='594'>";
        //    htmlString += "                                                                                                                            <tbody>";
        //    htmlString += "                                                                                                                                <tr>";
        //    htmlString += "                                                                                                                                    <td cellpadding='0' cellspacing='0' border='0' height='1' style='line-height:1px;min-width:198px'>";
        //    htmlString += "                                                                                                                                        <img src='https://ci6.googleusercontent.com/proxy/UdD10b8OwAMsIskWs35kVksPpJn8Z8NNDQtpw7bGuUfndy5BBB668zFGfSCq9A8cZzBIS2GRgAc3V6hkoUm_YW9wUZSQOPyE0OxEg-vIlRo1PlfvsCydU1P7HB74FfFyhiuIe02kACyfdmCHRsq5RkvllmTQJQDvftAw-Q=s0-d-e1-ft#https://image.email.shopee.co.id/lib/fe3a15707564067d761279/m/11/82629a96-3443-4ffb-978e-e261f6d86cb8.png' width='200' height='1' style='display:block;max-height:1px;min-height:1px;min-width:120px;width:120px' class='CToWUd'>";
        //    htmlString += "                                                                                                                                    </td>";
        //    htmlString += "                                                                                                                                    <td cellpadding='0' cellspacing='0' border='0' height='1' style='line-height:1px;min-width:198px'>";
        //    htmlString += "                                                                                                                                        <img src='https://ci6.googleusercontent.com/proxy/UdD10b8OwAMsIskWs35kVksPpJn8Z8NNDQtpw7bGuUfndy5BBB668zFGfSCq9A8cZzBIS2GRgAc3V6hkoUm_YW9wUZSQOPyE0OxEg-vIlRo1PlfvsCydU1P7HB74FfFyhiuIe02kACyfdmCHRsq5RkvllmTQJQDvftAw-Q=s0-d-e1-ft#https://image.email.shopee.co.id/lib/fe3a15707564067d761279/m/11/82629a96-3443-4ffb-978e-e261f6d86cb8.png' width='200' height='1' style='display:block;max-height:1px;min-height:1px;min-width:198px;width:198px' class='CToWUd'>";
        //    htmlString += "                                                                                                                                    </td>";
        //    htmlString += "                                                                                                                                    <td cellpadding='0' cellspacing='0' border='0' height='1' style='line-height:1px;min-width:198px'>";
        //    htmlString += "                                                                                                                                        <img src='https://ci6.googleusercontent.com/proxy/UdD10b8OwAMsIskWs35kVksPpJn8Z8NNDQtpw7bGuUfndy5BBB668zFGfSCq9A8cZzBIS2GRgAc3V6hkoUm_YW9wUZSQOPyE0OxEg-vIlRo1PlfvsCydU1P7HB74FfFyhiuIe02kACyfdmCHRsq5RkvllmTQJQDvftAw-Q=s0-d-e1-ft#https://image.email.shopee.co.id/lib/fe3a15707564067d761279/m/11/82629a96-3443-4ffb-978e-e261f6d86cb8.png' width='200' height='1' style='display:block;max-height:1px;min-height:1px;min-width:198px;width:198px' class='CToWUd'>";
        //    htmlString += "                                                                                                                                    </td>";
        //    htmlString += "                                                                                                                                </tr>";
        //    htmlString += "                                                                                                                           </tbody>";
        //    htmlString += "                                                                                                                        </table>";
        //    htmlString += "                                                                                                                    </td>";
        //    htmlString += "                                                                                                                </tr>";
        //    htmlString += "                                                                                                            </tbody>";
        //    htmlString += "                                                                                                        </table>";
        //    htmlString += "                                                                                                    </center>";
        //    htmlString += "                                                                                                </td>";
        //    htmlString += "                                                                                            </tr>";
        //    htmlString += "                                                                                        </tbody>";
        //    htmlString += "                                                                                    </table>";
        //    htmlString += "                                                                                </td>";
        //    htmlString += "                                                                            </tr>";
        //    htmlString += "                                                                        </tbody>";
        //    htmlString += "                                                                    </table>";

        //    htmlString += "                                                                </td>";
        //    htmlString += "                                                            </tr>";
        //    htmlString += "                                                            <tr>";
        //    htmlString += "                                                                <td align='left' valign='top'>";
        //    htmlString += "                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>";
        //    htmlString += "                                                                        <tbody>";
        //    htmlString += "                                                                            <tr>";
        //    htmlString += "                                                                                <td style='padding:10px 0px 0px'>";
        //    htmlString += "                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='border:0px;background-color:transparent;min-width:100%'>";
        //    htmlString += "                                                                                        <tbody>";
        //    htmlString += "                                                                                            <tr>";
        //    htmlString += "                                                                                                <td style='padding:0px'>";
        //    htmlString += "                                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='background-color:transparent;min-width:100%'>";
        //    htmlString += "                                                                                                        <tbody>";
        //    htmlString += "                                                                                                            <tr>";
        //    htmlString += "                                                                                                                <td style='padding:0px'>";
        //    htmlString += "                                                                                                                    <table width='100%' cellspacing='0' cellpadding='0' role='presentation'>";
        //    htmlString += "                                                                                                                        <tbody>";
        //    htmlString += "                                                                                                                            <tr>";
        //    htmlString += "                                                                                                                                <td align='center' style='padding:0px;margin:0p;'>";
        //    htmlString += "                                                                                                                                    <a href='" + Combine(baseUrl, "/home/contact") + "' title='' target='_blank' data-saferedirecturl='#'><img src='~/FrontEnd/img/help center button.png' alt='Helpcenter' height='60' width='603' style='display:block;padding:0px;text-align:center;height:60px;width:603px;border:0px' class='CToWUd'></a>";
        //    htmlString += "                                                                                                                                </td>";
        //    htmlString += "                                                                                                                            </tr>";
        //    htmlString += "                                                                                                                        </tbody>";
        //    htmlString += "                                                                                                                    </table>";
        //    htmlString += "                                                                                                                </td>";
        //    htmlString += "                                                                                                            </tr>";
        //    htmlString += "                                                                                                        </tbody>";
        //    htmlString += "                                                                                                    </table><table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>";
        //    htmlString += "                                                                                                        <tbody>";
        //    htmlString += "                                                                                                            <tr>";
        //    htmlString += "                                                                                                                <td>";
        //    htmlString += "                                                                                                                    <table width='100%' align='center' style='width:100%!important' cellspacing='0' cellpadding='0'>";
        //    htmlString += "                                                                                                                        <tbody>";

        //    htmlString += "                                                                                                                            <tr>";
        //    htmlString += "                                                                                                                                <td width='100%' align='center' style='border-top:1px solid #f3f3f3;border-bottom:1px solid #f3f3f3;padding:25px 0'>";
        //    htmlString += "                                                                                                                                    <table width='30%' align='center' style='margin:0 auto' cellspacing='0' cellpadding='10'>";
        //    htmlString += "                                                                                                                                        <tbody>";
        //    htmlString += "                                                                                                                                            <tr>";
        //    htmlString += "                                                                                                                                                <td style='width:33.3%;padding-left:5px;padding-right:5px;text-align:center'>";

        //    htmlString += "                                                                                                                                                    <a href='https://www.facebook.com/' title='' target='_blank' data-saferedirecturl='#'><img src='https://ci6.googleusercontent.com/proxy/nwYp_9RmF7myx91ko4jV8qr2OwvKWxSDaI09WRA7Pp-SlHyZtUY8zKEFRukd20-HuDQ2eRtEvIIhwQe7W2g1VcW0oT_y5-hKX4v28TV8ddCwYSP05dY8TA2vtFMkF45kx25DfB4rKBesGylyXqb6LhyYdgCV8ncA=s0-d-e1-ft#https://image.email.shopee.sg/lib/fe3b15707564067d761278/m/1/cd8ee4c2-e30f-41a3-ad6f-b720f33342f0.png' alt='Facebook' height='50' width='50' style='display:block;padding:0px;text-align:center;height:50px;width:50px;border:0px none transparent' class='CToWUd'></a>";
        //    htmlString += "                                                                                                                                                </td>";
        //    htmlString += "                                                                                                                                                <td style='width:33.3%;padding-left:5px;padding-right:5px;text-align:center'>";

        //    htmlString += "                                                                                                                                                    <a href='https://www.instagram.com/' title='' target='_blank' data-saferedirecturl='#'><img src='https://ci3.googleusercontent.com/proxy/3lxVMGF5Zll_k3AKxaFnhn55O30XYgq3uDsmEVveE3lxEqVVQt63ELmsoOsAffw_wUfzXDejnyLb_CuXvTLduIciMm3l2E-9syXAK-Awhq4GmDv97Z1LChI3Co6YZu6FQrdRYtOTrVQ1CQ4khvOZTMtXjGCnXvKq=s0-d-e1-ft#https://image.email.shopee.sg/lib/fe3b15707564067d761278/m/1/4ab223ec-7fc6-4417-b514-62b365329e6a.png' alt='Instagram' height='50' width='50' style='display:block;padding:0px;text-align:center;height:50px;width:50px;border:0px none transparent' class='CToWUd'></a>";
        //    htmlString += "                                                                                                                                                </td>";
        //    htmlString += "                                                                                                                                                <td style='width:33.3%;padding-left:5px;padding-right:5px;text-align:center'>";

        //    htmlString += "                                                                                                                                                    <a href='https://twitter.com/' title='' target='_blank' data-saferedirecturl='#'><img src='https://ci4.googleusercontent.com/proxy/Y9v_DSD-ovdabCPxa29_eef54Z3Q0BxHnFAVb5B24vTLWKGGvRfPZzErwuuIXDFXnwW7a5Pny5qKzCeXRw1ZrvWj22VJkiErPvy_pqyfI-mS6h0E8NxjQjx4965aXXwjFOSSOhu9ro5wxhYT-kgZL5-vU_9jGfLCUwaJhw=s0-d-e1-ft#https://image.S10.exacttarget.com/lib/fe34157075640574731c72/m/1/801a8ba9-cd54-4786-bbde-dedb60f89cdc.png' alt='Twitter' height='50' width='50' style='display:block;padding:0px;text-align:center;height:50px;width:50px;border:0px none transparent' class='CToWUd'></a>";
        //    htmlString += "                                                                                                                                                </td>";
        //    htmlString += "                                                                                                                                            </tr>";
        //    htmlString += "                                                                                                                                        </tbody>";
        //    htmlString += "                                                                                                                                    </table>";
        //    htmlString += "                                                                                                                                </td>";
        //    htmlString += "                                                                                                                            </tr>";
        //    htmlString += "                                                                                                                        </tbody>";
        //    htmlString += "                                                                                                                    </table>";
        //    htmlString += "                                                                                                                    <table style='font-size:10px;width:100%;display:none' width='100%' cellspacing='0' cellpadding='0'>";
        //    htmlString += "                                                                                                                        <tbody>";
        //    htmlString += "                                                                                                                            <tr>";
        //    htmlString += "                                                                                                                                <td align='left'>";
        //    htmlString += "                                                                                                                                    Subscription Center: <a href='https://www.facebook.com/' target='_blank' data-saferedirecturl='https://www.google.com/url?q=https://www.facebook.com/'><wbr>subscription_center.aspx?qs=<wbr>d9490f04e554b8613f1c65699125a5<wbr>22c759326db715a238a9eaac8d208b<wbr>24f928a0336226e997ff6e12da60f2<wbr>d2d387</a>";
        //    htmlString += "                                                                                                                                    Profile Center: <a href='https://www.instagram.com/' target='_blank' data-saferedirecturl='https://www.google.com/url?q=https://www.instagram.com/'><wbr>profile_center.aspx?qs=<wbr>d9490f04e554b861049a956a7600b5<wbr>7fd382adefbcb9f4aed597e91da242<wbr>cf5b2eb873fbd2b99ac6b6a2dafa5a<wbr>fceb41</a>";
        //    htmlString += "                                                                                                                                    One-click Unsubscribe: <a href='https://twitter.com/' target='_blank' data-saferedirecturl='https://www.google.com/url?q=https://twitter.com/'><wbr>unsub_center.aspx?qs=<wbr>d9490f04e554b8613f1c65699125a5<wbr>22c759326db715a238a9eaac8d208b<wbr>24f95e9668d5464f40ce0d5dbfcaf6<wbr>77f6bb09438b02330b1a91</a>";
        //    htmlString += "                                                                                                                                    Sender address: Shopee Singapore Private Limited 2 Science Park Drive, #03-01, Tower A Ascent Singapore SG 118222 SG";
        //    htmlString += "                                                                                                                                </td>";
        //    htmlString += "                                                                                                                            </tr>";
        //    htmlString += "                                                                                                                        </tbody>";
        //    htmlString += "                                                                                                                    </table>";
        //    htmlString += "                                                                                                                </td>";
        //    htmlString += "                                                                                                            </tr>";
        //    htmlString += "                                                                                                        </tbody>";
        //    htmlString += "                                                                                                   </table>";
        //    htmlString += "                                                                                                </td>";
        //    htmlString += "                                                                                            </tr>";
        //    htmlString += "                                                                                        </tbody>";
        //    htmlString += "                                                                                    </table>";
        //    htmlString += "                                                                                </td>";
        //    htmlString += "                                                                            </tr>";
        //    htmlString += "                                                                        </tbody>";
        //    htmlString += "                                                                    </table>";

        //    htmlString += "                                                                </td>";
        //    htmlString += "                                                            </tr>";
        //    htmlString += "                                                            <tr>";
        //    htmlString += "                                                                <td align='left' valign='top'>";
        //    htmlString += "                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='background-color:transparent;min-width:100%'>";
        //    htmlString += "                                                                        <tbody>";
        //    htmlString += "                                                                            <tr>";
        //    htmlString += "                                                                                <td style='padding:0px'>";
        //    htmlString += "                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>";
        //    htmlString += "                                                                                        <tbody>";
        //    htmlString += "                                                                                            <tr>";
        //    htmlString += "                                                                                                <td>";
        //    htmlString += "                                                                                                    <div style=\"text-align:center;color:#858585;font-family:'Roboto',arial,sans-serif;font-size:12px;line-height:2\">";
        //    htmlString += "                                                                                                        <span style='text-decoration:underline'><a href='" + Combine(baseUrl, "/Home/PrivacyPolicy") + "' style='color:#858585;text-decoration:underline' title='' target='_blank' data-saferedirecturl='#'>Privacy Notice</a></span> &nbsp;&nbsp; |&nbsp;&nbsp; <span style='text-decoration:underline'><a href='" + Combine(baseUrl, "/Home/TermsAndCondition" ) + "' style='color:#858585;text-decoration:underline' title='' target='_blank' data-saferedirecturl='#'>Terms and Conditions</a></span> &nbsp;&nbsp; |&nbsp;&nbsp; <span style='text-decoration:underline'>";
        //    htmlString += "                                                                                                            <a href='#' style='color:#808080;text-decoration:underline;white-space:nowrap' title='' target='_blank' data-saferedirecturl='#'>View on Browser</a>";
        //    htmlString += "                                                                                                        </span> &nbsp;&nbsp; &nbsp;|&nbsp;&nbsp; <span style='text-decoration:underline'>";
        //    htmlString += "                                                                                                            <a href='" + Combine(baseUrl, "/Home/Unsubscribe?email=" + email) + "' style='color:#858585;text-decoration:underline' title='' target='_blank' data-saferedirecturl='#'>Unsubscribe</a>";
        //    htmlString += "                                                                                                        </span>";
        //    htmlString += "                                                                                                    </div>";
        //    htmlString += "                                                                                                </td>";
        //    htmlString += "                                                                                            </tr>";
        //    htmlString += "                                                                                        </tbody>";
        //    htmlString += "                                                                                    </table><table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>";
        //    htmlString += "                                                                                        <tbody>";
        //    htmlString += "                                                                                            <tr>";
        //    htmlString += "                                                                                                <td>";
        //    htmlString += "                                                                                                    <div style=\"text-align:center;color:#858585;font-size:12px;font-family:'Roboto',arial,sans-serif;text-decoration:none\">";
        //    htmlString += "                                                                                                        <p style='text-decoration:none'>";
        //    htmlString += "                                                                                                            <span style='text-decoration:none'>";
        //    htmlString += "                                                                                                                This is a post-only mailing.&nbsp;<br>";
        //    htmlString += "                                                                                                                Add <a href='mailto:zaldys.ecommerce.demo@gmail.com' style='color:#858585;text-decoration:none' target='_blank'>zaldys.ecommerce.demo@gmail.com</a> to your address book to ensure our e-mails enter your inbox.";
        //    htmlString += "                                                                                                            </span>";
        //    htmlString += "                                                                                                        </p><p style='text-decoration:none'>";
        //    htmlString += "                                                                                                            Zaldy's Ecommerce Philippines Inc., Malolos City, Bulacan";
        //    htmlString += "                                                                                                        </p>";
        //    htmlString += "                                                                                                    </div>";
        //    htmlString += "                                                                                                </td>";
        //    htmlString += "                                                                                            </tr>";
        //    htmlString += "                                                                                        </tbody>";
        //    htmlString += "                                                                                    </table>";
        //    htmlString += "                                                                                </td>";
        //    htmlString += "                                                                            </tr>";
        //    htmlString += "                                                                        </tbody>";
        //    htmlString += "                                                                    </table>";

        //    htmlString += "                                                                </td>";
        //    htmlString += "                                                            </tr>";
        //    htmlString += "                                                        </tbody>";
        //    htmlString += "                                                    </table>";
        //    htmlString += "                                                </td>";
        //    htmlString += "                                            </tr>";
        //    htmlString += "                                        </tbody>";
        //    htmlString += "                                    </table>";
        //    htmlString += "                                </td>";
        //    htmlString += "                            </tr>";
        //    htmlString += "                        </tbody>";
        //    htmlString += "                    </table>";
        //    htmlString += "                </td>";
        //    htmlString += "            </tr>";
        //    htmlString += "        </tbody>";
        //    htmlString += "    </table>";

        //    htmlString += "</body>";
        //    htmlString += "</html>";


        //    return htmlString;
        //}


        private static string CreateEmailHtmlTemplate(IEnumerable<Product> products, string email)
        {
            string baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<html lang='en'>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <title>Zaldy's Ecommerce</title>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            sb.AppendLine("    <table cellspacing='0' cellpadding='0' border='0' width='600' class='m_-4140746664795773952container' align='center'>");
            sb.AppendLine("        <tbody>");
            sb.AppendLine("           <tr>");
            sb.AppendLine("                <td>");

            sb.AppendLine("                    <table style=\"background-color:#ffffff;border:0px solid transparent;font-size:16px;font-family:Arial,helvetica,sans-serif;line-height:100%;color:#808080\"  cellspacing='0' cellpadding='0' bgcolor='#FFFFFF' width='100%'>");

            sb.AppendLine("                        <tbody>");
            sb.AppendLine("                            <tr>");
            sb.AppendLine("                                <td align='center' valign='top'>");
            sb.AppendLine("                                    <table align='left' border='0' cellpadding='0' cellspacing='0' width='100%'>");
            sb.AppendLine("                                        <tbody>");
            sb.AppendLine("                                            <tr>");

            sb.AppendLine("                                                <td style='border:0px;padding:0px'>");

            sb.AppendLine("                                                    <table border='0' cellpadding='0' cellspacing='0' width='100%'>");
            sb.AppendLine("                                                        <tbody>");
            sb.AppendLine("                                                            <tr>");
            sb.AppendLine("                                                                <td align='left' class='m_-4140746664795773952header' valign='top'>");
            sb.AppendLine("                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>");
            sb.AppendLine("                                                                        <tbody>");
            sb.AppendLine("                                                                            <tr>");
            sb.AppendLine("                                                                               <td style='padding:0px 0px 10px'>");
            sb.AppendLine("                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='background-color:transparent;min-width:100%;border-top:0px;border-right:0px;border-bottom:1px solid transparent;border-left:0px'>");
            sb.AppendLine("                                                                                        <tbody>");
            sb.AppendLine("                                                                                            <tr>");
            sb.AppendLine("                                                                                                <td style='padding:0px'>");
            sb.AppendLine("                                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>");
            sb.AppendLine("                                                                                                        <tbody>");
            sb.AppendLine("                                                                                                            <tr>");
            sb.AppendLine("                                                                                                                <td style='padding:20px 0px 0px'>");
            sb.AppendLine("                                                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='background-color:transparent;min-width:100%'>");
            sb.AppendLine("                                                                                                                        <tbody>");
            sb.AppendLine("                                                                                                                            <tr>");
            sb.AppendLine("                                                                                                                                <td style='padding:0px'>");
            sb.AppendLine("                                                                                                                                    <table width='100%' style='min-width:375px!important' cellspacing='0' cellpadding='0' align='center'>");
            sb.AppendLine("                                                                                                                                        <tbody>");
            sb.AppendLine("                                                                                                                                            <tr>");
            sb.AppendLine("                                                                                                                                                <td width='100%' align='center'>");
            sb.AppendLine("                                                                                                                                                    <a  target='_blank'  href='" + Combine(baseUrl, "/Home/Index") + "' width='1' height='1' class='CToWUd'><img src='~/FrontEnd/img/demo logo2.png' alt=\"Zaldy's Ecommerce\" style='display:block;height:126px;width:100%;text-align:center;padding:0px' width='100%' height='126' class='CToWUd'></a>");
            sb.AppendLine("                                                                                                                                                </td>");
            sb.AppendLine("                                                                                                                                            </tr>");
            sb.AppendLine("                                                                                                                                        </tbody>");
            sb.AppendLine("                                                                                                                                    </table>");
            sb.AppendLine("                                                                                                                                    <img src='' width='1' height='1' class='CToWUd'>");
            sb.AppendLine("                                                                                                                                </td>");
            sb.AppendLine("                                                                                                                            </tr>");
            sb.AppendLine("                                                                                                                        </tbody>");
            sb.AppendLine("                                                                                                                    </table>");
            sb.AppendLine("                                                                                                                </td>");
            sb.AppendLine("                                                                                                            </tr>");
            sb.AppendLine("                                                                                                        </tbody>");
            sb.AppendLine("                                                                                                    </table><table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>");
            sb.AppendLine("                                                                                                        <tbody>");
            sb.AppendLine("                                                                                                            <tr>");
            sb.AppendLine("                                                                                                                <td style='padding:5px 0px 0px'>");
            sb.AppendLine("                                                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='background-color:transparent;min-width:100%'>");
            sb.AppendLine("                                                                                                                        <tbody>");
            sb.AppendLine("                                                                                                                            <tr>");
            sb.AppendLine("                                                                                                                                <td style='padding:0px'>");
            sb.AppendLine("                                                                                                                                    <div style=\"text-align:center;color:#000;font-family:'Roboto',arial,sans-serif\">");
            sb.AppendLine("                                                                                                                                        <div style='width:100%'>");
            sb.AppendLine("                                                                                                                                            <table align='center' border='0' cellpadding='5' cellspacing='0' style='width:600px'>");
            sb.AppendLine("                                                                                                                                                <tbody>");
            sb.AppendLine("                                                                                                                                                    <tr>");
            sb.AppendLine("                                                                                                                                                        <td align='center'>");
            sb.AppendLine("                                                                                                                                                            <span style='font-size:12px'><a href='" + Combine(baseUrl, "/Home/Store?selectedCategory=1&selectedNavCategory=Laptops") + "' style=\"font-weight:700;text-align:center;font-family:Roboto,arial,sans-serif;color:rgb(0,0,0);text-decoration:none\" title='' target='_blank' data-saferedirecturl='#'>LAPTOPS</a></span>");
            sb.AppendLine("                                                                                                                                                        </td>");
            sb.AppendLine("                                                                                                                                                        <td style='line-height:100%'>");
            sb.AppendLine("                                                                                                                                                            <span style='font-size:12px'><span style=\"font-family:arial,helvetica,sans-serif\"><strong style='color:#000;text-align:center'>|</strong></span></span>");
            sb.AppendLine("                                                                                                                                                        </td>");
            sb.AppendLine("                                                                                                                                                        <td align='center'>");
            sb.AppendLine("                                                                                                                                                           <span style='font-size:12px'><a href='" + Combine(baseUrl, "/Home/Store?selectedCategory=2&selectedNavCategory=Smartphones") + "' style=\"font-weight:700;text-align:center;font-family:Roboto,arial,sans-serif;color:rgb(0,0,0);text-decoration:none\" title='' target='_blank' data-saferedirecturl='#'>SMART PHONES</a></span>");
            sb.AppendLine("                                                                                                                                                        </td>");
            sb.AppendLine("                                                                                                                                                        <td style='line-height:100%'>");
            sb.AppendLine("                                                                                                                                                            <span style='font-size:12px'><span style=\"font-family:arial,helvetica,sans-serif\"><strong style='color:#000;text-align:center'>|</strong></span></span>");
            sb.AppendLine("                                                                                                                                                        </td>");
            sb.AppendLine("                                                                                                                                                        <td align='center'>");
            sb.AppendLine("                                                                                                                                                            <span style='font-size:12px'><a href='" + Combine(baseUrl, "/Home/Store?selectedCategory=3&selectedNavCategory=Accessories") + "' style=\"font-weight:700;text-align:center;font-family:Roboto,arial,sans-serif;color:rgb(0,0,0);text-decoration:none\" title='' target='_blank' data-saferedirecturl='#'>ACCESORIES</a></span>");
            sb.AppendLine("                                                                                                                                                        </td>");
            sb.AppendLine("                                                                                                                                                        <td style='line-height:100%'>");
            sb.AppendLine("                                                                                                                                                            <span style='font-size:12px'><span style=\"font-family:arial,helvetica,sans-serif\"><strong style='color:#000;text-align:center'>|</strong></span></span>");
            sb.AppendLine("                                                                                                                                                        </td>");
            sb.AppendLine("                                                                                                                                                        <td align='center'>");
            sb.AppendLine("                                                                                                                                                            <span style='font-size:12px'><a href='" + Combine(baseUrl, "/Home/Store?selectedCategory=4&selectedNavCategory=Headphones") + "' style=\"font-weight:700;text-align:center;font-family:Roboto,arial,sans-serif;color:rgb(0,0,0);text-decoration:none\" title='' target='_blank' data-saferedirecturl='#'>HEADPHONES</a></span>");
            sb.AppendLine("                                                                                                                                                        </td>");
            sb.AppendLine("                                                                                                                                                        <td style='line-height:100%'>");
            sb.AppendLine("                                                                                                                                                            <span style='font-size:12px'><span style=\"font-family:arial,helvetica,sans-serif\"><strong style='color:#000;text-align:center'>|</strong></span></span>");
            sb.AppendLine("                                                                                                                                                        </td>");
            sb.AppendLine("                                                                                                                                                        <td align='center'>");
            sb.AppendLine("                                                                                                                                                            <span style='font-size:12px'><a href='" + Combine(baseUrl, "/Home/Store?selectedCategory=6&selectedNavCategory=Tablet") + "' style=\"font-weight:700;text-align:center;font-family:Roboto,arial,sans-serif;color:rgb(0,0,0);text-decoration:none\" title='' target='_blank' data-saferedirecturl='#'>TABLETS</a></span>");
            sb.AppendLine("                                                                                                                                                        </td>");
            sb.AppendLine("                                                                                                                                                    </tr>");
            sb.AppendLine("                                                                                                                                                </tbody>");
            sb.AppendLine("                                                                                                                                            </table>");
            sb.AppendLine("                                                                                                                                        </div>");
            sb.AppendLine("                                                                                                                                    </div>");
            sb.AppendLine("                                                                                                                                </td>");
            sb.AppendLine("                                                                                                                            </tr>");
            sb.AppendLine("                                                                                                                        </tbody>");
            sb.AppendLine("                                                                                                                    </table>");
            sb.AppendLine("                                                                                                                </td>");
            sb.AppendLine("                                                                                                            </tr>");
            sb.AppendLine("                                                                                                        </tbody>");
            sb.AppendLine("                                                                                                    </table>");
            sb.AppendLine("                                                                                                </td>");
            sb.AppendLine("                                                                                            </tr>");
            sb.AppendLine("                                                                                        </tbody>");
            sb.AppendLine("                                                                                    </table>");
            sb.AppendLine("                                                                                </td>");
            sb.AppendLine("                                                                            </tr>");
            sb.AppendLine("                                                                        </tbody>");
            sb.AppendLine("                                                                    </table>");

            sb.AppendLine("                                                                </td>");
            sb.AppendLine("                                                            </tr>");
            sb.AppendLine("                                                            <tr>");
            sb.AppendLine("                                                                <td align='left' valign='top'>");
            sb.AppendLine("                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='border:0px solid transparent;background-color:transparent;min-width:100%'>");
            sb.AppendLine("                                                                        <tbody>");
            sb.AppendLine("                                                                            <tr>");
            sb.AppendLine("                                                                                <td style='padding:0px'>");

            sb.AppendLine("                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>");
            sb.AppendLine("                                                                                        <tbody>");
            sb.AppendLine("                                                                                            <tr>");
            sb.AppendLine("                                                                                                <td>");

            sb.AppendLine("                                                                                                    <center>");
            sb.AppendLine("                                                                                                        <table width='600px' cellspacing='0' cellpadding='0'>");
            sb.AppendLine("                                                                                                            <tbody>");
            sb.AppendLine("                                                                                                                <tr>");

            sb.AppendLine("                                                                                                                    <td align='center' style='display:flex;'>");
            sb.AppendLine("                                                                                                                        <h2 style='color: #d10024; display: block; background-color: #efefef; padding: 0px; margin: 0px; line-height: 90px; text-align: center; height: 90px; width: 100%; border: 0px'>TOP PICKS OF THE WEEK</h2><div class='a6S' dir='ltr' style='opacity: 0.01; left: 1004px; top: 1753px;'><div id=':p7' class='T-I J-J5-Ji aQv T-I-ax7 L3 a5q' title='Download' role='button' tabindex='0' aria-label='Download attachment ' data-tooltip-class='a1V'><div class='akn'><div class='aSK J-J5-Ji aYr'></div></div></div></div>");
            sb.AppendLine("                                                                                                                    </td>");
            sb.AppendLine("                                                                                                                </tr>");
            sb.AppendLine("                                                                                                            </tbody>");
            sb.AppendLine("                                                                                                        </table>");
            sb.AppendLine("                                                                                                        <table border='0' width='600' cellspacing='0' cellpadding='0' bgcolor='#efefef'>");
            sb.AppendLine("                                                                                                            <tbody>");


                                                                                                                                int prodCount = 0;
                                                                                                                                int prodIndex = 0;


                                                                                                                                foreach (var item in products)
                                                                                                                                {
                                                                                                                                    prodCount += 1;
                                                                                                                                    prodIndex += 1;

                                                                                                                                    if (prodCount == 1)
                                                                                                                                    {
           sb.AppendLine("                                                                                                                        <tr>");

           sb.AppendLine("                                                                                                                    <td bgcolor='#efefef' width='15px'>");
           sb.AppendLine("                                                                                                                        &nbsp;");
           sb.AppendLine("                                                                                                                    </td>");

           sb.AppendLine("                                                                                                                    <td width='278'>");
           sb.AppendLine("                                                                                                                        <!-- zaldy 1-->");
           sb.AppendLine("                                                                                                                        <table border='0' cellspacing='0' cellpadding='0' bgcolor='white'>");
           sb.AppendLine("                                                                                                                            <tbody>");
           sb.AppendLine("                                                                                                                                <tr>");
           sb.AppendLine("                                                                                                                                    <td colspan='2' width='278px' height='278px' style='padding:12px;'>");
           sb.AppendLine("                                                                                                                                        <a href='" + Combine(baseUrl, "/Home/Product?productId=" + item.ProductId.ToString() + "&selectedNavCategory=Categories") + "' target='_blank' data-saferedirecturl='#'>");
                                                                                                                                        if (item.Images.Count > 0)
                                                                                                                                        {

                                                                                                                                            var itemImages = item.Images;

                                                                                                                                            foreach (var itemImage in itemImages)
                                                                                                                                            {
                                                                                                                                                if (itemImage.Default)
                                                                                                                                                {

                                                                                                                                           sb.AppendLine("                                                                                                                                                        <img style='width: 100%; height: 258px; object-fit: scale-down;' src='" + itemImage.ImagePath + "' alt='" + itemImage.Title + "' height='258px' class='CToWUd'>");
                                                                                                                                                    break;
                                                                                                                                                }
                                                                                                                                            }
                                                                                                                                        }

           sb.AppendLine("                                                                                                                                        </a>");
           sb.AppendLine("                                                                                                                                    </td>");
           sb.AppendLine("                                                                                                                                </tr>");
           sb.AppendLine("                                                                                                                                <tr>");
           sb.AppendLine("                                                                                                                                    <td style='padding:8px' colspan='2' width='230'>");

           sb.AppendLine("                                                                                                                                        <div style=\"float:left;color:#000000;font-weight:bold;font-family:'Roboto',arial,sans-serif!important;overflow:hidden;text-overflow:ellipsis;max-width:278px;height:32px;text-transform:capitalize;\">" + item.Name + "</div>");

           sb.AppendLine("                                                                                                                                    </td>");
           sb.AppendLine("                                                                                                                                </tr>");
           sb.AppendLine("                                                                                                                                <tr>");


           sb.AppendLine("                                                                                                                                    <!-- WITH PROMO -->");
           sb.AppendLine("                                                                                                                                    <td style='padding:8px'>");

                                                                                                                                        if (item.PromoSaleOFF > 0)
                                                                                                                                        {
                                                                                                                                            if (DateTime.Now >= item.PromoSaleStartDateTime && DateTime.Now <= item.PromoSaleEndDateTime)
                                                                                                                                            {

           sb.AppendLine("                                                                                                                                                <span  style=\"text-decoration:line-through;color:#bdbdbd;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.OriginalPrice).ToString("#,##0.00") + "</span>");
           sb.AppendLine("                                                                                                                                                <br>");
           sb.AppendLine("                                                                                                                                                <span  style=\"color:#D10024;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.DiscountedPrice).ToString("#,##0.00") + "</span>");


                                                                                                                                            }
                                                                                                                                            else
                                                                                                                                            {
           sb.AppendLine("                                                                                                                                                <span  style=\"color:#D10024;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.OriginalPrice).ToString("#,##0.00") + "</span>");

                                                                                                                                            }
                                                                                                                                        }
                                                                                                                                        else
                                                                                                                                        {
           sb.AppendLine("                                                                                                                                            <span  style=\"color:#D10024;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.OriginalPrice).ToString("#,##0.00") + "</span>");
                                                                                                                                        }


           sb.AppendLine("                                                                                                                                        <br>");
           sb.AppendLine("                                                                                                                                       <span  style=\"color:#000000;font-family:'Roboto',arial,sans-serif!important\">" + (item.Sold == null ? 0 : item.Sold) + " sold</span>");
           sb.AppendLine("                                                                                                                                    </td>");

                                                                                                                                    if (item.PromoSaleOFF > 0)
                                                                                                                                    {
                                                                                                                                        if (DateTime.Now >= item.PromoSaleStartDateTime && DateTime.Now <= item.PromoSaleEndDateTime)
                                                                                                                                        {

           sb.AppendLine("                                                                                                                                            <td style='padding:8px' align='right' valign='bottom' width='45px'>");

           sb.AppendLine("                                                                                                                                                <div style='padding-top: 5px; padding-bottom: 5px; background-color: #D10024; text-align: center'>");
           sb.AppendLine("                                                                                                                                                    <span  style=\"color:#ffffff;font-family:'Roboto',arial,sans-serif!important\">" + (double.Parse(item.PromoSaleOFF.ToString()) * 100) + "</span>");
           sb.AppendLine("                                                                                                                                                    <span  style=\"color: #ffffff; font-family: 'Roboto',arial,sans-serif !important \">%</span>");
           sb.AppendLine("                                                                                                                                                    <br>");
           sb.AppendLine("                                                                                                                                                    <span style=\"color:#ffffff;font-family:'Roboto',arial,sans-serif!important\">OFF</span>");
           sb.AppendLine("                                                                                                                                                </div>");

           sb.AppendLine("                                                                                                                                            </td>");

                                                                                                                                        }

                                                                                                                                    }
                                                                                                                                    else
                                                                                                                                    {
           sb.AppendLine("                                                                                                                                        <!-- NO PROMO -->");
           sb.AppendLine("                                                                                                                                        <td style='padding:8px'>");
           sb.AppendLine("                                                                                                                                        </td>");
                                                                                                                                    }


           sb.AppendLine("                                                                                                                                </tr>");
           sb.AppendLine("                                                                                                                            </tbody>");
           sb.AppendLine("                                                                                                                        </table>");
           sb.AppendLine("                                                                                                                    </td>");
                                                                                                                                    }

                                                                                                                            if (prodCount == 2)
                                                                                                                            {

           sb.AppendLine("                                                                                                                    <td bgcolor='#efefef' width='14'>");
           sb.AppendLine("                                                                                                                        &nbsp;");
           sb.AppendLine("                                                                                                                    </td>");

           sb.AppendLine("                                                                                                                    <td width='278'>");
           sb.AppendLine("                                                                                                                        <!-- zaldy 1-->");
           sb.AppendLine("                                                                                                                        <table border='0' cellspacing='0' cellpadding='0' bgcolor='white'>");
           sb.AppendLine("                                                                                                                            <tbody>");
           sb.AppendLine("                                                                                                                                <tr>");
           sb.AppendLine("                                                                                                                                    <td colspan='2' width='278px' height='278px' style='padding:12px;'>");
           sb.AppendLine("                                                                                                                                        <a href='" + Combine(baseUrl, "/Home/Product?productId=" + item.ProductId.ToString() + "&selectedNavCategory=Categories") + "' target='_blank' data-saferedirecturl='#'>");
                                                                                                                                if (item.Images.Count > 0)
                                                                                                                                {

                                                                                                                                    var itemImages = item.Images;

                                                                                                                                    foreach (var itemImage in itemImages)
                                                                                                                                    {
                                                                                                                                        if (itemImage.Default)
                                                                                                                                        {

           sb.AppendLine("                                                                                                                                                        <img style='width: 100%; height: 258px; object-fit: scale-down;' src='" + itemImage.ImagePath + "' alt='" + itemImage.Title + "' height='258px'>");
                                                                                                                                            break;
                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                }

           sb.AppendLine("                                                                                                                                        </a>");
           sb.AppendLine("                                                                                                                                    </td>");
           sb.AppendLine("                                                                                                                               </tr>");
           sb.AppendLine("                                                                                                                                <tr>");
           sb.AppendLine("                                                                                                                                    <td style='padding:8px' colspan='2' width='230'>");

           sb.AppendLine("                                                                                                                                       <div style=\"float:left;color:#000000;font-weight:bold;font-family:'Roboto',arial,sans-serif!important;overflow:hidden;text-overflow:ellipsis;max-width:278px;height:32px;text-transform:capitalize;\">" + item.Name + "</div>");

           sb.AppendLine("                                                                                                                                    </td>");
           sb.AppendLine("                                                                                                                                </tr>");
           sb.AppendLine("                                                                                                                               <tr>");


           sb.AppendLine("                                                                                                                                    <!-- WITH PROMO -->");
           sb.AppendLine("                                                                                                                                    <td style='padding:8px'>");

                                                                                                                                if (item.PromoSaleOFF > 0)
                                                                                                                                {
                                                                                                                                    if (DateTime.Now >= item.PromoSaleStartDateTime && DateTime.Now <= item.PromoSaleEndDateTime)
                                                                                                                                    {

           sb.AppendLine("                                                                                                                                                <span  style=\"text-decoration:line-through;color:#bdbdbd;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.OriginalPrice).ToString("#,##0.00") + "</span>");
           sb.AppendLine("                                                                                                                                               <br>");
           sb.AppendLine("                                                                                                                                                <span  style=\"color:#D10024;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.DiscountedPrice).ToString("#,##0.00") + "</span>");


                                                                                                                                    }
                                                                                                                                    else
                                                                                                                                    {
           sb.AppendLine("                                                                                                                                                <span  style=\"color:#D10024;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.OriginalPrice).ToString("#,##0.00") + "</span>");

                                                                                                                                    }
                                                                                                                                }
                                                                                                                                else
                                                                                                                                {
           sb.AppendLine("                                                                                                                                            <span  style=\"color:#D10024;font-family:'Roboto',arial,sans-serif!important\">₱" + Convert.ToDecimal(item.OriginalPrice).ToString("#,##0.00") + "</span>");
                                                                                                                                }


           sb.AppendLine("                                                                                                                                        <br>");
           sb.AppendLine("                                                                                                                                        <span style=\"color:#000000;font-family:'Roboto',arial,sans-serif!important\">" + (item.Sold == null ? 0 : item.Sold) + " sold</span>");
           sb.AppendLine("                                                                                                                                    </td>");

                                                                                                                                if (item.PromoSaleOFF > 0)
                                                                                                                                {
                                                                                                                                    if (DateTime.Now >= item.PromoSaleStartDateTime && DateTime.Now <= item.PromoSaleEndDateTime)
                                                                                                                                    {

           sb.AppendLine("                                                                                                                                            <td style='padding:8px' align='right' valign='bottom' width='45px'>");

           sb.AppendLine("                                                                                                                                                <div style='padding-top: 5px; padding-bottom: 5px; background-color: #D10024; text-align: center'>");
           sb.AppendLine("                                                                                                                                                    <span style=\"color:#ffffff;font-family:'Roboto',arial,sans-serif!important\">" + (double.Parse(item.PromoSaleOFF.ToString()) * 100) + "</span>");
           sb.AppendLine("                                                                                                                                                    <span  style=\"color: #ffffff; font-family: 'Roboto',arial,sans-serif !important \">%</span>");
           sb.AppendLine("                                                                                                                                                    <br>");
           sb.AppendLine("                                                                                                                                                    <span  style=\"color:#ffffff;font-family:'Roboto',arial,sans-serif!important\">OFF</span>");
           sb.AppendLine("                                                                                                                                                </div>");

           sb.AppendLine("                                                                                                                                            </td>");

                                                                                                                                    }

                                                                                                                                }
                                                                                                                                else
                                                                                                                                {
           sb.AppendLine("                                                                                                                                        <!-- NO PROMO -->");
           sb.AppendLine("                                                                                                                                        <td style='padding:8px'>");
           sb.AppendLine("                                                                                                                                        </td>");
                                                                                                                                }


           sb.AppendLine("                                                                                                                                </tr>");
           sb.AppendLine("                                                                                                                            </tbody>");
           sb.AppendLine("                                                                                                                        </table>");
           sb.AppendLine("                                                                                                                    </td>");
           sb.AppendLine("                                                                                                                    <td bgcolor='#efefef' width='15'>");
           sb.AppendLine("                                                                                                                        &nbsp;");
           sb.AppendLine("                                                                                                                    </td>");


           sb.AppendLine("                                                                                                                   </tr><tr bgcolor='#efefef'><td colspan='3' bgcolor='#efefef' height='15px'>&nbsp;</td></tr>'");


                                                                                                                                prodCount = 0;
                                                                                                                            }



                                                                                                                            if ((products.Count() % 2) > 0 && prodIndex == products.Count())
                                                                                                                            {
            sb.AppendLine("                                                                                                                    </tr><tr bgcolor='#efefef'><td colspan='3' bgcolor='#efefef' height='15px'>&nbsp;</td></tr>");
                                                                                                                            }

                                                                                                                        }

            sb.AppendLine("</tbody>");
            sb.AppendLine("                                                                                                        </table>");
            sb.AppendLine("                                                                                                        <table width='600px' cellspacing='0' cellpadding='0' bgcolor='#efefef'>");
            sb.AppendLine("                                                                                                            <tbody>");
            sb.AppendLine("                                                                                                                <tr>");
            sb.AppendLine("                                                                                                                    <td align='center' style='line-height:100px;height:100px;'>");
            sb.AppendLine("                                                                                                                        <a href='" + Combine(baseUrl, "/Home/Store?selectedCategory=1%2C2%2C3%2C4%2C5%2C6&selectedNavCategory=Categories") + "' target='_blank' data-saferedirecturl='#'><img src='~/FrontEnd/img/shop now button.png' width='194' style='display:block;text-align:center;height:auto;width:194px;border:0px' class='CToWUd'></a>");
            sb.AppendLine("                                                                                                                    </td>");
            sb.AppendLine("                                                                                                                </tr>");
            sb.AppendLine("                                                                                                            </tbody>");
            sb.AppendLine("                                                                                                        </table>");

            sb.AppendLine("                                                                                                        <table>");
            sb.AppendLine("                                                                                                            <tbody>");
            sb.AppendLine("                                                                                                                <tr>");
            sb.AppendLine("                                                                                                                    <td>");
            sb.AppendLine("                                                                                                                        <table cellpadding='0' cellspacing='0' border='0' align='center' width='594'>");
            sb.AppendLine("                                                                                                                            <tbody>");
            sb.AppendLine("                                                                                                                                <tr>");
            sb.AppendLine("                                                                                                                                    <td cellpadding='0' cellspacing='0' border='0' height='1' style='line-height:1px;min-width:198px'>");
            sb.AppendLine("                                                                                                                                        <img src='https://ci6.googleusercontent.com/proxy/UdD10b8OwAMsIskWs35kVksPpJn8Z8NNDQtpw7bGuUfndy5BBB668zFGfSCq9A8cZzBIS2GRgAc3V6hkoUm_YW9wUZSQOPyE0OxEg-vIlRo1PlfvsCydU1P7HB74FfFyhiuIe02kACyfdmCHRsq5RkvllmTQJQDvftAw-Q=s0-d-e1-ft#https://image.email.shopee.co.id/lib/fe3a15707564067d761279/m/11/82629a96-3443-4ffb-978e-e261f6d86cb8.png' width='200' height='1' style='display:block;max-height:1px;min-height:1px;min-width:120px;width:120px' class='CToWUd'>");
            sb.AppendLine("                                                                                                                                    </td>");
            sb.AppendLine("                                                                                                                                    <td cellpadding='0' cellspacing='0' border='0' height='1' style='line-height:1px;min-width:198px'>");
            sb.AppendLine("                                                                                                                                        <img src='https://ci6.googleusercontent.com/proxy/UdD10b8OwAMsIskWs35kVksPpJn8Z8NNDQtpw7bGuUfndy5BBB668zFGfSCq9A8cZzBIS2GRgAc3V6hkoUm_YW9wUZSQOPyE0OxEg-vIlRo1PlfvsCydU1P7HB74FfFyhiuIe02kACyfdmCHRsq5RkvllmTQJQDvftAw-Q=s0-d-e1-ft#https://image.email.shopee.co.id/lib/fe3a15707564067d761279/m/11/82629a96-3443-4ffb-978e-e261f6d86cb8.png' width='200' height='1' style='display:block;max-height:1px;min-height:1px;min-width:198px;width:198px' class='CToWUd'>");
            sb.AppendLine("                                                                                                                                    </td>");
            sb.AppendLine("                                                                                                                                    <td cellpadding='0' cellspacing='0' border='0' height='1' style='line-height:1px;min-width:198px'>");
            sb.AppendLine("                                                                                                                                        <img src='https://ci6.googleusercontent.com/proxy/UdD10b8OwAMsIskWs35kVksPpJn8Z8NNDQtpw7bGuUfndy5BBB668zFGfSCq9A8cZzBIS2GRgAc3V6hkoUm_YW9wUZSQOPyE0OxEg-vIlRo1PlfvsCydU1P7HB74FfFyhiuIe02kACyfdmCHRsq5RkvllmTQJQDvftAw-Q=s0-d-e1-ft#https://image.email.shopee.co.id/lib/fe3a15707564067d761279/m/11/82629a96-3443-4ffb-978e-e261f6d86cb8.png' width='200' height='1' style='display:block;max-height:1px;min-height:1px;min-width:198px;width:198px' class='CToWUd'>");
            sb.AppendLine("                                                                                                                                    </td>");
            sb.AppendLine("                                                                                                                                </tr>");
            sb.AppendLine("                                                                                                                           </tbody>");
            sb.AppendLine("                                                                                                                        </table>");
            sb.AppendLine("                                                                                                                    </td>");
            sb.AppendLine("                                                                                                                </tr>");
            sb.AppendLine("                                                                                                            </tbody>");
            sb.AppendLine("                                                                                                        </table>");
            sb.AppendLine("                                                                                                    </center>");
            sb.AppendLine("                                                                                                </td>");
            sb.AppendLine("                                                                                            </tr>");
            sb.AppendLine("                                                                                        </tbody>");
            sb.AppendLine("                                                                                    </table>");
            sb.AppendLine("                                                                                </td>");
            sb.AppendLine("                                                                            </tr>");
            sb.AppendLine("                                                                        </tbody>");
            sb.AppendLine("                                                                    </table>");

            sb.AppendLine("                                                                </td>");
            sb.AppendLine("                                                            </tr>");
            sb.AppendLine("                                                            <tr>");
            sb.AppendLine("                                                                <td align='left' valign='top'>");
            sb.AppendLine("                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>");
            sb.AppendLine("                                                                        <tbody>");
            sb.AppendLine("                                                                            <tr>");
            sb.AppendLine("                                                                                <td style='padding:10px 0px 0px'>");
            sb.AppendLine("                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='border:0px;background-color:transparent;min-width:100%'>");
            sb.AppendLine("                                                                                        <tbody>");
            sb.AppendLine("                                                                                            <tr>");
            sb.AppendLine("                                                                                                <td style='padding:0px'>");
            sb.AppendLine("                                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='background-color:transparent;min-width:100%'>");
            sb.AppendLine("                                                                                                        <tbody>");
            sb.AppendLine("                                                                                                            <tr>");
            sb.AppendLine("                                                                                                                <td style='padding:0px'>");
            sb.AppendLine("                                                                                                                    <table width='100%' cellspacing='0' cellpadding='0' role='presentation'>");
            sb.AppendLine("                                                                                                                        <tbody>");
            sb.AppendLine("                                                                                                                            <tr>");
            sb.AppendLine("                                                                                                                                <td align='center' style='padding:0px;margin:0p;'>");
            sb.AppendLine("                                                                                                                                    <a href='" + Combine(baseUrl, "/home/contact") + "' title='' target='_blank' data-saferedirecturl='#'><img src='~/FrontEnd/img/help center button.png' alt='Helpcenter' height='60' width='603' style='display:block;padding:0px;text-align:center;height:60px;width:603px;border:0px' class='CToWUd'></a>");
            sb.AppendLine("                                                                                                                                </td>");
            sb.AppendLine("                                                                                                                            </tr>");
            sb.AppendLine("                                                                                                                        </tbody>");
            sb.AppendLine("                                                                                                                    </table>");
            sb.AppendLine("                                                                                                                </td>");
            sb.AppendLine("                                                                                                            </tr>");
            sb.AppendLine("                                                                                                        </tbody>");
            sb.AppendLine("                                                                                                    </table><table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>");
            sb.AppendLine("                                                                                                        <tbody>");
            sb.AppendLine("                                                                                                            <tr>");
            sb.AppendLine("                                                                                                                <td>");
            sb.AppendLine("                                                                                                                    <table width='100%' align='center' style='width:100%!important' cellspacing='0' cellpadding='0'>");
            sb.AppendLine("                                                                                                                        <tbody>");

            sb.AppendLine("                                                                                                                            <tr>");
            sb.AppendLine("                                                                                                                                <td width='100%' align='center' style='border-top:1px solid #f3f3f3;border-bottom:1px solid #f3f3f3;padding:25px 0'>");
            sb.AppendLine("                                                                                                                                    <table width='30%' align='center' style='margin:0 auto' cellspacing='0' cellpadding='10'>");
            sb.AppendLine("                                                                                                                                        <tbody>");
            sb.AppendLine("                                                                                                                                            <tr>");
            sb.AppendLine("                                                                                                                                                <td style='width:33.3%;padding-left:5px;padding-right:5px;text-align:center'>");

            sb.AppendLine("                                                                                                                                                    <a href='https://www.facebook.com/' title='' target='_blank' data-saferedirecturl='#'><img src='https://ci6.googleusercontent.com/proxy/nwYp_9RmF7myx91ko4jV8qr2OwvKWxSDaI09WRA7Pp-SlHyZtUY8zKEFRukd20-HuDQ2eRtEvIIhwQe7W2g1VcW0oT_y5-hKX4v28TV8ddCwYSP05dY8TA2vtFMkF45kx25DfB4rKBesGylyXqb6LhyYdgCV8ncA=s0-d-e1-ft#https://image.email.shopee.sg/lib/fe3b15707564067d761278/m/1/cd8ee4c2-e30f-41a3-ad6f-b720f33342f0.png' alt='Facebook' height='50' width='50' style='display:block;padding:0px;text-align:center;height:50px;width:50px;border:0px none transparent' class='CToWUd'></a>");
            sb.AppendLine("                                                                                                                                                </td>");
            sb.AppendLine("                                                                                                                                                <td style='width:33.3%;padding-left:5px;padding-right:5px;text-align:center'>");

            sb.AppendLine("                                                                                                                                                    <a href='https://www.instagram.com/' title='' target='_blank' data-saferedirecturl='#'><img src='https://ci3.googleusercontent.com/proxy/3lxVMGF5Zll_k3AKxaFnhn55O30XYgq3uDsmEVveE3lxEqVVQt63ELmsoOsAffw_wUfzXDejnyLb_CuXvTLduIciMm3l2E-9syXAK-Awhq4GmDv97Z1LChI3Co6YZu6FQrdRYtOTrVQ1CQ4khvOZTMtXjGCnXvKq=s0-d-e1-ft#https://image.email.shopee.sg/lib/fe3b15707564067d761278/m/1/4ab223ec-7fc6-4417-b514-62b365329e6a.png' alt='Instagram' height='50' width='50' style='display:block;padding:0px;text-align:center;height:50px;width:50px;border:0px none transparent' class='CToWUd'></a>");
            sb.AppendLine("                                                                                                                                                </td>");
            sb.AppendLine("                                                                                                                                                <td style='width:33.3%;padding-left:5px;padding-right:5px;text-align:center'>");

            sb.AppendLine("                                                                                                                                                    <a href='https://twitter.com/' title='' target='_blank' data-saferedirecturl='#'><img src='https://ci4.googleusercontent.com/proxy/Y9v_DSD-ovdabCPxa29_eef54Z3Q0BxHnFAVb5B24vTLWKGGvRfPZzErwuuIXDFXnwW7a5Pny5qKzCeXRw1ZrvWj22VJkiErPvy_pqyfI-mS6h0E8NxjQjx4965aXXwjFOSSOhu9ro5wxhYT-kgZL5-vU_9jGfLCUwaJhw=s0-d-e1-ft#https://image.S10.exacttarget.com/lib/fe34157075640574731c72/m/1/801a8ba9-cd54-4786-bbde-dedb60f89cdc.png' alt='Twitter' height='50' width='50' style='display:block;padding:0px;text-align:center;height:50px;width:50px;border:0px none transparent' class='CToWUd'></a>");
            sb.AppendLine("                                                                                                                                                </td>");
            sb.AppendLine("                                                                                                                                            </tr>");
            sb.AppendLine("                                                                                                                                        </tbody>");
            sb.AppendLine("                                                                                                                                    </table>");
            sb.AppendLine("                                                                                                                                </td>");
            sb.AppendLine("                                                                                                                            </tr>");
            sb.AppendLine("                                                                                                                        </tbody>");
            sb.AppendLine("                                                                                                                    </table>");
            sb.AppendLine("                                                                                                                    <table style='font-size:10px;width:100%;display:none' width='100%' cellspacing='0' cellpadding='0'>");
            sb.AppendLine("                                                                                                                        <tbody>");
            sb.AppendLine("                                                                                                                            <tr>");
            sb.AppendLine("                                                                                                                                <td align='left'>");
            sb.AppendLine("                                                                                                                                    Subscription Center: <a href='https://www.facebook.com/' target='_blank' data-saferedirecturl='https://www.google.com/url?q=https://www.facebook.com/'><wbr>subscription_center.aspx?qs=<wbr>d9490f04e554b8613f1c65699125a5<wbr>22c759326db715a238a9eaac8d208b<wbr>24f928a0336226e997ff6e12da60f2<wbr>d2d387</a>");
            sb.AppendLine("                                                                                                                                    Profile Center: <a href='https://www.instagram.com/' target='_blank' data-saferedirecturl='https://www.google.com/url?q=https://www.instagram.com/'><wbr>profile_center.aspx?qs=<wbr>d9490f04e554b861049a956a7600b5<wbr>7fd382adefbcb9f4aed597e91da242<wbr>cf5b2eb873fbd2b99ac6b6a2dafa5a<wbr>fceb41</a>");
            sb.AppendLine("                                                                                                                                    One-click Unsubscribe: <a href='https://twitter.com/' target='_blank' data-saferedirecturl='https://www.google.com/url?q=https://twitter.com/'><wbr>unsub_center.aspx?qs=<wbr>d9490f04e554b8613f1c65699125a5<wbr>22c759326db715a238a9eaac8d208b<wbr>24f95e9668d5464f40ce0d5dbfcaf6<wbr>77f6bb09438b02330b1a91</a>");
            sb.AppendLine("                                                                                                                                    Sender address: Shopee Singapore Private Limited 2 Science Park Drive, #03-01, Tower A Ascent Singapore SG 118222 SG");
            sb.AppendLine("                                                                                                                                </td>");
            sb.AppendLine("                                                                                                                            </tr>");
            sb.AppendLine("                                                                                                                        </tbody>");
            sb.AppendLine("                                                                                                                    </table>");
            sb.AppendLine("                                                                                                                </td>");
            sb.AppendLine("                                                                                                            </tr>");
            sb.AppendLine("                                                                                                        </tbody>");
            sb.AppendLine("                                                                                                   </table>");
            sb.AppendLine("                                                                                                </td>");
            sb.AppendLine("                                                                                            </tr>");
            sb.AppendLine("                                                                                        </tbody>");
            sb.AppendLine("                                                                                    </table>");
            sb.AppendLine("                                                                                </td>");
            sb.AppendLine("                                                                            </tr>");
            sb.AppendLine("                                                                        </tbody>");
            sb.AppendLine("                                                                    </table>");

            sb.AppendLine("                                                                </td>");
            sb.AppendLine("                                                            </tr>");
            sb.AppendLine("                                                            <tr>");
            sb.AppendLine("                                                                <td align='left' valign='top'>");
            sb.AppendLine("                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='background-color:transparent;min-width:100%'>");
            sb.AppendLine("                                                                        <tbody>");
            sb.AppendLine("                                                                            <tr>");
            sb.AppendLine("                                                                                <td style='padding:0px'>");
            sb.AppendLine("                                                                                    <table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>");
            sb.AppendLine("                                                                                        <tbody>");
            sb.AppendLine("                                                                                            <tr>");
            sb.AppendLine("                                                                                                <td>");
            sb.AppendLine("                                                                                                    <div style=\"text-align:center;color:#858585;font-family:'Roboto',arial,sans-serif;font-size:12px;line-height:2\">");
            sb.AppendLine("                                                                                                        <span style='text-decoration:underline'><a href='" + Combine(baseUrl, "/Home/PrivacyPolicy") + "' style='color:#858585;text-decoration:underline' title='' target='_blank' data-saferedirecturl='#'>Privacy Notice</a></span> &nbsp;&nbsp; |&nbsp;&nbsp; <span style='text-decoration:underline'><a href='" + Combine(baseUrl, "/Home/TermsAndCondition") + "' style='color:#858585;text-decoration:underline' title='' target='_blank' data-saferedirecturl='#'>Terms and Conditions</a></span> &nbsp;&nbsp; |&nbsp;&nbsp; <span style='text-decoration:underline'>");
            sb.AppendLine("                                                                                                            <a href='#' style='color:#808080;text-decoration:underline;white-space:nowrap' title='' target='_blank' data-saferedirecturl='#'>View on Browser</a>");
            sb.AppendLine("                                                                                                        </span> &nbsp;&nbsp; &nbsp;|&nbsp;&nbsp; <span style='text-decoration:underline'>");
            sb.AppendLine("                                                                                                            <a href='" + Combine(baseUrl, "/Home/Unsubscribe?email=" + email) + "' style='color:#858585;text-decoration:underline' title='' target='_blank' data-saferedirecturl='#'>Unsubscribe</a>");
            sb.AppendLine("                                                                                                        </span>");
            sb.AppendLine("                                                                                                    </div>");
            sb.AppendLine("                                                                                                </td>");
            sb.AppendLine("                                                                                            </tr>");
            sb.AppendLine("                                                                                        </tbody>");
            sb.AppendLine("                                                                                    </table><table cellpadding='0' cellspacing='0' width='100%' role='presentation' style='min-width:100%'>");
            sb.AppendLine("                                                                                        <tbody>");
            sb.AppendLine("                                                                                            <tr>");
            sb.AppendLine("                                                                                                <td>");
            sb.AppendLine("                                                                                                    <div style=\"text-align:center;color:#858585;font-size:12px;font-family:'Roboto',arial,sans-serif;text-decoration:none\">");
            sb.AppendLine("                                                                                                        <p style='text-decoration:none'>");
            sb.AppendLine("                                                                                                            <span style='text-decoration:none'>");
            sb.AppendLine("                                                                                                                This is a post-only mailing.&nbsp;<br>");
            sb.AppendLine("                                                                                                                Add <a href='mailto:zaldys.ecommerce.demo@gmail.com' style='color:#858585;text-decoration:none' target='_blank'>zaldys.ecommerce.demo@gmail.com</a> to your address book to ensure our e-mails enter your inbox.");
            sb.AppendLine("                                                                                                            </span>");
            sb.AppendLine("                                                                                                        </p><p style='text-decoration:none'>");
            sb.AppendLine("                                                                                                            Zaldy's Ecommerce Philippines Inc., Malolos City, Bulacan");
            sb.AppendLine("                                                                                                        </p>");
            sb.AppendLine("                                                                                                    </div>");
            sb.AppendLine("                                                                                                </td>");
            sb.AppendLine("                                                                                            </tr>");
            sb.AppendLine("                                                                                        </tbody>");
            sb.AppendLine("                                                                                    </table>");
            sb.AppendLine("                                                                                </td>");
            sb.AppendLine("                                                                            </tr>");
            sb.AppendLine("                                                                        </tbody>");
            sb.AppendLine("                                                                    </table>");

            sb.AppendLine("                                                                </td>");
            sb.AppendLine("                                                            </tr>");
            sb.AppendLine("                                                        </tbody>");
            sb.AppendLine("                                                    </table>");
            sb.AppendLine("                                                </td>");
            sb.AppendLine("                                            </tr>");
            sb.AppendLine("                                        </tbody>");
            sb.AppendLine("                                    </table>");
            sb.AppendLine("                                </td>");
            sb.AppendLine("                            </tr>");
            sb.AppendLine("                        </tbody>");
            sb.AppendLine("                    </table>");
            sb.AppendLine("                </td>");
            sb.AppendLine("            </tr>");
            sb.AppendLine("        </tbody>");
            sb.AppendLine("    </table>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");


            return sb.ToString();
        }


        public ActionResult Compare(int? productId, string selectedNavCategory)
        {
            if (productId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Include(p => p.Category).Include(p => p.Brand).Include(p => p.Images).Single(p => p.ProductId == productId);
            ViewBag.Category = product.Category.Name;

            if (product == null)
            {
                return HttpNotFound();
            }

            var products = db.Products.Include(p => p.Category).Include(p => p.Brand).Include(p => p.Images).Where(p => p.CategoryId == product.CategoryId && p.ProductId != productId).Take(2);
            List<Product> _products = new List<Product>();
         
            _products.Add(product);
            _products.AddRange(products);

            ViewBag.SelectedNavCategory = selectedNavCategory;
            ViewBag.CurrentProduct = product;
            return View(_products);
        }


    }
}