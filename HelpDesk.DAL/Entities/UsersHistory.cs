using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpDesk.DAL.Entities
{
    public class UsersHistory
    {
        public int UsersHistoryId { get; set; }
        public DateTime Date { get; set; }

        [Required]
        public string AuthorId { get; set; }

        public string UserId { get; set; }
        public string Column { get; set; }        
        public string NewValue { get; set; }
    }
}