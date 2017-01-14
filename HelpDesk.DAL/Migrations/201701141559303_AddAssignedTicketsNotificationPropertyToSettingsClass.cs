namespace HelpDesk.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAssignedTicketsNotificationPropertyToSettingsClass : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Settings", "AssignedTicketsNotifications", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Settings", "AssignedTicketsNotifications");
        }
    }
}
