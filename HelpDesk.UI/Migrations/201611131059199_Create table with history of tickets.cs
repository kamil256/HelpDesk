namespace HelpDesk.UI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Createtablewithhistoryoftickets : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TicketsHistory",
                c => new
                    {
                        TicketsHistoryId = c.Guid(nullable: false, identity: true),
                        ChangeDate = c.DateTime(nullable: false),
                        ChangeAuthorId = c.String(),
                        TicketId = c.String(),
                        ActionType = c.String(),
                        ColumnName = c.String(),
                        OldValue = c.String(),
                        NewValue = c.String(),
                    })
                .PrimaryKey(t => t.TicketsHistoryId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TicketsHistory");
        }
    }
}
