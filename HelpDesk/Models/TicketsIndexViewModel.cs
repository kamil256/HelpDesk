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
        // Status values: New, In progress, Solved, Closed
        public string Status { get; set; } = "All";
        public int AssignedTo { get; set; } = 0;
        public int Category { get; set; } = 0;
        public string Search { get; set; }
        public string SortBy { get; set; } = "CreateDate";
        public bool DescSort { get; set; } = true;
        public int Page { get; set; } = 1;

        public SelectList Statuses { get; set; }
        public SelectList AdminsList { get; set; }
        public SelectList Categories { get; set; }
        
        public IPagedList<HelpDesk.Entities.Ticket> Tickets { get; set; }
    }
}