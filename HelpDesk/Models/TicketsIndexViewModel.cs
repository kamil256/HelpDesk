using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.Models
{
    public class TicketsIndexViewModel
    {
        public string Search { get; set; }
        public string SortBy { get; set; } = "CreateDate";
        public bool DescSort { get; set; } = true;
        public int Page { get; set; } = 1;
        public IPagedList<HelpDesk.Entities.Ticket> Tickets { get; set; }
    }
}