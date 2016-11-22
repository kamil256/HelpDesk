namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeNamespacesOfDALClasses : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Tickets", "User2_UserID", "dbo.User2");
            DropForeignKey("dbo.Tickets", "User2_UserID1", "dbo.User2");
            DropForeignKey("dbo.Tickets", "User2_UserID2", "dbo.User2");
            DropIndex("dbo.Tickets", new[] { "User2_UserID" });
            DropIndex("dbo.Tickets", new[] { "User2_UserID1" });
            DropIndex("dbo.Tickets", new[] { "User2_UserID2" });
            DropIndex("dbo.User2", new[] { "Email" });
            DropColumn("dbo.Tickets", "User2_UserID");
            DropColumn("dbo.Tickets", "User2_UserID1");
            DropColumn("dbo.Tickets", "User2_UserID2");
            DropTable("dbo.User2");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.User2",
                c => new
                    {
                        UserID = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        Email = c.String(nullable: false, maxLength: 254),
                        HashedPassword = c.String(nullable: false),
                        Salt = c.String(nullable: false),
                        Phone = c.String(),
                        MobilePhone = c.String(),
                        Company = c.String(),
                        Department = c.String(),
                        Role = c.String(),
                    })
                .PrimaryKey(t => t.UserID);
            
            AddColumn("dbo.Tickets", "User2_UserID2", c => c.Int());
            AddColumn("dbo.Tickets", "User2_UserID1", c => c.Int());
            AddColumn("dbo.Tickets", "User2_UserID", c => c.Int());
            CreateIndex("dbo.User2", "Email", unique: true);
            CreateIndex("dbo.Tickets", "User2_UserID2");
            CreateIndex("dbo.Tickets", "User2_UserID1");
            CreateIndex("dbo.Tickets", "User2_UserID");
            AddForeignKey("dbo.Tickets", "User2_UserID2", "dbo.User2", "UserID");
            AddForeignKey("dbo.Tickets", "User2_UserID1", "dbo.User2", "UserID");
            AddForeignKey("dbo.Tickets", "User2_UserID", "dbo.User2", "UserID");
        }
    }
}
