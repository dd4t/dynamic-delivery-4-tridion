using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.Templating;
using DD4T.Extensions.DynamicDelivery.Templates.Builder;

namespace DD4T.Extensions.DynamicDelivery.Templates
{
    public abstract class TemplateBase : ITemplate
    {
        public TemplateBase()
        {
            Log = TemplatingLogger.GetLogger(typeof(TemplateBase));
        }

        public TemplateBase(TemplatingLogger log)
        {
            Log = log;
        }

        protected TemplatingLogger Log
        {
            get;
            set;
        }
        protected Package Package { get; set; }
        protected Engine Engine { get; set; }
        private BuildManager buildManager = new BuildManager();

        public BuildManager manager
        {
            get { return buildManager; }
            set { buildManager = value; }
        }

        public abstract void Transform(Engine engine, Package package);

        protected bool HasPackageValue(Package package, string key)
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
