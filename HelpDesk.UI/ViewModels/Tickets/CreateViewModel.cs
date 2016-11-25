using HelpDesk.DAL.Entities;
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
        [DisplayName("Requested by")]
        public string RequestedByID { get; set; }

        [DisplayName("Category")]
        public int? CategoryID { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public User RequestedBy { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}