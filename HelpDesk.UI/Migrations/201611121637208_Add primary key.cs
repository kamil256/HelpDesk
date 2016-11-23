namespace HelpDesk.UI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addprimarykey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsersHistory", "AspNetUsersHistoryId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsersHistory", "AspNetUsersHistoryId");
        }
    }
}
