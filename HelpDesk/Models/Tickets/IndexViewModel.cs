using HelpDesk.Entities;
using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.Models.Tickets
{
    public class IndexViewModel
    {
        public string Status { get; set; } = "All";
        public string AssignedToID { get; set; }
        public int? CategoryID { get; set; }
        public string Search { get; set; }
        public bool AdvancedSearch { get; set; }

        public string SortBy { get; set; } = "CreatedOn";
        public bool DescSort { get; set; } = true;

        public int Page { get; set; } = 1;

        public IEnumerable<Ticket> Tickets { get; set; }

        public class Ticket
        {
            public int Id { get; set; }
            public string CreatedOn { get; set; }
            public string RequestedBy { get; set; }
            public string Title { get; set; }
            public string Category { get; set; }
            public string Status { get; set; }
        }
    }
}