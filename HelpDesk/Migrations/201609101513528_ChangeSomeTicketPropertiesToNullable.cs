namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeSomeTicketPropertiesToNullable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "Order", c => c.Int(nullable: false));
            AlterColumn("dbo.Tickets", "SolveOrCloseDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Tickets", "SolveOrCloseDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.Categories", "Order");
        }
    }
}
