using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpDesk.Models
{
    public class Ticket
    {
        public int TicketID { get; set; }
        //[ForeignKey("User")]
        public int RequestorID { get; set; }
        //[ForeignKey("User")]
        public int SolverID { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime SolveOrCloseDate { get; set; }
        public int StatusID { get; set; }
        public int CategoryID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Solution { get; set; }

        public virtual User Requestor { get; set; }
        public virtual User Solver { get; set; }
        public virtual Status { get; set; }
        public virtual Category { get; set; }
    }
}