using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.Models.Users
{
    public class UserFilteringModel
    {
        public string Role { get; set; }
        public string Search { get; set; }        
        public string SortBy { get; set; } = "Last name";
        public bool DescSort { get; set; } = false;
        public int Page { get; set; } = 1;
        public bool IgnorePaging { get; set; } = false;
    }
}