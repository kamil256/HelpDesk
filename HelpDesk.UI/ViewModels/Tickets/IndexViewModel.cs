using HelpDesk.UI.ViewModels.Categories;
using HelpDesk.UI.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels.Tickets
{
    public class IndexViewModel
    {
        public IList<AdministratorDTO> Administrators { get; set; }
        public IList<CategoryDTO> Categories { get; set; }
    }
}