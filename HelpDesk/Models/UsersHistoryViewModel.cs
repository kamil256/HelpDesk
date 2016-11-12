using HelpDesk.Entities;
using HelpDesk.Models.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpDesk.Models
{
    public class UsersHistoryViewModel
    {
        public string UserID { get; set; }

        public List<Log> Logs { get; set; }
    }

    public class Log
    {
        public DateTime Date { get; set; }
        public string Content { get; set; }
    }
}