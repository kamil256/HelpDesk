using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.Models.Tickets
{
    public class TicketDTO
    {
        public int TicketId { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string RequestedBy { get; set; }
        public string AssignedTo { get; set; }
        public string CreatedById { get; set; }
        public string RequestedById { get; set; }
        public string AssignedToId { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
    }
}