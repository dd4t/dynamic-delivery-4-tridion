using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating;

namespace DD4T.Examples.Templates.ExtensionMethods
{
    public abstract class ExtendibleTemplate : IExtendibleTemplate
    {

        public Tridion.ContentManager.Templating.Engine Engine
        {
            get;
            set; 
        }

        public Tridion.ContentManager.Templating.Package Package
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

        public abstract void Transform(Engine engine, Package package);
    }
}
