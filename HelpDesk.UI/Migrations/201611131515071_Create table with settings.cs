namespace HelpDesk.UI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Createtablewithsettings : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        SettingsId = c.String(nullable: false, maxLength: 128),
                        NewTicketsNotifications = c.Boolean(nullable: false),
                        SolvedTicketsNotifications = c.Boolean(nullable: false),
                        UsersPerPage = c.Int(nullable: false),
                        TicketsPerPage = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SettingsId)
                .ForeignKey("dbo.AspNetUsers", t => t.SettingsId)
                .Index(t => t.SettingsId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Settings", "SettingsId", "dbo.AspNetUsers");
            DropIndex("dbo.Settings", new[] { "SettingsId" });
            DropTable("dbo.Settings");
        }
    }
}
