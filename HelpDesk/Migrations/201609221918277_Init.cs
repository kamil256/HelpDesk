namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CategoryID);
            
            CreateTable(
                "dbo.Tickets",
                c => new
                    {
                        TicketID = c.Int(nullable: false, identity: true),
                        CreatedByID = c.String(maxLength: 128),
                        RequestedByID = c.String(maxLength: 128),
                        AssignedToID = c.String(maxLength: 128),
                        CreatedOn = c.DateTime(nullable: false),
                        Status = c.String(nullable: false),
                        CategoryID = c.Int(),
                        Title = c.String(nullable: false),
                        Content = c.String(nullable: false),
                        Solution = c.String(),
                        User2_UserID = c.Int(),
                        User2_UserID1 = c.Int(),
                        User2_UserID2 = c.Int(),
                    })
                .PrimaryKey(t => t.TicketID)
                .ForeignKey("dbo.AspNetUsers", t => t.AssignedToID)
                .ForeignKey("dbo.Categories", t => t.CategoryID)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByID)
                .ForeignKey("dbo.AspNetUsers", t => t.RequestedByID)
                .ForeignKey("dbo.User2", t => t.User2_UserID)
                .ForeignKey("dbo.User2", t => t.User2_UserID1)
                .ForeignKey("dbo.User2", t => t.User2_UserID2)
                .Index(t => t.CreatedByID)
                .Index(t => t.RequestedByID)
                .Index(t => t.AssignedToID)
                .Index(t => t.CategoryID)
                .Index(t => t.User2_UserID)
                .Index(t => t.User2_UserID1)
                .Index(t => t.User2_UserID2);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Phone = c.String(),
                        MobilePhone = c.String(),
                        Company = c.String(),
                        Department = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
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
                .PrimaryKey(t => t.UserID)
                .Index(t => t.Email, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tickets", "User2_UserID2", "dbo.User2");
            DropForeignKey("dbo.Tickets", "User2_UserID1", "dbo.User2");
            DropForeignKey("dbo.Tickets", "User2_UserID", "dbo.User2");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Tickets", "RequestedByID", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "CreatedByID", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.Tickets", "AssignedToID", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.User2", new[] { "Email" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Tickets", new[] { "User2_UserID2" });
            DropIndex("dbo.Tickets", new[] { "User2_UserID1" });
            DropIndex("dbo.Tickets", new[] { "User2_UserID" });
            DropIndex("dbo.Tickets", new[] { "CategoryID" });
            DropIndex("dbo.Tickets", new[] { "AssignedToID" });
            DropIndex("dbo.Tickets", new[] { "RequestedByID" });
            DropIndex("dbo.Tickets", new[] { "CreatedByID" });
            DropTable("dbo.User2");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Tickets");
            DropTable("dbo.Categories");
        }
    }
}
