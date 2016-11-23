using HelpDesk.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.UI
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
        public static MvcHtmlString TextBoxWithNumber(this HtmlHelper htmlHelper, string name, int value)
        {
            TagBuilder span1 = new TagBuilder("span");
            span1.AddCssClass("glyphicon glyphicon-chevron-left");
            span1.MergeAttribute("onclick", $"if (isNaN($('input[name={name}]').val())) $('input[name={name}]').val(1); else if ($('input[name={name}]').val() > 1) $('input[name={name}]').val(Number($('input[name={name}]').val()) - 1)");
            TagBuilder td1 = new TagBuilder("td");
            td1.InnerHtml = span1.ToString();

            TagBuilder input = new TagBuilder("input");
            input.AddCssClass("form-control");
            input.MergeAttribute("name", name);
            input.MergeAttribute("value", value.ToString());
            TagBuilder td2 = new TagBuilder("td");
            td2.InnerHtml = input.ToString();

            TagBuilder span2 = new TagBuilder("span");
            span2.AddCssClass("glyphicon glyphicon-chevron-right");
            span2.MergeAttribute("onclick", $"if (isNaN($('input[name={name}]').val())) $('input[name={name}]').val(1); else $('input[name={name}]').val(Number($('input[name={name}]').val()) + 1)");
            TagBuilder td3 = new TagBuilder("td");
            td3.InnerHtml = span2.ToString();

            TagBuilder tr = new TagBuilder("tr");
            tr.InnerHtml = td1.ToString();
            tr.InnerHtml += td2.ToString();
            tr.InnerHtml += td3.ToString();

            TagBuilder table = new TagBuilder("table");
            table.AddCssClass("text-box-with-number");
            table.InnerHtml = tr.ToString();

            return new MvcHtmlString(table.ToString());
        }

        public static MvcHtmlString SortableCategoryItem(this HtmlHelper htmlHelper, int id, string value)
        {
            TagBuilder span1 = new TagBuilder("span");
            span1.AddCssClass("glyphicon");
            span1.AddCssClass("glyphicon-option-vertical"/*"glyphicon-resize-vertical"*/);
            TagBuilder td1 = new TagBuilder("td");
            td1.InnerHtml = span1.ToString();

            TagBuilder input1 = new TagBuilder("input");
            input1.AddCssClass("form-control");
            input1.MergeAttribute("name", "categoriesName");
            input1.MergeAttribute("value", value);
            TagBuilder input2 = new TagBuilder("input");
            input2.MergeAttribute("type", "hidden");
            input2.MergeAttribute("name", "categoriesId");
            input2.MergeAttribute("value", id.ToString());
            TagBuilder td2 = new TagBuilder("td");
            td2.InnerHtml = input1.ToString();
            td2.InnerHtml += input2.ToString();

            TagBuilder span2 = new TagBuilder("span");
            span2.AddCssClass("glyphicon");            
            span2.AddCssClass("glyphicon-remove");
            span2.MergeAttribute("onclick", "removeCategory(event)");
            TagBuilder td3 = new TagBuilder("td");
            td3.InnerHtml = span2.ToString();

            TagBuilder tr = new TagBuilder("tr");
            tr.InnerHtml = td1.ToString();
            tr.InnerHtml += td2.ToString();
            tr.InnerHtml += td3.ToString();

            TagBuilder table = new TagBuilder("table");
            table.InnerHtml = tr.ToString();

            TagBuilder li = new TagBuilder("li");
            li.InnerHtml = table.ToString();

            return new MvcHtmlString(li.ToString());
        }

        public static MvcHtmlString SortableCategoryLastItem(this HtmlHelper htmlHelper)
        {
            TagBuilder input1 = new TagBuilder("input");
            input1.AddCssClass("form-control");
            input1.MergeAttribute("id", "new-category");
            //input1.MergeAttribute("value", "");
            TagBuilder td1 = new TagBuilder("td");
            td1.InnerHtml = input1.ToString();

            TagBuilder span = new TagBuilder("span");
            span.AddCssClass("glyphicon");
            span.AddCssClass("glyphicon-plus");
            span.MergeAttribute("onclick", "addCategory()");
            TagBuilder td2 = new TagBuilder("td");
            td2.InnerHtml = span.ToString();

            TagBuilder tr = new TagBuilder("tr");
            tr.InnerHtml = td1.ToString();
            tr.InnerHtml += td2.ToString();
            
            TagBuilder table = new TagBuilder("table");
            table.InnerHtml = tr.ToString();

            TagBuilder div = new TagBuilder("div");
            div.MergeAttribute("id", "add-category");
            div.InnerHtml = table.ToString();

            return new MvcHtmlString(div.ToString());
        }
    }
}