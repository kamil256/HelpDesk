using HelpDesk.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.DAL
{
    public interface IUnitOfWork
    {
        UserRepository UserRepository { get; }
        RoleRepository RoleRepository { get; }
        GenericRepository<Ticket> TicketRepository { get; }
        GenericRepository<Category> CategoryRepository { get; }
        GenericRepository<AspNetUsersHistory> AspNetUsersHistoryRepository { get; }
        GenericRepository<TicketsHistory> TicketsHistoryRepository { get; }
        void Save();
    }
}