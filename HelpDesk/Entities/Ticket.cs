using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpDesk.Entities
{
    public class Ticket
    {
        public int TicketID { get; set; }
        public int? CreatedByID { get; set; }
        public int? RequestedByID { get; set; }
        public int? AssignedToID { get; set; }
        public DateTime CreateDate { get; set; }

        [Required]
        public string Status { get; set; }
        public int CategoryID { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
        public string Solution { get; set; }

        public virtual User CreatedBy { get; set; }
        public virtual User RequestedBy { get; set; }
        public virtual User AssignedTo { get; set; }
        public virtual Category Category { get; set; }
    }
}