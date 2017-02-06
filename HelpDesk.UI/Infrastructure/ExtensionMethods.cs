using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.Infrastructure
{
    public static class ExtensionMethods
    {
        public static string RemoveExcessSpaces(this string text)
        {
            if (text != null)
            {
                System.Text.RegularExpressions.Regex trimmer = new System.Text.RegularExpressions.Regex(@"\s\s+");
                return trimmer.Replace(text.Trim(), " ");
            }
            else
                return text;
        }
    }
}