using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating;

namespace DD4T.Templates.Base.Builder
{
    public class BuildProperties
    {
        public int DefaultLinkLevels = 1;
        public bool DefaultResolveWidthAndHeight = false;
        public bool DefaultPublishEmptyFields = false;
        public bool DefaultFollowLinksPerField = false;
        
        public int LinkLevels { get; set; }
        public bool ResolveWidthAndHeight { get; set; }
        public bool PublishEmptyFields { get; set; }
        public bool FollowLinksPerField { get; set; }        

        
        public BuildProperties(Package package)
        {
            
            if (HasPackageValue(package, "LinkLevels"))
            {
                LinkLevels = Convert.ToInt32(package.GetValue("LinkLevels"));
            }
            else
            {
                LinkLevels = this.DefaultLinkLevels;
            }
            if (HasPackageValue(package, "ResolveWidthAndHeight"))
            {
                ResolveWidthAndHeight = package.GetValue("ResolveWidthAndHeight").ToLower().Equals("yes");
            }
            else
            {
                ResolveWidthAndHeight = this.DefaultResolveWidthAndHeight;
            }
            if (HasPackageValue(package, "PublishEmptyFields"))
            {
                PublishEmptyFields = package.GetValue("PublishEmptyFields").ToLower().Equals("yes");
            }
            else
            {
                PublishEmptyFields = this.DefaultPublishEmptyFields;
            }
            if (HasPackageValue(package, "FollowLinksPerField"))
            {
                FollowLinksPerField = package.GetValue("FollowLinksPerField").ToLower().Equals("yes");
            }
            else
            {
                FollowLinksPerField = this.DefaultFollowLinksPerField;
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
