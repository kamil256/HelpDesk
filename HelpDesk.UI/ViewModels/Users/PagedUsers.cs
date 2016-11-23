using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels.Users
{
    public class PagedUsers
    {
        public IEnumerable<UserDTO> Users { get; set; }
        public int NumberOfPages { get; set; }
    }
}