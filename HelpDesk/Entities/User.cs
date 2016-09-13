using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpDesk.Entities
{
    public class User
    {
        public int UserID { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [Index(IsUnique = true)]
        [MaxLength(254)]
        public string Email { get; set; }

        [Required]
        public string HashedPassword { get; set; }

        [Required]
        public string Salt { get; set; }

        public string Phone { get; set; }
        public string MobilePhone { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public string Role { get; set; }

        public virtual ICollection<Ticket> CreatedTickets { get; set; }
        public virtual ICollection<Ticket> RequestedTickets { get; set; }
        public virtual ICollection<Ticket> AssignedTickets { get; set; }
    }
}