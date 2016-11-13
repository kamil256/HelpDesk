using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpDesk.Entities
{
    [Table("TicketsHistory")]
    public class TicketsHistory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TicketsHistoryId { get; set; }

        public DateTime ChangeDate { get; set; }
        public string ChangeAuthorId { get; set; }

        public string TicketId { get; set; }
        public string ActionType { get; set; }
        public string ColumnName { get; set; }        
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}