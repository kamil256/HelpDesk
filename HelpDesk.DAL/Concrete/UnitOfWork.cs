﻿using System;
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

        private GenericRepository<Ticket> ticketRepository;
        private GenericRepository<Category> categoryRepository;
        private GenericRepository<Settings> settingsRepository;
        private GenericRepository<UsersHistory> aspNetUsersHistoryRepository;
        private GenericRepository<TicketsHistory> ticketsHistoryRepository;

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

        public GenericRepository<Settings> SettingsRepository
        {
            get
            {
                if (settingsRepository == null)
                    settingsRepository = new GenericRepository<Settings>(context);
                return settingsRepository;
            }
        }

        public GenericRepository<UsersHistory> AspNetUsersHistoryRepository
        {
            get
            {
                if (aspNetUsersHistoryRepository == null)
                    aspNetUsersHistoryRepository = new GenericRepository<UsersHistory>(context);
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