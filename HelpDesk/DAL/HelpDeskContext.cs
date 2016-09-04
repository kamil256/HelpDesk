using HelpDesk.Models;
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
            modelBuilder.Entity<Ticket>().HasRequired(t => t.Requestor).WithMany(u => u.CreatedTickets).HasForeignKey(t => t.RequestorID).WillCascadeOnDelete(false);
            modelBuilder.Entity<Ticket>().HasRequired(t => t.Solver).WithMany(u => u.SolvedTickets).HasForeignKey(t => t.SolverID).WillCascadeOnDelete(false);
        }
    }
}