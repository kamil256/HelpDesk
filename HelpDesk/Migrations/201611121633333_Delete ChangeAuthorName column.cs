namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteChangeAuthorNamecolumn : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsersHistory", "ChangeAuthorFullName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsersHistory", "ChangeAuthorFullName", c => c.String());
        }
    }
}
