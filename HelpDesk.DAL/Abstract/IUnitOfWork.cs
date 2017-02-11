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
        IRepository<User, string> UserRepository { get; }
        IRepository<Role, string> RoleRepository { get; }
        IRepository<Ticket, int> TicketRepository { get; }
        IRepository<Category, int> CategoryRepository { get; }
        IRepository<Settings, string> SettingsRepository { get; }
        IRepository<TicketsHistory, int> TicketsHistoryRepository { get; }
        void Save();
    }
}