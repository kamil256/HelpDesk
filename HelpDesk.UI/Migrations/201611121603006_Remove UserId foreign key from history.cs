namespace HelpDesk.UI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUserIdforeignkeyfromhistory : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Histories", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Histories", new[] { "UserId" });
            AlterColumn("dbo.Histories", "UserId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Histories", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Histories", "UserId");
            AddForeignKey("dbo.Histories", "UserId", "dbo.AspNetUsers", "Id");
        }
    }
}
