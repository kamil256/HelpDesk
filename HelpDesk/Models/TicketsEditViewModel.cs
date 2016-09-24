using HelpDesk.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.Models
{
    public class TicketsEditViewModel
    {
        public int TicketID { get; set; }

        [DisplayName("Requested by")]
        public string RequestedByID { get; set; }

        [DisplayName("Assigned to")]
        public string AssignedToID { get; set; }
        
        [Required]
        public string Status { get; set; }

        [DisplayName("Category")]
        public int? CategoryID { get; set; }
        
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
        public string Solution { get; set; }

        [DisplayName("Created by")]
        public AppUser CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public AppUser RequestedBy { get; set; }
        public AppUser AssignedTo { get; set; }
        public IEnumerable<AppUser> Admins { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}