﻿using HelpDesk.DAL.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
//using HelpDesk.Entities;

namespace HelpDesk.DAL.Concrete
{
    public class HelpDeskContext : IdentityDbContext<HelpDesk.DAL.Entities.User>
    {
        public HelpDeskContext() : base("HelpDeskContext") { }

        static HelpDeskContext()
        {
            Database.SetInitializer<HelpDeskContext>(new IdentityDbInit());
        }

        public static HelpDeskContext Create()
        {
            return new HelpDeskContext();
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Category> Categories { get; set; }        
        public DbSet<UsersHistory> AspNetUsersHistory { get; set; }
        public DbSet<TicketsHistory> TicketsHistory { get; set; }
        public DbSet<Settings> Settings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Ticket>().HasOptional(t => t.CreatedBy).WithMany(u => u.CreatedTickets).HasForeignKey(t => t.CreatedByID).WillCascadeOnDelete(false);
            modelBuilder.Entity<Ticket>().HasOptional(t => t.RequestedBy).WithMany(u => u.RequestedTickets).HasForeignKey(t => t.RequestedByID).WillCascadeOnDelete(false);
            modelBuilder.Entity<Ticket>().HasOptional(t => t.AssignedTo).WithMany(u => u.AssignedTickets).HasForeignKey(t => t.AssignedToID).WillCascadeOnDelete(false);
        }
    }

    public class IdentityDbInit : DropCreateDatabaseIfModelChanges<HelpDeskContext>
    {
        protected override void Seed(HelpDeskContext context)
        {
            PerformInitialSetup(context);
            base.Seed(context);
        }

        public void PerformInitialSetup(HelpDeskContext context)
        {
            base.Seed(context);
            UserManager userMgr = new UserManager(new UserStore<User>(context));
            RoleManager roleMgr = new RoleManager(new RoleStore<Role>(context));
            string roleName = "Admin";
            string userName = "admin@example.com";
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
                userMgr.Create(new User { UserName = userName, Email = email }, password);
                user = userMgr.FindByName(userName);
            }
            user.Settings = new Entities.Settings();
            context.Settings.Add(user.Settings);
            {
                userMgr.AddToRole(user.Id, roleName);
            }
            context.SaveChanges();
        }
    }
}