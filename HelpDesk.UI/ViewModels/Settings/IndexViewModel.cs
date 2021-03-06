﻿using HelpDesk.DAL.Entities;
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
        public bool AssignedTicketsNotifications { get; set; }

        [Required]
        public bool SolvedTicketsNotifications { get; set; }

        [Required]
        public bool ClosedTicketsNotifications { get; set; }

        [Required]
        [RegularExpression("([1-9][0-9]*)")]
        public int UsersPerPage { get; set; }

        [Required]
        [RegularExpression("([1-9][0-9]*)")]
        public int TicketsPerPage { get; set; }

        public List<Category> Categories { get; set; }
    }
}