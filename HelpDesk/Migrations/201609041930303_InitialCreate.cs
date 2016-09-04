namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.CategoryID);
            
            CreateTable(
                "dbo.Tickets",
                c => new
                    {
                        TicketID = c.Int(nullable: false, identity: true),
                        RequestorID = c.Int(nullable: false),
                        SolverID = c.Int(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        SolveOrCloseDate = c.DateTime(nullable: false),
                        status = c.String(),
                        CategoryID = c.Int(nullable: false),
                        Title = c.String(),
                        Content = c.String(),
                        Solution = c.String(),
                    })
                .PrimaryKey(t => t.TicketID)
                .ForeignKey("dbo.Categories", t => t.CategoryID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.RequestorID)
                .ForeignKey("dbo.Users", t => t.SolverID)
                .Index(t => t.RequestorID)
                .Index(t => t.SolverID)
                .Index(t => t.CategoryID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Email = c.String(),
                        Password = c.String(),
                        Salt = c.String(),
                        Phone = c.String(),
                        MobilePhone = c.String(),
                        Company = c.String(),
                        Department = c.String(),
                        Role = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tickets", "SolverID", "dbo.Users");
            DropForeignKey("dbo.Tickets", "RequestorID", "dbo.Users");
            DropForeignKey("dbo.Tickets", "CategoryID", "dbo.Categories");
            DropIndex("dbo.Tickets", new[] { "CategoryID" });
            DropIndex("dbo.Tickets", new[] { "SolverID" });
            DropIndex("dbo.Tickets", new[] { "RequestorID" });
            DropTable("dbo.Users");
            DropTable("dbo.Tickets");
            DropTable("dbo.Categories");
        }
    }
}
