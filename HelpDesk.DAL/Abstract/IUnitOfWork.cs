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
        GenericRepository<Ticket> TicketRepository { get; }
        GenericRepository<Category> CategoryRepository { get; }
        GenericRepository<Settings> SettingsRepository { get; }
        GenericRepository<TicketsHistory> TicketsHistoryRepository { get; }
        void Save();
    }
}