using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels
{
    public class TicketsHistoryViewModel
    {
        public string TicketID { get; set; }

        public List<Log> Logs { get; set; }
    }
}