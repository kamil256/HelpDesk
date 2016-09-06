using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk
{
    public static class HelperMethods
    {
        public static MvcHtmlString SortableTableColumnHeader(this HtmlHelper htmlHelper, string displayName, string sortString, string sortBy, bool descSort)
        {
            TagBuilder a = new TagBuilder("a");
            UrlHelper url = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            if(sortBy == sortString)
            {
                a.MergeAttribute("href", url.Action("Index", new { sortBy = sortString, descSort = !descSort }));
                a.InnerHtml = displayName + " " + (descSort ? "\u25BC" : "\u25B2");
            }
            else
            {
                a.MergeAttribute("href", url.Action("Index", new { sortBy = sortString }));
                a.InnerHtml = displayName;
            }
            return new MvcHtmlString(a.ToString());
        }
    }
}