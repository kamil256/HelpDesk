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
    public class TicketFilteringModel
    {
        public string UserId { get; set; }
        public string Status { get; set; } = "All";
        public string AssignedToID { get; set; }
        public int? CategoryID { get; set; }
        public string Search { get; set; }
        public bool AdvancedSearch { get; set; }

        public string SortBy { get; set; } = "Created on";
        public bool DescSort { get; set; } = true;

        public int Page { get; set; } = 1;
    }
}