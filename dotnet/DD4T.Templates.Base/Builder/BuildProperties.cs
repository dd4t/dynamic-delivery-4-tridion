using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating;
using DD4T.Serialization;

namespace DD4T.Templates.Base.Builder
{
    public class BuildProperties
    {
        

        public static int DefaultLinkLevels = 1;
        public static bool DefaultResolveWidthAndHeight = false;
        public static bool DefaultPublishEmptyFields = false;
        public static bool DefaultFollowLinksPerField = false;
        public static SerializationFormats DefaultSerializationFormat = SerializationFormats.XML;
        public static bool DefaultOmitContextPublications = false;
        public static bool DefaultOmitOwningPublications = false;
        public static bool DefaultOmitFolders = false;

        public int LinkLevels { get; set; }
        public bool ResolveWidthAndHeight { get; set; }
        public bool PublishEmptyFields { get; set; }
        public bool FollowLinksPerField { get; set; }
        public bool OmitContextPublications { get; set; }
        public bool OmitOwningPublications { get; set; }
        public bool OmitFolders { get; set; }
        public SerializationFormats SerializationFormat { get; set; }


        public BuildProperties(Package package)
        {
            if (package == null)
                return;
            if (HasPackageValue(package, "LinkLevels"))
            {
                LinkLevels = Convert.ToInt32(package.GetValue("LinkLevels"));
            }
            else
            {
                LinkLevels = DefaultLinkLevels;
            }
            if (HasPackageValue(package, "ResolveWidthAndHeight"))
            {
                ResolveWidthAndHeight = package.GetValue("ResolveWidthAndHeight").ToLower().Equals("yes");
            }
            else
            {
                ResolveWidthAndHeight = DefaultResolveWidthAndHeight;
            }
            if (HasPackageValue(package, "PublishEmptyFields"))
            {
                PublishEmptyFields = package.GetValue("PublishEmptyFields").ToLower().Equals("yes");
            }
            else
            {
                PublishEmptyFields = DefaultPublishEmptyFields;
            }
            if (HasPackageValue(package, "FollowLinksPerField"))
            {
                FollowLinksPerField = package.GetValue("FollowLinksPerField").ToLower().Equals("yes");
            }
            else
            {
                FollowLinksPerField = DefaultFollowLinksPerField;
            }
            if (HasPackageValue(package, "SerializationFormat"))
            {
                SerializationFormat = (SerializationFormats)Enum.Parse(typeof(SerializationFormats), package.GetValue("SerializationFormat").ToUpper());
            }
            else
            {
                SerializationFormat = DefaultSerializationFormat;
            }
            if (HasPackageValue(package, "OmitContextPublications"))
            {
                OmitContextPublications = package.GetValue("OmitContextPublications").ToLower().Equals("yes");
            }
            else
            {
                OmitContextPublications = DefaultOmitContextPublications;
            }
            if (HasPackageValue(package, "OmitOwningPublications"))
            {
                OmitOwningPublications = package.GetValue("OmitOwningPublications").ToLower().Equals("yes");
            }
            else
            {
                OmitOwningPublications = DefaultOmitOwningPublications;
            }
            if (HasPackageValue(package, "OmitFolders"))
            {
                OmitFolders = package.GetValue("OmitFolders").ToLower().Equals("yes");
            }
            else
            {
                OmitFolders = DefaultOmitFolders;
            }
        }

        private bool HasPackageValue(Package package, string key)
        {
            foreach (KeyValuePair<string, Item> kvp in package.GetEntries())
            {
                if (kvp.Key.Equals(key))
                {
                    return true;
                }
            }
            return false;
        }
    }
}