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
    public class TicketsIndexOwnViewModel: ISortableViewModel
    {
        public string Search { get; set; }
        public bool AdvancedSearch { get; set; }
        public string SortBy { get; set; } = "CreatedOn";
        public bool DescSort { get; set; } = true;
        public int Page { get; set; } = 1;

        public IPagedList<HelpDesk.Entities.Ticket> Tickets { get; set; }
    }
}