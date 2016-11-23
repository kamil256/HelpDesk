using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels
{
    public interface ISortableViewModel
    {
        string SortBy { get; set; }
        bool DescSort { get; set; }
    }
}