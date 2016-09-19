using HelpDesk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk
{
    public static class HelperMethods
    {
        public static MvcHtmlString SortableHeader(this HtmlHelper htmlHelper, string property, string display = null)
        {
            if (display == null)
                display = property;
            ISortableViewModel model = (ISortableViewModel)htmlHelper.ViewData.Model;
            TagBuilder span = new TagBuilder("span");
            span.MergeAttribute("onclick", $"sort('{property}')");
            span.MergeAttribute("style", "cursor: pointer; text-decoration: underline;");
            if (model.SortBy == property)
                span.InnerHtml = display + "&nbsp;" + (model.DescSort ? "\u25BC" : "\u25B2");
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