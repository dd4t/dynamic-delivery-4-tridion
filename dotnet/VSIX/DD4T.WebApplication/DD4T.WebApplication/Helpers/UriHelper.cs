using System.Configuration;
using System.Text.RegularExpressions;

namespace DD4T.WebApplication.Classes
{
    public static class UriHelper
    {
        private static readonly Regex DEFAULTPAGEREGEX = new Regex(@".*/[^/\.]*(/?)$");

        /// <summary>
        ///  Returns a cleaned up url with the default page name added if required
        /// </summary>
        public static string ParseUrl(string pageId)
        {
            var defaultPageFileName = GetDefaultPageFileName();

            if (string.IsNullOrEmpty(pageId))
            {
                pageId = defaultPageFileName;
            }
            else
            {
                if (DEFAULTPAGEREGEX.IsMatch("/" + pageId))
                {
                    if (pageId.EndsWith("/"))
                    {
                        pageId += defaultPageFileName;
                    }
                    else
                    {
                        pageId += "/" + defaultPageFileName;
                    }
                }
            }
            return pageId;
        }

        /// <summary>
        /// Returns the current default html page
        /// </summary>
        public static string GetDefaultPageFileName()
        {
            return ConfigurationManager.AppSettings["DD4T.DefaultPage"] ?? "index.html";
        }
    }
}