namespace HelpDesk.DAL.Migrations
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
                        CategoryId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CategoryId);
            
            CreateTable(
                "dbo.Tickets",
                c => new
                    {
                        TicketId = c.Int(nullable: false, identity: true),
                        CreateDate = c.DateTime(nullable: false),
                        CreatorId = c.String(maxLength: 128),
                        RequesterId = c.String(maxLength: 128),
                        AssignedUserId = c.String(maxLength: 128),
                        Status = c.String(nullable: false),
                        CategoryId = c.Int(),
                        Title = c.String(nullable: false),
                        Content = c.String(nullable: false),
                        Solution = c.String(),
                    })
                .PrimaryKey(t => t.TicketId)
                .ForeignKey("dbo.AspNetUsers", t => t.AssignedUserId)
                .ForeignKey("dbo.Categories", t => t.CategoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatorId)
                .ForeignKey("dbo.AspNetUsers", t => t.RequesterId)
                .Index(t => t.CreatorId)
                .Index(t => t.RequesterId)
                .Index(t => t.AssignedUserId)
                .Index(t => t.CategoryId);
            
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
                        Active = c.Boolean(nullable: false),
                        LastActivity = c.DateTime(),
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
                "dbo.Settings",
                c => new
                    {
                        SettingsId = c.String(nullable: false, maxLength: 128),
                        NewTicketsNotifications = c.Boolean(nullable: false),
                        SolvedTicketsNotifications = c.Boolean(nullable: false),
                        ClosedTicketsNotifications = c.Boolean(nullable: false),
                        UsersPerPage = c.Int(nullable: false),
                        TicketsPerPage = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SettingsId)
                .ForeignKey("dbo.AspNetUsers", t => t.SettingsId)
                .Index(t => t.SettingsId);
            
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
                "dbo.TicketsHistory",
                c => new
                    {
                        TicketsHistoryId = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        AuthorId = c.String(nullable: false),
                        TicketId = c.Int(nullable: false),
                        Column = c.String(nullable: false),
                        NewValue = c.String(),
                    })
                .PrimaryKey(t => t.TicketsHistoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Tickets", "RequesterId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "CreatorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.Tickets", "AssignedUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Settings", "SettingsId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Settings", new[] { "SettingsId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Tickets", new[] { "CategoryId" });
            DropIndex("dbo.Tickets", new[] { "AssignedUserId" });
            DropIndex("dbo.Tickets", new[] { "RequesterId" });
            DropIndex("dbo.Tickets", new[] { "CreatorId" });
            DropTable("dbo.TicketsHistory");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Settings");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Tickets");
            DropTable("dbo.Categories");
        }
    }
}
