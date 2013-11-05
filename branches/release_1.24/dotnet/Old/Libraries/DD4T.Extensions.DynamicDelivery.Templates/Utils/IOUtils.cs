using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace DD4T.Extensions.DynamicDelivery.Templates.Utils
{
   public class IOUtils
   {
      public static Stream LoadResourceAsStream(String resourcePath)
      {
         Assembly ass = Assembly.GetCallingAssembly();
         Stream stream = ass.GetManifestResourceStream(resourcePath);

         if (stream == null)
            throw new Exception("Could not locate embedded resource '" + resourcePath + "' in assembly");
         return stream;
      }
   }
}
