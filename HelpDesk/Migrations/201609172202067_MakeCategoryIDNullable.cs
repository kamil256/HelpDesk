namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeCategoryIDNullable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Tickets", "CategoryID", "dbo.Categories");
            DropIndex("dbo.Tickets", new[] { "CategoryID" });
            AddColumn("dbo.Tickets", "CreatedOn", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Tickets", "CategoryID", c => c.Int());
            CreateIndex("dbo.Tickets", "CategoryID");
            AddForeignKey("dbo.Tickets", "CategoryID", "dbo.Categories", "CategoryID");
            DropColumn("dbo.Tickets", "CreateDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tickets", "CreateDate", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.Tickets", "CategoryID", "dbo.Categories");
            DropIndex("dbo.Tickets", new[] { "CategoryID" });
            AlterColumn("dbo.Tickets", "CategoryID", c => c.Int(nullable: false));
            DropColumn("dbo.Tickets", "CreatedOn");
            CreateIndex("dbo.Tickets", "CategoryID");
            AddForeignKey("dbo.Tickets", "CategoryID", "dbo.Categories", "CategoryID", cascadeDelete: true);
        }
    }
}
