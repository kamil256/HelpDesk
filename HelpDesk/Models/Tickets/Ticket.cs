using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.Models.Tickets
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public string CreatedOn { get; set; }
        public string RequestedBy { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
    }
}