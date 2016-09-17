using HelpDesk.Entities;
using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.Models
{
    public class TicketsIndexViewModel
    {
        public string Status { get; set; } = "All";
        public int? AssignedToID { get; set; }
        public int? CategoryID { get; set; }
        public string Search { get; set; }
        public bool AdvancedSearch { get; set; }
        public string SortBy { get; set; } = "CreatedOn";
        public bool DescSort { get; set; } = true;
        public int Page { get; set; } = 1;

        public IEnumerable<User> Admins { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        
        public IPagedList<HelpDesk.Entities.Ticket> Tickets { get; set; }
    }
}