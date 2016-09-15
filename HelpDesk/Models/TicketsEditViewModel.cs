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

        public User CreatedBy { get; set; }

        // Only required when creating new ticket, because requestor may have been removed
        public int? RequestedByID { get; set; }
        public User RequestedBy { get; set; }

        public int? AssignedToID { get; set; }
        public User AssignedTo { get; set; }

        public string CreatedOn { get; set; }

        [Required]
        public string StatusID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Category field is required")]
        public int CategoryID { get; set; }
        public SelectList Categories { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
        public string Solution { get; set; }
    }
}