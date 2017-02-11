using HelpDesk.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.BLL
{
    public class PagedUsersList
    {
        public IEnumerable<User> Users { get; set; }
        public int NumberOfPages { get; set; }
        public int FoundItemsCount { get; set; }
        public int TotalItemsCount { get; set; }
    }
}
