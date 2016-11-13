using HelpDesk.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.Models
{
    public class SettingsIndexViewModel
    {
        public bool NewTicketsNotifications { get; set; }
        public bool SolvedTicketsNotifications { get; set; }
        public int? UsersPerPage { get; set; }
        public int? TicketsPerPage { get; set; }

        public Category[] Categories { get; set; }
        public string[] CategoriesName { get; set; }
        public int[] CategoriesId { get; set; }
    }
}