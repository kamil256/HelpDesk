using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.Models
{
    public class TicketsIndexViewModel
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int Category { get; set; } = 0;
        public string Status { get; set; } = "All";
        public string Search { get; set; }
        public string SortBy { get; set; } = "CreateDate";
        public bool DescSort { get; set; } = true;
        public int Page { get; set; } = 1;
        public IPagedList<HelpDesk.Entities.Ticket> Tickets { get; set; }
        public SelectList Categories { get; set; }
        public SelectList Statuses { get; set; }
    }
}