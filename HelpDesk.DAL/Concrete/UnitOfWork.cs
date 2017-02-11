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

        private IRepository<User> userRepository;
        private IRepository<Ticket> ticketRepository;
        private IRepository<Category> categoryRepository;
        private IRepository<Settings> settingsRepository;
        private IRepository<TicketsHistory> ticketsHistoryRepository;

        public IRepository<User> UserRepository
        {
            get
            {
                if (userRepository == null)
                    userRepository = new GenericRepository<User>(context);
                return userRepository;
            }
        }

        public IRepository<Ticket> TicketRepository
        {
            get
            {
                if (ticketRepository == null)
                    ticketRepository = new GenericRepository<Ticket>(context);
                return ticketRepository;
            }
        }

        public IRepository<Category> CategoryRepository
        {
            get
            {
                if (categoryRepository == null)
                    categoryRepository = new GenericRepository<Category>(context);
                return categoryRepository;
            }
        }

        public IRepository<Settings> SettingsRepository
        {
            get
            {
                if (settingsRepository == null)
                    settingsRepository = new GenericRepository<Settings>(context);
                return settingsRepository;
            }
        }

        public IRepository<TicketsHistory> TicketsHistoryRepository
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