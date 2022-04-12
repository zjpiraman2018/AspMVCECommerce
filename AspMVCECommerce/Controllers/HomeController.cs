using AspMVCECommerce.Models;
using AspMVCECommerce.ViewModel;
using PagedList;
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


    }
}