using HelpDesk.DAL.Entities;
using HelpDesk.UI.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.UI.ViewModels.Tickets
{
    public class EditViewModel
    {
        public int TicketId { get; set; }

        [DisplayName("Create date")]
        public string CreateDate { get; set; }

        [DisplayName("Requester")]
        public string RequesterId { get; set; }

        [DisplayName("Assigned user")]
        public string AssignedUserId { get; set; }
        
        [Required]
        public string Status { get; set; }

        [DisplayName("Category")]
        public int? CategoryId { get; set; }
        
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
        public string Solution { get; set; }

        [DisplayName("Creator")]
        public User Creator { get; set; }        
        public UserDTO Requester { get; set; }

        // TODO: is displayname attribute necessary?
        [DisplayName("Assigned user")]
        public User AssignedUser { get; set; }

        public IEnumerable<User> Administrators { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}