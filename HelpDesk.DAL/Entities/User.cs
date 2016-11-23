using System;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace HelpDesk.DAL.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string MobilePhone { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }

        public virtual ICollection<Ticket> CreatedTickets { get; set; }
        public virtual ICollection<Ticket> RequestedTickets { get; set; }
        public virtual ICollection<Ticket> AssignedTickets { get; set; }
        public virtual Settings Settings { get; set; }
    }
}