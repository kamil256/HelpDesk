using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HelpDesk.Entities;
using System.Data.Entity;

namespace HelpDesk.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HelpDeskContext context = new HelpDeskContext();

        private UserRepository userRepository;
        private RoleRepository roleRepository;
        private GenericRepository<Ticket> ticketRepository;
        private GenericRepository<Category> categoryRepository;
        private GenericRepository<AspNetUsersHistory> aspNetUsersHistoryRepository;
        private GenericRepository<TicketsHistory> ticketsHistoryRepository;

        public UserRepository UserRepository
        {
            get
            {
                if (userRepository == null)
                    userRepository = new UserRepository();
                return userRepository;
            }
        }

        public RoleRepository RoleRepository
        {
            get
            {
                if (roleRepository == null)
                    roleRepository = new RoleRepository();
                return roleRepository;
            }
        }

        public GenericRepository<Ticket> TicketRepository
        {
            get
            {
                if (ticketRepository == null)
                    ticketRepository = new GenericRepository<Ticket>(context);
                return ticketRepository;
            }
        }

        public GenericRepository<Category> CategoryRepository
        {
            get
            {
                if (categoryRepository == null)
                    categoryRepository = new GenericRepository<Category>(context);
                return categoryRepository;
            }
        }

        public GenericRepository<AspNetUsersHistory> AspNetUsersHistoryRepository
        {
            get
            {
                if (aspNetUsersHistoryRepository == null)
                    aspNetUsersHistoryRepository = new GenericRepository<AspNetUsersHistory>(context);
                return aspNetUsersHistoryRepository;
            }
        }

        public GenericRepository<TicketsHistory> TicketsHistoryRepository
        {
            get
            {
                if (ticketsHistoryRepository == null)
                    ticketsHistoryRepository = new GenericRepository<TicketsHistory>(context);
                return ticketsHistoryRepository;
            }
        }

        public void Save()
        {
            context.SaveChanges();
        }
    }
}