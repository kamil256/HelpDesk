using HelpDesk.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels.Settings
{
    public class IndexViewModel
    {
        [Required]
        public bool NewTicketsNotifications { get; set; }

        [Required]
        public bool SolvedTicketsNotifications { get; set; }

        [Required]
        [RegularExpression("([1-9][0-9]*)")]
        public int UsersPerPage { get; set; }

        [Required]
        [RegularExpression("([1-9][0-9]*)")]
        public int TicketsPerPage { get; set; }

        public Category[] Categories { get; set; }

        // TODO: when I change categories Id to string, it can be changed to int?[] not set to zero in view
        public int[] CategoriesId { get; set; }
        public string[] CategoriesName { get; set; }
    }
}