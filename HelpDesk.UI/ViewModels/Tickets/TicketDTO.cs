using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels.Tickets
{
    // TODO: remove unnecessary properties
    public class TicketDTO
    {
        public int TicketId { get; set; }
        public string CreateDate { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public string RequesterId { get; set; }
        public string RequesterName { get; set; }
        public string AssignedUserId { get; set; }
        public string AssignedUserName { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
    }
}