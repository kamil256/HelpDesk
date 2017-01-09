namespace HelpDesk.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotificationAboutClosingTicketsToSettingsTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Settings", "ClosedTicketsNotifications", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Settings", "ClosedTicketsNotifications");
        }
    }
}
