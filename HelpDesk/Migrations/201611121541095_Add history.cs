namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addhistory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Histories",
                c => new
                    {
                        HistoryId = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(maxLength: 128),
                        TableName = c.String(),
                        RecordId = c.String(),
                        ColumnName = c.String(),
                        ActionType = c.String(),
                        OldValue = c.String(),
                        NewValue = c.String(),
                    })
                .PrimaryKey(t => t.HistoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Histories", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Histories", new[] { "UserId" });
            DropTable("dbo.Histories");
        }
    }
}
