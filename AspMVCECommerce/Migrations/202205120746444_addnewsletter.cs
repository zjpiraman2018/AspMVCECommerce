namespace AspMVCECommerce.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addnewsletter : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NewsLetters",
                c => new
                    {
                        NewsLetterId = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        Created = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.NewsLetterId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.NewsLetters");
        }
    }
}
