using HelpDesk.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpDesk.Models
{
    public class UsersDetailsViewModel
    {
        public string UserID { get; set; }

        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        public string Phone { get; set; }

        [Display(Name = "Mobile phone")]
        public string MobilePhone { get; set; }

        public string Company { get; set; }

        public string Department { get; set; }

        public string Role { get; set; }
    }
}