using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels.Tickets
{
    public class HistoryViewModel
    {
        public int TicketID { get; set; }
        public List<Log> Logs { get; set; }

        public class Log
        {
            public DateTime Date { get; set; }
            public string AuthorId { get; set; }
            public string AuthorName { get; set; }
            public string Column { get; set; }
            public string NewValue { get; set; }
        }
    }

    
}