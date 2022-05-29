namespace AspMVCECommerce.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addlog : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        LogId = c.Int(nullable: false, identity: true),
                        Type = c.String(),
                        Message = c.String(),
                        Created = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.LogId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Logs");
        }
    }
}
