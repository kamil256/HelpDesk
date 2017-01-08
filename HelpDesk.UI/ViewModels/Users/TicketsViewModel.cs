using HelpDesk.UI.ViewModels.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels.Users
{
    public class TicketsViewModel
    {
        public string UserId { get; set; }
        public IList<AdministratorDTO> Administrators { get; set; }
    }
}