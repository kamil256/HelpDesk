using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk
{
    public static class HelperMethods
    {
        public static MvcHtmlString SortableTableColumnHeader(this HtmlHelper htmlHelper, string displayName, string sortString, string sortBy, bool descSort, string search, int page)
        {
            TagBuilder a = new TagBuilder("a");
            UrlHelper url = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            if(sortBy == sortString)
            {
                a.MergeAttribute("href", url.Action("Index", new { sortBy = sortString, descSort = !descSort, search = search, page = page }));
                a.InnerHtml = displayName + " " + (descSort ? "\u25BC" : "\u25B2");
            }
            else
            {
                a.MergeAttribute("href", url.Action("Index", new { sortBy = sortString, search = search, page = page }));
                a.InnerHtml = displayName;
            }
            return new MvcHtmlString(a.ToString());
        }

        public static MvcHtmlString SortableHeader(this HtmlHelper htmlHelper, string property, string display = null)
        {
            if (display == null)
                display = property;
            Models.TicketsIndexViewModel model = htmlHelper.ViewData.Model as Models.TicketsIndexViewModel;
            TagBuilder span = new TagBuilder("span");
            span.MergeAttribute("onclick", $"sort('{property}')");
            span.MergeAttribute("style", "cursor: pointer; text-decoration: underline;");
            if (model.SortBy == property)
                span.InnerHtml = display + " " + (model.DescSort ? "\u25BC" : "\u25B2");
            else
                span.InnerHtml = display;
            return new MvcHtmlString(span.ToString());
        }

        public static MvcHtmlString MarkSearchedString(this HtmlHelper htmlHelper, string text, string search)
        {
            string marked = text;
            if (!String.IsNullOrEmpty(text) && !String.IsNullOrEmpty(search))
            {
                int start = marked.ToLower().IndexOf(search.ToLower()), end;
                while (start != -1)
                {
                    end = start + search.Length;
                    marked = marked.Insert(end, "</mark>");
                    marked = marked.Insert(start, "<mark>");
                    start = marked.ToLower().IndexOf(search.ToLower(), end + "<mark></mark".Length);
                }
            }
            return MvcHtmlString.Create(marked);
        }
    }
}