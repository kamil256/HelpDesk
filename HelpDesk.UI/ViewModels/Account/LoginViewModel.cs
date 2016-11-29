using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        // TODO: is ReturnURL necessary?
        public string ReturnUrl { get; set; }
    }
}