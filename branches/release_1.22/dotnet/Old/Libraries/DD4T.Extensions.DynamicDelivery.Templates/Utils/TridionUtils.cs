using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using System.Text.RegularExpressions;

namespace DD4T.Extensions.DynamicDelivery.Templates.Utils
{
   public class TridionUtils
   {

      private static TemplatingLogger log = TemplatingLogger.GetLogger(typeof(TridionUtils));

      // The path separator used in published paths
      public const string PathSeparator = "/";

      public static Publication GetPublicationFromContext(Package package, Engine engine)
      {
         TemplatingLogger log = TemplatingLogger.GetLogger(typeof(TridionUtils));
         Publication myPublication = null;
         Item contextItem = package.GetByName("Page");

         if (contextItem != null)
         {
            log.Info("(GetPublicationFromContext) Retrieving for context publication from the Page");
            TcmUri uriDataSource = new TcmUri(contextItem.GetAsSource().GetValue("ID"));
            Page mycontextItem = engine.GetObject(uriDataSource) as Page;
            myPublication = (Publication)mycontextItem.ContextRepository;
         }
         else
         {
            log.Info("(GetPublicationFromContext) Retrieving for context publication from the Component");
            contextItem = package.GetByName("Component");
            TcmUri uriDataSource = new TcmUri(contextItem.GetAsSource().GetValue("ID"));
            Component mycontextItem = engine.GetObject(uriDataSource) as Component;
            myPublication = (Publication)mycontextItem.ContextRepository;
         }
         log.Info("(GetPublicationFromContext) Context publication is:" + myPublication.Title);
         return myPublication;
      }

      public static TcmUri GetLocalUri(TcmUri uriPublication, TcmUri uriItem)
      {
         TcmUri uriReturn = new TcmUri(uriItem.ItemId, uriItem.ItemType, uriPublication.ItemId);
         TemplatingLogger log = TemplatingLogger.GetLogger(typeof(TemplateUtilities));
         log.Info("(getLocalUri)Old URI was:" + uriItem.ToString());
         log.Info("(getLocalUri)New URI is:" + uriReturn.ToString());
         return uriReturn;
      }

      /// <summary>
      /// Construct a filename for an item.
      /// </summary>
      /// <remarks>Based on the properties of the item. The filename property must be set, but
      /// there can be other aspects set on the item that are taken into account in the filename</remarks>
      /// <param name="item"></param>
      /// <returns></returns>
      public static string ConstructFileName(Item item)
      {
         IDictionary<string, string> properties = item.Properties;
         string fileName = properties[Item.ItemPropertyFileName];
         if (fileName == null)
         {
            log.Warning(String.Format("No filename set in property {0}", Item.ItemPropertyFileName));
            return null;
         }

         // Handle prefix
         fileName = GetPropertyValue(item, Item.ItemPropertyFileNamePrefix) + fileName;

         // Handle subfolder (todo: fix this, ItemPropertyFileNameSubFolder does not exist!!
         //string subFolder = GetPropertyValue(item, Item.ItemPropertyFileNameSubFolder);
         //if (subFolder != "") {
         //    if (subFolder.StartsWith("/")) {
         //        // Strip of leading /
         //        subFolder = subFolder.Substring(1);
         //    }
         //    if (!subFolder.EndsWith(PathSeparator)) {
         //        // Ensure there is always a separator at the end
         //        subFolder += PathSeparator;
         //    }
         //    fileName = subFolder + fileName;
         //}

         // Handle extension
         int extensionDotIndex = fileName.LastIndexOf(".");
         string overriddenExtension = GetPropertyValue(item, Item.ItemPropertyFileNameExtension);
         if (overriddenExtension != "")
         {
            if (extensionDotIndex != -1)
            {
               // replace extension, so strip current one first
               fileName = fileName.Substring(0, extensionDotIndex);
            }
            // In all cases there will be an extension now
            extensionDotIndex = fileName.Length;

            fileName += "." + overriddenExtension;
         }

         // Handle filename suffix
         string suffix = GetPropertyValue(item, Item.ItemPropertyFileNameSuffix);
         if (suffix != "")
         {
            if (extensionDotIndex == -1)
            {
               fileName += suffix;
            }
            else
            {
               fileName = fileName.Substring(0, extensionDotIndex) + suffix + fileName.Substring(extensionDotIndex);
            }
         }

         return fileName;
      }

      /// <summary>
      /// Get a property value from an item, or an empty string if the property value
      /// is missing or null.
      /// </summary>
      /// <remarks>Simple helper for ConstructFileName, to access item property values</remarks>
      /// <param name="item">The item to determine the property value for</param>
      /// <param name="propertyName">The name of the property to retrieve</param>
      /// <returns>The property value, or an empty string if the property is missing or null</returns>
      public static string GetPropertyValue(Item item, string propertyName)
      {
         string value;
         item.Properties.TryGetValue(propertyName, out value);
         return (value != null ? value : "");
      }

      public static string StripTcdlTags(string input)
      {
         return Regex.Replace(input, @"</?tcdl:.*?>", string.Empty);
      }
   }
}
