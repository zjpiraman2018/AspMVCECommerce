namespace AspMVCECommerce.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAverageRatingPropertyToProductModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "AverageRating", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "AverageRating");
        }
    }
}
