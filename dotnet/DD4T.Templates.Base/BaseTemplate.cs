using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.Templating;
using DD4T.Templates.Builder;
using System.Xml;

namespace DD4T.Templates
{
    public abstract class BaseTemplate : ITemplate
    {
        public BaseTemplate()
        {
            Log = TemplatingLogger.GetLogger(typeof(BaseTemplate));
        }

        public BaseTemplate(TemplatingLogger log)
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
        private BuildManager _buildManager = new BuildManager();

        public BuildManager Manager
        {
            get { return _buildManager; }
            set { _buildManager = value; }
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
    public class XmlTextWriterFormattedNoDeclaration : XmlTextWriter
    {
        public XmlTextWriterFormattedNoDeclaration(System.IO.TextWriter w)
            : base(w)
        {
            Formatting = System.Xml.Formatting.Indented;
        }
        public XmlTextWriterFormattedNoDeclaration(System.IO.MemoryStream ms, Encoding enc)
            : base(ms, enc)
        {
            Formatting = System.Xml.Formatting.Indented;
        }
        public override void WriteStartDocument() { } // suppress
    }

}
