using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels.Users
{
    public class CreateViewModel
    {
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

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "The {0} must be at least {2} characters long.")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

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