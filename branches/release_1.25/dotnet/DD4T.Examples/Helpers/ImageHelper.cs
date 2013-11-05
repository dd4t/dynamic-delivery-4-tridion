using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DD4T.Examples.Helpers
{
    public static class ImageHelper
    {
        public static string ResizeToWidth(this string url, int width)
        {
            return ResizeToDimension(url, width, "w");
        }

        public static string ResizeToHeight(this string url, int height)
        {
            return ResizeToDimension(url, height, "h");
        }
        private static string ResizeToDimension(string url, int val, string dimension)
        {
            Regex re = new Regex(@"(\.[a-zA-Z]+$)");
            if (re.IsMatch(url))
            {
                string ext = re.Match(url).Groups[1].ToString();
                return re.Replace(url, "_" + dimension + Convert.ToString(val) + ext);
            }
            return url;
        }
    }
}
