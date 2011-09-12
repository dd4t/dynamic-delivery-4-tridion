using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Tridion.ContentManager.Templating;

namespace DD4T.Templates.Utils
{
   public class GeneralUtils
   {
      static TemplatingLogger log = TemplatingLogger.GetLogger(typeof(GeneralUtils));
      static DateTime startTime = DateTime.Now;
      public static void TimedLog(string Message)
      {
         DateTime currentTime = DateTime.Now;
         TimeSpan duration = currentTime - startTime;
         log.Debug(duration.TotalMilliseconds + " - " + Message);
      }
      public static void ResetLogTimer()
      {
         startTime = DateTime.Now;
      }
   }
}
