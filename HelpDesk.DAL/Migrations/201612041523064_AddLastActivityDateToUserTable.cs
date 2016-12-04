namespace HelpDesk.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLastActivityDateToUserTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "LastActivity", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "LastActivity");
        }
    }
}
