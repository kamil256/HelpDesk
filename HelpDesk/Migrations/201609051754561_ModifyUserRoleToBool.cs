namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyUserRoleToBool : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Admin", c => c.Boolean(nullable: false));
            DropColumn("dbo.Users", "Role");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "Role", c => c.String());
            DropColumn("dbo.Users", "Admin");
        }
    }
}
