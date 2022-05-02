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
                 new Brand {BrandId =11, Name = "ACER"},
                 new Brand {BrandId =12, Name = "LOGITECH"},
                new Brand {BrandId =13, Name = "JEECOO"},
                new Brand {BrandId =14, Name = "ZULU"},
                new Brand {BrandId =15, Name = "SENZER"},
                new Brand {BrandId =16, Name = "BINNUNE"},
                new Brand {BrandId =17, Name = "TURTLE BEACH"},
                new Brand {BrandId =18, Name = "EARTEC"},
                new Brand {BrandId =19, Name = "SUIFLES"},
                new Brand {BrandId =20, Name = "HYPERX"},
                new Brand {BrandId =21, Name = "PHILIPS"},
                new Brand {BrandId =22, Name = "BOSE"},
                new Brand {BrandId =23, Name = "BQAA"},
                new Brand {BrandId =24, Name = "LAMICALL"},
                new Brand {BrandId =25, Name = "ARNARKOK"},
                new Brand {BrandId =26, Name = "HEYSONG"},
                new Brand {BrandId =27, Name = "NTONPOWER"},
                new Brand {BrandId =28, Name = "COZOO"},
                new Brand {BrandId =29, Name = "PERILOGICS"},
                new Brand {BrandId =30, Name = "WIXGEAR"},
                new Brand {BrandId =31, Name = "YLQP"},
                new Brand {BrandId =32, Name = "AMAZON"},
                new Brand {BrandId =33, Name = "OLEXEX"},
                new Brand {BrandId =34, Name = "COOPERS"},
                new Brand {BrandId =35, Name = "ANYMOOD"},
                new Brand {BrandId =36, Name = "ZZB"},
                new Brand {BrandId =37, Name = "G-TIDE"},
                new Brand {BrandId =38, Name = "NEOREGENT"},
                new Brand {BrandId =39, Name = "VELORIM"},
                new Brand {BrandId =40, Name = "VENTURER"},
                new Brand {BrandId =41, Name = "TCL"},
                new Brand {BrandId =42, Name = "TRACFONE"},
                new Brand {BrandId =43, Name = "MOTOROLA"},
                new Brand {BrandId =44, Name = "AZUMI"},
                new Brand {BrandId =45, Name = "ONEPLUS"},
                new Brand {BrandId =46, Name = "XIAOMI"},
                new Brand {BrandId =47, Name = "NOKIA"},
                new Brand {BrandId =48, Name = "UMIDIGI"},
                new Brand {BrandId =49, Name = "ULEFONE"},
                new Brand {BrandId =50, Name = "FAMBROW"},
                new Brand {BrandId =51, Name = "SUPERIORTEK"},
                new Brand {BrandId =52, Name = "CANON"},
                new Brand {BrandId =53, Name = "KODAK"},
                new Brand {BrandId =54, Name = "VMOTAL"},
                new Brand {BrandId =55, Name = "VETEK"},
                new Brand {BrandId =56, Name = "BIFEVSR"},
                new Brand {BrandId =57, Name = "VJIANGER"}

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
