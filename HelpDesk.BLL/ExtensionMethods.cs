using System.Text.RegularExpressions;

namespace HelpDesk.BBL.ExtensionMethods
{
    public static class ExtensionMethods
    {
        public static string RemoveExcessSpaces(this string text)
        {
            if (text != null)
            {
                Regex trimmer = new Regex(@"\s\s+");
                return trimmer.Replace(text.Trim(), " ");
            }
            else
                return text;
        }
    }
}