using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.Models
{
    public class UsersIndexViewModel
    {
        public string Search { get; set; }
        public string SortBy { get; set; } = "LastName";
        public bool DescSort { get; set; } = false;
        public int Page { get; set; } = 1;
        public IPagedList<HelpDesk.Entities.User> Users { get; set; }
    }
}