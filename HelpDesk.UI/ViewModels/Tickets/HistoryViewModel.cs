using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels.Tickets
{
    public class HistoryViewModel
    {
        public string TicketID { get; set; }

        public List<Log> Logs { get; set; }
    }
}