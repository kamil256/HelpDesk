namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeSolverIDToOptional : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Tickets", new[] { "SolverID" });
            AlterColumn("dbo.Tickets", "SolverID", c => c.Int());
            CreateIndex("dbo.Tickets", "SolverID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Tickets", new[] { "SolverID" });
            AlterColumn("dbo.Tickets", "SolverID", c => c.Int(nullable: false));
            CreateIndex("dbo.Tickets", "SolverID");
        }
    }
}
