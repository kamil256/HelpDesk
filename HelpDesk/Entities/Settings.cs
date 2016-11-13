using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpDesk.Entities
{
    [Table("Settings")]
    public class Settings
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public string SettingsId { get; set; }
        [ForeignKey("User")]
        public string SettingsId { get; set; }
        public bool NewTicketsNotifications { get; set; } = true;
        public bool SolvedTicketsNotifications { get; set; } = true;
        public int UsersPerPage { get; set; } = 10;
        public int TicketsPerPage { get; set; } = 10;

        public virtual AppUser User { get; set; }
    }
}