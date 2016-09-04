﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string Phone { get; set; }
        public string MobilePhone { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public string Role { get; set; }

        public virtual ICollection<Ticket> CreatedTickets { get; set; }
        public virtual ICollection<Ticket> SolvedTickets { get; set; }
    }
}