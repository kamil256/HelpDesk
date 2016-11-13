using HelpDesk.Entities;
using HelpDesk.Models.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpDesk.Models
{
    public class TicketsHistoryViewModel
    {
        public string TicketID { get; set; }

        public List<Log> Logs { get; set; }
    }
}