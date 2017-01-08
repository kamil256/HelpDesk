using HelpDesk.DAL.Entities;
using HelpDesk.UI.ViewModels.Categories;
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
    public class CreateViewModel
    {
        [Required]
        [DisplayName("Requester")]
        public string RequesterId { get; set; }

        [DisplayName("Category")]
        public int? CategoryId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public UserDTO Requester { get; set; }
        public IEnumerable<CategoryDTO> Categories { get; set; }
    }
}