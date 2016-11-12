namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAspNetUsersHistorytable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AspNetUsersHistory",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ChangeDate = c.DateTime(nullable: false),
                        ChangeAuthorFullName = c.String(),
                        ChangeAuthorId = c.String(),
                        ActionType = c.String(),
                        ColumnName = c.String(),
                        OldValue = c.String(),
                        NewValue = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropTable("dbo.Histories");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Histories",
                c => new
                    {
                        HistoryId = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(),
                        TableName = c.String(),
                        RecordId = c.String(),
                        ColumnName = c.String(),
                        ActionType = c.String(),
                        OldValue = c.String(),
                        NewValue = c.String(),
                    })
                .PrimaryKey(t => t.HistoryId);
            
            DropTable("dbo.AspNetUsersHistory");
        }
    }
}
