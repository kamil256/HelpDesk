namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeIdtoUserIdtobemoreunderstandable : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.AspNetUsersHistory");
            AddColumn("dbo.AspNetUsersHistory", "UserId", c => c.String());
            AlterColumn("dbo.AspNetUsersHistory", "AspNetUsersHistoryId", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.AspNetUsersHistory", "AspNetUsersHistoryId");
            DropColumn("dbo.AspNetUsersHistory", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsersHistory", "Id", c => c.String(nullable: false, maxLength: 128));
            DropPrimaryKey("dbo.AspNetUsersHistory");
            AlterColumn("dbo.AspNetUsersHistory", "AspNetUsersHistoryId", c => c.Int(nullable: false));
            DropColumn("dbo.AspNetUsersHistory", "UserId");
            AddPrimaryKey("dbo.AspNetUsersHistory", "Id");
        }
    }
}
