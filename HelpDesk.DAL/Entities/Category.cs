using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.DAL.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}