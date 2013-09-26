using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace DD4T.Examples.Helpers
{
    public static class DateHelper
    {
        public static MvcHtmlString FormatDate(this DateTime value)
        {
            // language format of date is handled by MVC based onselected language on website
            return FormatDate(value, "{0: d MMMM yyyy}");
        }

        public static MvcHtmlString FormatDate(this DateTime value, string dateFormat)
        {
            return new MvcHtmlString(string.Format(dateFormat, value));
        }


    }
}
