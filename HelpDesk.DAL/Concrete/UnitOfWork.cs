using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Entities;

namespace HelpDesk.DAL.Concrete
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HelpDeskContext context = new HelpDeskContext();

        private IRepository<User, string> userRepository;
        private IRepository<Role, string> roleRepository;
        private IRepository<Ticket, int> ticketRepository;
        private IRepository<Category, int> categoryRepository;
        private IRepository<Settings, string> settingsRepository;
        private IRepository<TicketsHistory, int> ticketsHistoryRepository;

        public IRepository<User, string> UserRepository
        {
            get
            {
                if (userRepository == null)
                    userRepository = new GenericRepository<User, string>(context);
                return userRepository;
            }
        }

        public IRepository<Role, string> RoleRepository
        {
            get
            {
                if (roleRepository == null)
                    roleRepository = new GenericRepository<Role, string>(context);
                return roleRepository;
            }
        }

        public IRepository<Ticket, int> TicketRepository
        {
            get
            {
                if (ticketRepository == null)
                    ticketRepository = new GenericRepository<Ticket, int>(context);
                return ticketRepository;
            }
        }

        public IRepository<Category, int> CategoryRepository
        {
            get
            {
                if (categoryRepository == null)
                    categoryRepository = new GenericRepository<Category, int>(context);
                return categoryRepository;
            }
        }

        public IRepository<Settings, string> SettingsRepository
        {
            get
            {
                if (settingsRepository == null)
                    settingsRepository = new GenericRepository<Settings, string>(context);
                return settingsRepository;
            }
        }

        public IRepository<TicketsHistory, int> TicketsHistoryRepository
        {
            get
            {
                if (ticketsHistoryRepository == null)
                    ticketsHistoryRepository = new GenericRepository<TicketsHistory, int>(context);
                return ticketsHistoryRepository;
            }
        }

        public void Save()
        {
            context.SaveChanges();
        }
    }
}