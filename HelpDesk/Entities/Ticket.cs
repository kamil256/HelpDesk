using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpDesk.Entities
{
    public class Ticket
    {
        public int TicketID { get; set; }
        public int RequestorID { get; set; }
        public int? SolverID { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? SolveOrCloseDate { get; set; }
        public string status { get; set; }
        public int CategoryID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Solution { get; set; }

        public virtual User Requestor { get; set; }
        public virtual User Solver { get; set; }
        public virtual Category Category { get; set; }
    }
}