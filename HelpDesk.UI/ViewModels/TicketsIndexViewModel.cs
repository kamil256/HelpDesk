using HelpDesk.DAL.Entities;
using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.UI.ViewModels
{
    public class TicketsIndexViewModel: ISortableViewModel
    {
        [DisplayName("Assigned to")]
        public string AssignedToID { get; set; }

        [DisplayName("Category")]
        public int? CategoryID { get; set; }
        public string Status { get; set; } = "All";
        public string Search { get; set; }
        public bool AdvancedSearch { get; set; }
        public string SortBy { get; set; } = "CreatedOn";
        public bool DescSort { get; set; } = true;
        public int Page { get; set; } = 1;

        public IEnumerable<AppUser> Admins { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        
        public IPagedList<HelpDesk.DAL.Entities.Ticket> Tickets { get; set; }
    }
}