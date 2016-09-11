using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpDesk.Models
{
    public class UsersEditViewModel
    {
        public int UserID { get; set; }

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [MaxLength(254)]
        public string Email { get; set; }

        [Phone]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Phone]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Mobile phone")]
        public string MobilePhone { get; set; }

        public string Company { get; set; }

        public string Department { get; set; }

        public string Role { get; set; }
    }
}