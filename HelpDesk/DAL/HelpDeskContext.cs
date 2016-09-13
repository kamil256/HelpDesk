using HelpDesk.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace HelpDesk.DAL
{
    public class HelpDeskContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>().HasOptional(t => t.CreatedBy).WithMany(u => u.CreatedTickets).HasForeignKey(t => t.CreatedByID).WillCascadeOnDelete(false);
            modelBuilder.Entity<Ticket>().HasOptional(t => t.RequestedBy).WithMany(u => u.RequestedTickets).HasForeignKey(t => t.RequestedByID).WillCascadeOnDelete(false);
            modelBuilder.Entity<Ticket>().HasOptional(t => t.AssignedTo).WithMany(u => u.AssignedTickets).HasForeignKey(t => t.AssignedToID).WillCascadeOnDelete(false);
        }
    }
}