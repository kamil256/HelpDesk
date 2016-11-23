﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpDesk.DAL.Entities
{
    public class Ticket
    {
        public int TicketID { get; set; }
        public string CreatedByID { get; set; }
        public string RequestedByID { get; set; }
        public string AssignedToID { get; set; }
        public DateTime CreatedOn { get; set; }

        [Required]
        public string Status { get; set; }
        public int? CategoryID { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
        public string Solution { get; set; }

        public virtual AppUser CreatedBy { get; set; }
        public virtual AppUser RequestedBy { get; set; }
        public virtual AppUser AssignedTo { get; set; }
        public virtual Category Category { get; set; }
    }
}