using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.Templating;
using DD4T.Templates.Base.Builder;
using System.Xml;

namespace DD4T.Templates.Base
{
    /// <summary>
    /// Base Template to use for 'normal' templates. Contains some useful helper methods.
    /// </summary>
    public abstract class DefaultTemplate : ITemplate
    {
        public DefaultTemplate()
        {
            Log = TemplatingLogger.GetLogger(typeof(BaseTemplate));
        }

        public DefaultTemplate(TemplatingLogger log)
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

        public Component GetComponent()
        {
            Item component = Package.GetByType(ContentType.Component);
            return (Component)Engine.GetObject(component.GetAsSource().GetValue("ID"));
        }

        public Page GetPage()
        {
            Item pageItem = Package.GetByType(ContentType.Page);
            if (pageItem != null)
            {
                return Engine.GetObject(pageItem.GetAsSource().GetValue("ID")) as Page;
            }

            Page page = Engine.PublishingContext.RenderContext.ContextItem as Page;
            return page;
        }

        public bool IsPageTemplate()
        {            
            return Engine.PublishingContext.ResolvedItem.Item is Page;
        }
    }
    

}
