using HelpDesk.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.Models
{
    public class TicketsCreateViewModel
    {
        public User Requestor { get; set; }
        public SelectList Categories { get; set; }

        [Required]
        public int RequestedByID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Category field is required")]
        public int CategoryID { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }
}