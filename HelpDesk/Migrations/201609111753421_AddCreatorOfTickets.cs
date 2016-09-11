namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCreatorOfTickets : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Tickets", "RequestorID", "dbo.Users");
            DropIndex("dbo.Tickets", new[] { "RequestorID" });
            AddColumn("dbo.Tickets", "CreatorID", c => c.Int());
            AddColumn("dbo.Tickets", "Creator_UserID", c => c.Int());
            AlterColumn("dbo.Tickets", "RequestorID", c => c.Int());
            CreateIndex("dbo.Tickets", "RequestorID");
            CreateIndex("dbo.Tickets", "Creator_UserID");
            AddForeignKey("dbo.Tickets", "Creator_UserID", "dbo.Users", "UserID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tickets", "Creator_UserID", "dbo.Users");
            DropIndex("dbo.Tickets", new[] { "Creator_UserID" });
            DropIndex("dbo.Tickets", new[] { "RequestorID" });
            AlterColumn("dbo.Tickets", "RequestorID", c => c.Int(nullable: false));
            DropColumn("dbo.Tickets", "Creator_UserID");
            DropColumn("dbo.Tickets", "CreatorID");
            CreateIndex("dbo.Tickets", "RequestorID");
            AddForeignKey("dbo.Tickets", "RequestorID", "dbo.Users", "UserId");
        }
    }
}
