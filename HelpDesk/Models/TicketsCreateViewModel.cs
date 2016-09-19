using HelpDesk.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.Models
{
    public class TicketsCreateViewModel
    {
        [Required]
        [DisplayName("Requested by")]
        public int? RequestedByID { get; set; }

        [Required]
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