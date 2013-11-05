using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;

namespace DD4T.Examples.Helpers
{
    public static class ResourceHelper
    {
        public static string GetResource(this HtmlHelper helper, string bundle, string key)
        {
            return HttpContext.GetGlobalResourceObject(bundle, key) as string;
        }
    }
}
