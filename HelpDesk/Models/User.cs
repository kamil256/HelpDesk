using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpDesk.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Index(IsUnique = true)]
        [Display(Name = "Email")]
        [MaxLength(254)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [NotMapped]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [NotMapped]
        public string ConfirmPassword { get; set; }

        [Required]
        public string HashedPassword { get; set; }

        [Required]
        public string Salt { get; set; }

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

        public virtual ICollection<Ticket> CreatedTickets { get; set; }
        public virtual ICollection<Ticket> SolvedTickets { get; set; }
    }
}