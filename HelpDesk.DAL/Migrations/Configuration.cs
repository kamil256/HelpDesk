namespace HelpDesk.DAL.Migrations
{
    using Concrete;
    using Entities;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<HelpDesk.DAL.Concrete.HelpDeskContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(HelpDesk.DAL.Concrete.HelpDeskContext context)
        {
            UserManager userMgr = new UserManager(new UserStore<User>(context));
            RoleManager roleMgr = new RoleManager(new RoleStore<Role>(context));
            string roleName = "Admin";
            string userName = "admin@example.com";
            // CURRENT ADMIN PASSWORD IS DIFFERENT THAN INITIAL!!!
            string password = "Password";
            string email = "admin@example.com";
            if (!roleMgr.RoleExists(roleName))
            {
                roleMgr.Create(new Role(roleName));
            }
            if (!roleMgr.RoleExists("User"))
            {
                roleMgr.Create(new Role("User"));
            }
            User user = userMgr.FindByName(userName);
            if (user == null)
            {
                userMgr.Create(new User { UserName = userName, Email = email, FirstName = "Admin", LastName = "Admin" }, password);
                user = userMgr.FindByName(userName);
                user.Settings = new Entities.Settings();
                context.Settings.Add(user.Settings);
                userMgr.AddToRole(user.Id, roleName);
            }
            context.SaveChanges();
        }
    }
}
