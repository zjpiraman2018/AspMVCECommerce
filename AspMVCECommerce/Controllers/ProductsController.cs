using AspMVCECommerce.Models;
using AspMVCECommerce.ViewModel;
using PagedList;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace AspMVCECommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Products
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page , string pageSize)
        {
            ViewBag.SelectedNavCategory = "Manage Product";
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;


            var products = db.Products.Include(p => p.Category).Include(p=>p.Brand).Include(p => p.Images);

            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString)
                                       || p.Description.Contains(searchString)
                                       || p.Details.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p=> p.Name);
                    break;
                //case "Date":
                //    students = students.OrderBy(s => s.EnrollmentDate);
                //    break;
                //case "date_desc":
                //    students = students.OrderByDescending(s => s.EnrollmentDate);
                //    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }
        
            int _pageSize = string.IsNullOrEmpty(pageSize) ? 10 : Int32.Parse(pageSize);
            ViewBag.CurrentItemsPerPage = _pageSize;
            int pageNumber = (page ?? 1);
   

            return View(products.ToPagedList(pageNumber, _pageSize));
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Include(p => p.Category).Include(p => p.Brand).Include(p=>p.Images).Single(p=> p.ProductId == id);
            product.Description = WebUtility.HtmlDecode(product.Description).Replace("'", "\\'").Replace("\r\n","");
            product.Details = WebUtility.HtmlDecode(product.Details).Replace("'", "\\'").Replace("\r\n", "");
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.SelectedNavCategory = "Manage Product";
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name");
            ViewBag.BrandId = new SelectList(db.Brands, "BrandId", "Name");
            ViewBag.SelectedNavCategory = "Manage Product";
            return View();
        }


        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "ProductId,Name,Description,Details,OriginalPrice,DiscountedPrice,Stock,CategoryId,BrandId,ImageFile,Images,Colors,Sizes,PromoSaleOFF,PromoSaleStartDateTime,PromoSaleEndDateTime")]
            ProductViewModel productViewModel)
        {
            if (ModelState.IsValid)
            {

           
                var product = productViewModel.ToProduct();
                product.CreatedDateTime = DateTime.Now;

                db.Products.Add(product);

                // db.SaveChanges auto update the primary key of product object after inserting record to database
                db.SaveChanges();

                // Get the last inserted ID for product
                var productId = product.ProductId;

                int imageIndex = 0;
                if (Request.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];

                        if (file != null && file.ContentLength > 0)
                        {
                           
                            Image imageModel = new Image();

                            imageModel.Default = (imageIndex == 0 ? true : false);

                            imageModel.ProductId = productId;

                            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                            imageModel.Title = fileName;

                            string extension = Path.GetExtension(file.FileName);
                            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;

                        
                            imageModel.ImagePath = "~/FrontEnd/img/" + fileName;

                            fileName = Path.Combine(Server.MapPath("~/FrontEnd/img/"), fileName);
                            file.SaveAs(fileName);

                            db.Images.Add(imageModel);
                            imageIndex += 1;

                        }
                    }

                    db.SaveChanges();
                    ModelState.Clear();
                }

                // ADD SIZES TO DATABASE
                foreach (var size in productViewModel.Sizes)
                {
                    var _size = new Size();
                    _size.Name = size.Name;
                    _size.ProductId = productId;
                    db.Sizes.Add(_size);
                    db.SaveChanges();
                }
                // END OF ADD SIZES TO DATABASE


                // ADD COLORS TO DATABASE
                foreach (var color in productViewModel.Colors)
                {
                    var _color = new Color();
                    _color.Name = color.Name;
                    _color.ProductId = productId;
                    db.Colors.Add(_color);
                    db.SaveChanges();
                }
                // END OF ADD COLORS TO DATABASE

                return RedirectToAction("Index");
            }
            ViewBag.SelectedNavCategory = "Manage Product";
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", productViewModel.CategoryId);
            ViewBag.BrandId = new SelectList(db.Brands, "BrandId", "Name", productViewModel.BrandId);
            return View(productViewModel);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id, string success)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Find(id);
           
            if (product == null)
            {
                return HttpNotFound();
            }

            var productViewModel = product.ToProductViewModel();

            var images = db.Images.Where(a => a.ProductId == id).ToList();
            foreach (var image in images)
            {
                productViewModel.Images.Add(image.ToImageViewModel());
            }


            var sizes = db.Sizes.Where(a => a.ProductId == id).ToList();
            foreach (var size in sizes)
            {
                productViewModel.Sizes.Add(size.ToSizeViewModel());
            }

            var colors = db.Colors.Where(a => a.ProductId == id).ToList();
            foreach (var color in colors)
            {
                productViewModel.Colors.Add(color.ToColorViewModel());
            }


            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            ViewBag.BrandId = new SelectList(db.Brands, "BrandId", "Name", product.BrandId);

            ViewData["Success"] = (string.IsNullOrEmpty(success) ? null : success);

            ViewBag.SelectedNavCategory = "Manage Product";
            return View(productViewModel);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductId,Name,Description,Details,OriginalPrice,DiscountedPrice,Stock,CategoryId,BrandId,ImageFile,Images,Colors,Sizes,PromoSaleOFF,CreatedDateTime,PromoSaleStartDateTime,PromoSaleEndDateTime")] ProductViewModel productViewModel)
        {
            var product = productViewModel.ToProduct();
            if (ModelState.IsValid)
            {
                var _product = db.Products.Find(product.ProductId);
                _product.ProductId = product.ProductId;
                _product.CategoryId = product.CategoryId;
                _product.BrandId = product.BrandId;
                _product.Description = product.Description;
                _product.Details = product.Details;
                _product.DiscountedPrice = product.DiscountedPrice;
                _product.Name = product.Name;
                _product.OriginalPrice = product.OriginalPrice;
                _product.Stock = product.Stock;
                _product.PromoSaleOFF = product.PromoSaleOFF;
                _product.PromoSaleStartDateTime = product.PromoSaleStartDateTime;
                _product.PromoSaleEndDateTime = product.PromoSaleEndDateTime;
                _product.CreatedDateTime = product.CreatedDateTime;


                db.Entry(_product).State = EntityState.Modified;
                db.SaveChanges();



                // UPDATE SIZES RECORD
                var notForDeleteSizeIdList = productViewModel.Sizes.Where(s => s.SizeId > 0).Select(s=>s.SizeId).ToList();
                var forDeleteSizeList = db.Sizes.Where(s => s.ProductId == product.ProductId && !notForDeleteSizeIdList.Contains(s.SizeId)).ToList();

                db.Sizes.RemoveRange(forDeleteSizeList);
                db.SaveChanges();


                var sizeViewModelList = productViewModel.Sizes.Where(s => s.SizeId == 0).ToList();

                foreach (var sizeViewModel in sizeViewModelList)
                {
                    var size = new Size()
                    {
                         Name = sizeViewModel.Name,
                         ProductId = product.ProductId,
                         SizeId = 0
                    };

                    db.Sizes.Add(size);
                    db.SaveChanges();
                }
                // END OF UPDATE SIZES RECORD


                // UPDATE COLORS RECORD
                var notForDeleteColorIdList = productViewModel.Colors.Where(s => s.ColorId > 0).Select(s => s.ColorId).ToList();
                var forDeleteColorList = db.Colors.Where(s => s.ProductId == product.ProductId && !notForDeleteColorIdList.Contains(s.ColorId)).ToList();

                db.Colors.RemoveRange(forDeleteColorList);
                db.SaveChanges();


                var colorViewModelList = productViewModel.Colors.Where(s => s.ColorId == 0).ToList();

                foreach (var colorViewModel in colorViewModelList)
                {
                    var color = new Color()
                    {
                        Name = colorViewModel.Name,
                        ProductId = product.ProductId,
                        ColorId = 0
                    };

                    db.Colors.Add(color);
                    db.SaveChanges();
                }
                // END OF UPDATE Colors RECORD

                foreach (var imageViewModel in productViewModel.Images)
                {
                    var image = db.Images.Find(imageViewModel.ImageId);
                    if (imageViewModel.IsRemove.ToString() == "True")
                    {
                        db.Images.Remove(image);
                        db.SaveChanges();
                    }
                    else
                    {
                        image.Default = (imageViewModel.Default.ToString() == "True" ? true : false);
                        db.Entry(image).State = EntityState.Modified;
                        db.SaveChanges();
                    }
   
                }

                int imageIndex = 0;
                if (Request.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];

                        if (file != null && file.ContentLength > 0)
                        {
                            Image imageModel = new Image();

                            imageModel.Default = (imageIndex == 0 && productViewModel.Images.Count == 0 ? true : false);

                            imageModel.ProductId = product.ProductId;

                            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                            imageModel.Title = fileName;

                            string extension = Path.GetExtension(file.FileName);
                            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;


                            imageModel.ImagePath = "~/FrontEnd/img/" + fileName;

                            fileName = Path.Combine(Server.MapPath("~/FrontEnd/img/"), fileName);
                            file.SaveAs(fileName);

                            db.Images.Add(imageModel);
                            imageIndex += 1;

                        }
                    }

                    db.SaveChanges();
                    ModelState.Clear();
                }


                ViewBag.SelectedNavCategory = "Manage Product";
                ViewData["Success"] = "TRUE";
                return RedirectToAction("Edit", new { Id = product.ProductId , success = "TRUE"});
            }

            var modelStateErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();
            string modelStateErrorMessage = "";
            foreach (var error in modelStateErrors)
            {
                modelStateErrorMessage = (modelStateErrorMessage == "" ? "" : modelStateErrorMessage + ", ") + error.ErrorMessage.ToString();
            }

            ViewData["Success"] = modelStateErrorMessage;
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);

            var images = db.Images.Where(a => a.ProductId == product.ProductId).ToList();

            productViewModel.Images = new System.Collections.Generic.List<ImageViewModel>();

            foreach (var image in images)
            {
                productViewModel.Images.Add(image.ToImageViewModel());
            }

            ViewBag.SelectedNavCategory = "Manage Product";
            return View(productViewModel);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Include(p => p.Category).Include(p=>p.Brand).Where(p=>p.ProductId == id).FirstOrDefault();
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.SelectedNavCategory = "Manage Product";
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            ViewBag.SelectedNavCategory = "Manage Product";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
