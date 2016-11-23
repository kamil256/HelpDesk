namespace HelpDesk.UI.Migrations
{
    using Microsoft.AspNet.Identity.Owin;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web;

    internal sealed class Configuration : DbMigrationsConfiguration<HelpDesk.DAL.Concrete.HelpDeskContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(HelpDesk.DAL.Concrete.HelpDeskContext context)
        {
            //context.Roles.AddOrUpdate
            //(
            //    new AppRole { Name = "Admin" },
            //    new AppRole { Name = "User" }
            //);
            //AppUser user = new AppUser
            //{
            //    FirstName = "Admin",
            //    LastName = "Admin",
            //    Email = "admin@example.com"
            //};
            //AppUserManager userManager = System.Web.HttpContext.Current.Request.GetOwinContext().GetUserManager<AppUserManager>();
            //if (userManager.Users.SingleOrDefault(u => u.Email == user.Email) == null)
            //    userManager.CreateAsync(user, "Password");


            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
