using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.Models.Tickets
{
    public class PagedTickets
    {
        public IEnumerable<TicketDTO> Tickets { get; set; }
        public int NumberOfPages { get; set; }
    }
}