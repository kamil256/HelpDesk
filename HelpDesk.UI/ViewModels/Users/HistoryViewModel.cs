using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels.Users
{
    public class HistoryViewModel
    {
        public string UserId { get; set; }
        public List<Log> Logs { get; set; }

        public class Log
        {
            public DateTime Date { get; set; }
            public int TicketId { get; set; }
            public string Column { get; set; }
            public string NewValue { get; set; }
        }
    }
}