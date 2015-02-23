using DD4T.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DD4T.Mvc.Html
{

        /// <summary>
        /// Helpers for resource
        /// </summary>
    public static class ResourceHelper
    {
        /// <summary>
        /// Using bundle and key parameters to find appropriate resource.
        /// Use as: Html.GetResource(bundle, key)
        /// </summary>
        /// <param name="helper">the html helper</param>
        /// <param name="bundle">resource bundle name</param>
        /// <param name="key">resource name</param>
        /// <returns>resource (translated) in current language</returns>
        public static string GetResource(this HtmlHelper helper, string bundle, string key)
        {
            return HttpContext.GetGlobalResourceObject(bundle, key) as string;
        }

        public static string GetResource(this HtmlHelper helper, string key)
        {
            return GetResource(helper, ConfigurationHelper.ResourcePath, key);
        }
    }
}
