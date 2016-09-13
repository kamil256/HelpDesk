namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeSolverToAssignedPersonAndRemoveSolveOrCloseDate : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Tickets", name: "Creator_UserID", newName: "CreatedByID");
            RenameColumn(table: "dbo.Tickets", name: "RequestorID", newName: "RequestedByID");
            RenameColumn(table: "dbo.Tickets", name: "SolverID", newName: "AssignedToID");
            RenameIndex(table: "dbo.Tickets", name: "IX_Creator_UserID", newName: "IX_CreatedByID");
            RenameIndex(table: "dbo.Tickets", name: "IX_RequestorID", newName: "IX_RequestedByID");
            RenameIndex(table: "dbo.Tickets", name: "IX_SolverID", newName: "IX_AssignedToID");
            AlterColumn("dbo.Tickets", "Status", c => c.String(nullable: false));
            AlterColumn("dbo.Tickets", "Title", c => c.String(nullable: false));
            AlterColumn("dbo.Tickets", "Content", c => c.String(nullable: false));
            DropColumn("dbo.Tickets", "CreatorID");
            DropColumn("dbo.Tickets", "SolveOrCloseDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tickets", "SolveOrCloseDate", c => c.DateTime());
            AddColumn("dbo.Tickets", "CreatorID", c => c.Int());
            AlterColumn("dbo.Tickets", "Content", c => c.String());
            AlterColumn("dbo.Tickets", "Title", c => c.String());
            AlterColumn("dbo.Tickets", "Status", c => c.String());
            RenameIndex(table: "dbo.Tickets", name: "IX_AssignedToID", newName: "IX_SolverID");
            RenameIndex(table: "dbo.Tickets", name: "IX_RequestedByID", newName: "IX_RequestorID");
            RenameIndex(table: "dbo.Tickets", name: "IX_CreatedByID", newName: "IX_Creator_UserID");
            RenameColumn(table: "dbo.Tickets", name: "AssignedToID", newName: "SolverID");
            RenameColumn(table: "dbo.Tickets", name: "RequestedByID", newName: "RequestorID");
            RenameColumn(table: "dbo.Tickets", name: "CreatedByID", newName: "Creator_UserID");
        }
    }
}
