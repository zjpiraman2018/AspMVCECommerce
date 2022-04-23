namespace AspMVCECommerce.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialmigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Brands",
                c => new
                    {
                        BrandId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.BrandId);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.CategoryId);
            
            CreateTable(
                "dbo.CheckOuts",
                c => new
                    {
                        CheckOutId = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Mobile = c.String(),
                        ShoppingCartId = c.Int(nullable: false),
                        ShippingAddress2Id = c.Int(),
                        CustomAddressId = c.Int(),
                    })
                .PrimaryKey(t => t.CheckOutId)
                .ForeignKey("dbo.CustomAddresses", t => t.CustomAddressId)
                .ForeignKey("dbo.ShippingAddress2", t => t.ShippingAddress2Id)
                .ForeignKey("dbo.ShoppingCarts", t => t.ShoppingCartId, cascadeDelete: true)
                .Index(t => t.ShoppingCartId)
                .Index(t => t.ShippingAddress2Id)
                .Index(t => t.CustomAddressId);
            
            CreateTable(
                "dbo.CustomAddresses",
                c => new
                    {
                        CustomAddressId = c.Int(nullable: false, identity: true),
                        City = c.String(),
                        CountryCode = c.String(),
                        Line1 = c.String(),
                        Phone = c.String(),
                        PostalCode = c.String(),
                        Line2 = c.String(),
                        Province = c.String(),
                    })
                .PrimaryKey(t => t.CustomAddressId);
            
            CreateTable(
                "dbo.ShippingAddress2",
                c => new
                    {
                        ShippingAddress2Id = c.Int(nullable: false, identity: true),
                        City = c.String(),
                        CountryCode = c.String(),
                        Line1 = c.String(),
                        Phone = c.String(),
                        PostalCode = c.String(),
                        Line2 = c.String(),
                        RecipientName = c.String(),
                        Province = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                    })
                .PrimaryKey(t => t.ShippingAddress2Id);
            
            CreateTable(
                "dbo.ShoppingCarts",
                c => new
                    {
                        ShoppingCartId = c.Int(nullable: false, identity: true),
                        Created = c.DateTime(nullable: false),
                        CustomerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ShoppingCartId)
                .ForeignKey("dbo.AspNetUsers", t => t.CustomerId)
                .Index(t => t.CustomerId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.LineItems",
                c => new
                    {
                        LineItemId = c.Int(nullable: false, identity: true),
                        Quantity = c.Int(nullable: false),
                        TotalPrice = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        SizeId = c.Int(),
                        ColorId = c.Int(),
                        ShoppingCartId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LineItemId)
                .ForeignKey("dbo.Colors", t => t.ColorId)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .ForeignKey("dbo.Sizes", t => t.SizeId)
                .ForeignKey("dbo.ShoppingCarts", t => t.ShoppingCartId)
                .Index(t => t.ProductId)
                .Index(t => t.SizeId)
                .Index(t => t.ColorId)
                .Index(t => t.ShoppingCartId);
            
            CreateTable(
                "dbo.Colors",
                c => new
                    {
                        ColorId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ColorId)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        ProductId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        Details = c.String(),
                        OriginalPrice = c.Int(nullable: false),
                        DiscountedPrice = c.Int(nullable: false),
                        Stock = c.Int(nullable: false),
                        PromoSaleOFF = c.Double(nullable: false),
                        PromoSaleStartDateTime = c.DateTime(),
                        PromoSaleEndDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        BrandId = c.Int(nullable: false),
                        AverageRating = c.Int(),
                    })
                .PrimaryKey(t => t.ProductId)
                .ForeignKey("dbo.Brands", t => t.BrandId, cascadeDelete: true)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId)
                .Index(t => t.BrandId);
            
            CreateTable(
                "dbo.Images",
                c => new
                    {
                        ImageId = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        ImagePath = c.String(),
                        Default = c.Boolean(nullable: false),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ImageId)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Reviews",
                c => new
                    {
                        ReviewId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Email = c.String(),
                        Description = c.String(),
                        Rating = c.Double(nullable: false),
                        Created = c.DateTime(nullable: false),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReviewId)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Sizes",
                c => new
                    {
                        SizeId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SizeId)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderId = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        PaymentMethod = c.String(),
                        PaymentStatus = c.String(),
                        LineItemsCount = c.Int(nullable: false),
                        TotalAmount = c.Int(nullable: false),
                        OrderStatus = c.String(),
                        ShoppingCartId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.OrderId)
                .ForeignKey("dbo.ShoppingCarts", t => t.ShoppingCartId, cascadeDelete: true)
                .Index(t => t.ShoppingCartId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Orders", "ShoppingCartId", "dbo.ShoppingCarts");
            DropForeignKey("dbo.CheckOuts", "ShoppingCartId", "dbo.ShoppingCarts");
            DropForeignKey("dbo.LineItems", "ShoppingCartId", "dbo.ShoppingCarts");
            DropForeignKey("dbo.LineItems", "SizeId", "dbo.Sizes");
            DropForeignKey("dbo.LineItems", "ProductId", "dbo.Products");
            DropForeignKey("dbo.LineItems", "ColorId", "dbo.Colors");
            DropForeignKey("dbo.Sizes", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Reviews", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Images", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Colors", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Products", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.Products", "BrandId", "dbo.Brands");
            DropForeignKey("dbo.ShoppingCarts", "CustomerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CheckOuts", "ShippingAddress2Id", "dbo.ShippingAddress2");
            DropForeignKey("dbo.CheckOuts", "CustomAddressId", "dbo.CustomAddresses");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Orders", new[] { "ShoppingCartId" });
            DropIndex("dbo.Sizes", new[] { "ProductId" });
            DropIndex("dbo.Reviews", new[] { "ProductId" });
            DropIndex("dbo.Images", new[] { "ProductId" });
            DropIndex("dbo.Products", new[] { "BrandId" });
            DropIndex("dbo.Products", new[] { "CategoryId" });
            DropIndex("dbo.Colors", new[] { "ProductId" });
            DropIndex("dbo.LineItems", new[] { "ShoppingCartId" });
            DropIndex("dbo.LineItems", new[] { "ColorId" });
            DropIndex("dbo.LineItems", new[] { "SizeId" });
            DropIndex("dbo.LineItems", new[] { "ProductId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.ShoppingCarts", new[] { "CustomerId" });
            DropIndex("dbo.CheckOuts", new[] { "CustomAddressId" });
            DropIndex("dbo.CheckOuts", new[] { "ShippingAddress2Id" });
            DropIndex("dbo.CheckOuts", new[] { "ShoppingCartId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Orders");
            DropTable("dbo.Sizes");
            DropTable("dbo.Reviews");
            DropTable("dbo.Images");
            DropTable("dbo.Products");
            DropTable("dbo.Colors");
            DropTable("dbo.LineItems");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.ShoppingCarts");
            DropTable("dbo.ShippingAddress2");
            DropTable("dbo.CustomAddresses");
            DropTable("dbo.CheckOuts");
            DropTable("dbo.Categories");
            DropTable("dbo.Brands");
        }
    }
}
