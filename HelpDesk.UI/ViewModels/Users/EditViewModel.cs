using HelpDesk.UI.ViewModels.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels.Users
{
    public class EditViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "Last activity")]
        public string LastActivity { get; set; }

        [Display(Name = "Active account")]
        public bool Active { get; set; }

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        [Phone]
        [Display(Name = "Mobile phone")]
        public string MobilePhone { get; set; }

        public string Company { get; set; }
        public string Department { get; set; }
        public string Role { get; set; }
    }
}