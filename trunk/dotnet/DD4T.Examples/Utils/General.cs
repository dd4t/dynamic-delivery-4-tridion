using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DD4T.Examples.Utils
{
    public static class General
    {

        public static string AdjustUrl(this string url)
        {
            return AdjustUrlToContext(url);
        }

        public static string AdjustUrlToContext(string url)
        {
            string appPath = HttpContext.Current.Request.ApplicationPath;

            if (string.IsNullOrEmpty(appPath) || appPath.Equals("/"))
                return url;
            return appPath + url;
        }

    }
}
