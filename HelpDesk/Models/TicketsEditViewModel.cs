using HelpDesk.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.Models
{
    public class TicketsEditViewModel
    {
        public int TicketID { get; set; }

        public User Requestor { get; set; }
        public SelectList Categories { get; set; }

        public User Creator { get; set; }

        public User Solver { get; set; }

        public string CreateDate { get; set; }

        public string SolveOrCloseDate { get; set; }

        // Only required when creating new ticket, because requestor may have been removed
        public int? RequestorID { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Category field is required")]
        public int Category { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string Solution { get; set; }
    }
}