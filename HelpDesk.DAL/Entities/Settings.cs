using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpDesk.DAL.Entities
{
    [Table("Settings")]
    public class Settings
    {
        [ForeignKey("User")]
        public string SettingsId { get; set; }
        public bool NewTicketsNotifications { get; set; } = true;
        public bool SolvedTicketsNotifications { get; set; } = true;
        public bool ClosedTicketsNotifications { get; set; } = true;
        public int UsersPerPage { get; set; } = 10;
        public int TicketsPerPage { get; set; } = 10;

        public virtual User User { get; set; }
    }
}