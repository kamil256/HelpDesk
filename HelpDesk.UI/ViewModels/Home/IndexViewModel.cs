using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels.Home
{
    public class IndexViewModel
    {
        public int TotalTicketsCount { get; set; }        
        public int TotalUsersCount { get; set; }
        public int LoggedInUsersCount { get; set; }

        public int CreatedTickets { get; set; }
        public int RequestedTickets { get; set; }
        public int SolvedTicketsPercentage { get; set; }

        public Dictionary<string, int> Last7DaysTicketsByStatusCounts { get; set; }
        public Dictionary<string, int> Last30DaysTicketsByStatusCounts { get; set; }
        public Dictionary<string, int> OlderThan30DaysTicketsByStatusCounts { get; set; }

        public string CurrentMonthName { get; set; }
        public string LastMonthName { get; set; }
        public string MonthBeforeLastName { get; set; }

        public int YearInCurrentMonth { get; set; }
        public int YearInLastMonth { get; set; }
        public int YearInMonthBeforeLast { get; set; }

        public List<Category> CurrentMonthTicketsByCategoryCounts { get; set; }
        public List<Category> LastMonthTicketsByCategoryCounts { get; set; }
        public List<Category> MonthBeforeLastTicketsByCategoryCounts { get; set; }

        public class Category
        {
            public string Name { get; set; }
            public int TicketsCount { get; set; }
        }
    }
}