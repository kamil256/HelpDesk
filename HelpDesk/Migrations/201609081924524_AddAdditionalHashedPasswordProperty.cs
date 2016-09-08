namespace HelpDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAdditionalHashedPasswordProperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "HashedPassword", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "Salt", c => c.String(nullable: false));
            DropColumn("dbo.Users", "Password");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "Password", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Users", "Salt", c => c.String());
            DropColumn("dbo.Users", "HashedPassword");
        }
    }
}
