using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpDesk.DAL.Entities
{
    [Table("TicketsHistory")]
    public class TicketsHistory
    {
        public int TicketsHistoryId { get; set; }
        public DateTime Date { get; set; }

        [Required]
        public string AuthorId { get; set; }

        public int TicketId { get; set; }
        public string Column { get; set; }
        public string NewValue { get; set; }
    }
}