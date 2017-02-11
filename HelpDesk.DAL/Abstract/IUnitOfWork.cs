using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.DAL.Abstract
{
    public interface IUnitOfWork
    {
        IRepository<User> UserRepository { get; }
        IRepository<Ticket> TicketRepository { get; }
        IRepository<Category> CategoryRepository { get; }
        IRepository<Settings> SettingsRepository { get; }
        IRepository<TicketsHistory> TicketsHistoryRepository { get; }
        void Save();
    }
}