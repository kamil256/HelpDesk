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
        GenericRepository<Ticket> TicketRepository { get; }
        GenericRepository<Category> CategoryRepository { get; }
        void Save();
    }
}