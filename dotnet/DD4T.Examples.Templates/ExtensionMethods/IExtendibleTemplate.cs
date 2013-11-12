using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.Templating;

namespace DD4T.Examples.Templates.ExtensionMethods
{
    public interface IExtendibleTemplate : ITemplate
    {
        Engine Engine { get; set; }
        Package Package { get; set; }
        TemplatingLogger Logger { get; set; }
        //int RenderContext { get; set; }
        void Initialize(Engine engine, Package package);
    }
}
