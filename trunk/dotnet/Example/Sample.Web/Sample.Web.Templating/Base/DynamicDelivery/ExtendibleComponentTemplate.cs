using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating;
using Sample.Web.Templating.ExtensionMethods;
using DD4T.Templates.Base;

namespace Sample.Web.Templating.Base.DynamicDelivery
{
    public abstract class ExtendibleComponentTemplate : BaseComponentTemplate, IExtendibleTemplate
    {

        public new Tridion.ContentManager.Templating.Engine Engine
        {
            get;
            set; 
        }

        public new Tridion.ContentManager.Templating.Package Package
        {
            get;
            set;
        }
        public TemplatingLogger Logger
        {
            get;
            set;
        }

        public void Initialize(Tridion.ContentManager.Templating.Engine engine, Tridion.ContentManager.Templating.Package package)
        {
            this.Engine = engine;
            this.Package = package;
            this.Logger = TemplatingLogger.GetLogger(this.GetType());
        }

        public override void Transform(Engine engine, Package package)
        {
            this.Initialize(engine, package);
            if (this.IsPageTemplate())
            {
                this.Logger.Warning("called from a Component Template instead of a Page Template");
                return;
            }
            base.Transform(engine, package);
        }
    }
}
