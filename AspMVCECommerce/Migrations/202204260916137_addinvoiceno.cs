namespace AspMVCECommerce.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addinvoiceno : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "InvoiceNo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "InvoiceNo");
        }
    }
}
