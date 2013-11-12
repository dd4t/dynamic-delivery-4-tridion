using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml;
using System.IO;
using System.Reflection;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using DD4T.Templates.Base;
using DD4T.Templates.Base.Utils;

namespace DD4T.Templates.XML
{

    [TcmTemplateTitle("Minimize XML")]
    public class MinimizeXML : ITemplate
    {
        protected TemplatingLogger log = TemplatingLogger.GetLogger(typeof(ConvertXmlToJava));
        protected Package package;
        protected Engine engine;


        public void Transform(Engine engine, Package package)
        {
            this.package = package;
            this.engine = engine;

            if (engine.PublishingContext.RenderContext != null && engine.PublishingContext.RenderContext.ContextVariables.Contains(BasePageTemplate.VariableNameCalledFromDynamicDelivery))
            {
                if (engine.PublishingContext.RenderContext.ContextVariables[BasePageTemplate.VariableNameCalledFromDynamicDelivery].Equals(BasePageTemplate.VariableValueCalledFromDynamicDelivery))
                {
                    log.Debug("template is rendered by a DynamicDelivery page template, will not convert from XML to java");
                    return;
                }
            }

            Item outputItem = package.GetByName("Output");
            String inputValue = package.GetValue("Output");

            if (inputValue == null || inputValue.Length == 0)
            {
                log.Warning("Could not find 'Output' in the package, nothing to transform");
                return;
            }

            // Combine the 'to lower' and 'to java' functions, since there is no reason to have one without the other.
            // Note: it is still possible (for backwards compatibility) to have a separate ToLower TBB in your templates.
            // In that case, the first letter of each element will be converted into lower case twice, which doesn't do any harm.
            string outputValue = XmlMinimizer.Convert(inputValue);            

            // replace the Output item in the package
            package.Remove(outputItem);
            outputItem.SetAsString(outputValue);
            package.PushItem("Output", outputItem);
        }

    }
}
