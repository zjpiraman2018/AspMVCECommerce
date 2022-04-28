namespace AspMVCECommerce.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class addsoldproperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "Sold", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "Sold");
        }
    }
}
