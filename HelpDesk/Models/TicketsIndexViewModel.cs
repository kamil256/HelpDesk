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
        // Status values: New, In progress, Solved, Closed
        public string Status { get; set; } = "All";

        [DisplayName("Assigned to")]
        public int? AssignedTo { get; set; }
        public int? Category { get; set; }
        public string Search { get; set; }
        public bool AdvancedSearch { get; set; }
        public string SortBy { get; set; } = "CreatedOn";
        public bool DescSort { get; set; } = true;
        public int Page { get; set; } = 1;

        public SelectList Statuses { get; set; }
        public IEnumerable<User> Admins { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        
        public IPagedList<HelpDesk.Entities.Ticket> Tickets { get; set; }
    }
}