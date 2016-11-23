using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpDesk.DAL.Entities
{
    [Table("UsersHistory")]
    public class UsersHistory
    {
        public int UsersHistoryId { get; set; }

        public DateTime ChangeDate { get; set; }
        public string ChangeAuthorId { get; set; }

        public string UserId { get; set; }
        public string ActionType { get; set; }
        public string ColumnName { get; set; }        
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}