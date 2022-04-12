namespace AspMVCECommerce.Migrations
{
    using AspMVCECommerce.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<AspMVCECommerce.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(AspMVCECommerce.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            //var Colors = new List<Color>
            //{
            //     new Color {ColorId =1, Name = "Red"},
            //     new Color {ColorId =2, Name = "Blue"},
            //     new Color {ColorId =3, Name = "Yellow"},
            //     new Color {ColorId =4, Name = "Green"},
            //     new Color {ColorId =5, Name = "Purple"}
            //};

            //Colors.ForEach(color => context.Colors.AddOrUpdate(x => x.Name, color));

            //var Sizes = new List<Size>
            //{
            //     new Size {SizeId =1, Name = "Small"},
            //     new Size {SizeId =2, Name = "Extra Small"},
            //     new Size {SizeId =3, Name = "Medium"},
            //     new Size {SizeId =4, Name = "Large"},
            //     new Size {SizeId =5, Name = "Extra Large"}
            //};

            //Sizes.ForEach(size => context.Sizes.AddOrUpdate(x => x.Name, size));

            var Categories = new List<Category>
            {
                 new Category {CategoryId =1, Name = "Laptop"},
                 new Category {CategoryId =2, Name = "Smart Phones"},
                 new Category {CategoryId =3, Name = "Accessories"},
                 new Category {CategoryId =4, Name = "Headphones"},
                 new Category {CategoryId =5, Name = "Camera"},
                 new Category {CategoryId =6, Name = "Tablet"},
            };

            Categories.ForEach(category => context.Categories.AddOrUpdate(x => x.Name, category));


            var Brands = new List<Brand>
            {
                 new Brand {BrandId =1, Name = "SAMSUNG"},
                 new Brand {BrandId =2, Name = "LG"},
                 new Brand {BrandId =3, Name = "SONY"},
                 new Brand {BrandId =4, Name = "TOSHIBA"},
                 new Brand {BrandId =5, Name = "LENOVO"},
                 new Brand {BrandId =6, Name = "APPLE"},
                 new Brand {BrandId =7, Name = "MICROSOFT"},
                 new Brand {BrandId =8, Name = "AOC"},
                 new Brand {BrandId =9, Name = "SHARP"},
                 new Brand {BrandId =10, Name = "ASUS"},
                 new Brand {BrandId =11, Name = "ACER"}
            };

            Brands.ForEach(brand => context.Brands.AddOrUpdate(x => x.Name, brand));

            context.SaveChanges();


            //var registerVM = new RegisterViewModel();
            //registerVM.Email = "zjpiraman2018@gmail.com";
            //var controller = new AccountController();
            //var task = Task.Run(async () => await controller.Register(registerVM));



            var passwordHash = new PasswordHasher();
            string password = passwordHash.HashPassword("123456");
            var adminUser = new ApplicationUser
            {
                UserName = "admin@gmail.com",
                PasswordHash = password,
                PhoneNumber = "12345678911",
                Email = "admin@gmail.com"
            };


            context.Users.AddOrUpdate(u => u.UserName, adminUser);
            context.SaveChanges();
            var adminId = adminUser.Id;

            context.Roles.AddOrUpdate(
                new IdentityRole { Id = "1", Name = "Admin" },
                new IdentityRole { Id = "2", Name = "Customer" }
            );
            context.SaveChanges();

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);
            userManager.AddToRole(adminId, "Admin");
            userManager.UpdateSecurityStamp(adminId);
            base.Seed(context);
        }
    }

 
}
