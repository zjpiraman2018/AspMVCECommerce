namespace AspMVCECommerce.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addConfirmFieldToNewsLetter : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NewsLetters", "IsComfirmed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NewsLetters", "IsComfirmed");
        }
    }
}
