using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Publishing.Rendering;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.Logging;
using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Text;
using System.Text.RegularExpressions;
using Dynamic = DD4T.ContentModel;
using System.Collections.Generic;
using System.Xml.Serialization;
using DD4T.Templates.Utils;

namespace DD4T.Templates {

	[TcmTemplateTitle("Convert to lower case")]
	public class ConvertToLowerCase : ITemplate {

		private TemplatingLogger log = TemplatingLogger.GetLogger(typeof(ConvertToLowerCase));
		private Package package;
		private Engine engine;
		private List<XmlNode> toBeRemoved = new List<XmlNode>();

		public void Transform(Engine engine, Package package) {
			this.package = package;
			this.engine = engine;

         if (engine.PublishingContext.RenderContext != null && engine.PublishingContext.RenderContext.ContextVariables.Contains(BasePageTemplate.VariableNameCalledFromDynamicDelivery))
         {
            if (engine.PublishingContext.RenderContext.ContextVariables[BasePageTemplate.VariableNameCalledFromDynamicDelivery].Equals(BasePageTemplate.VariableValueCalledFromDynamicDelivery))
            {
               log.Debug("template is rendered by a DynamicDelivery page template, will not convert to lower case");
               return;
            }
         }

			Item outputItem = package.GetByName("Output");
			String inputValue = package.GetValue("Output");
			if (inputValue == null || inputValue.Length == 0) {
				log.Error("Could not find 'Output' in the package. Exiting template.");
				return;
			}

			StringReader srXml = new StringReader(inputValue);
			XmlReader readerXml = new XmlTextReader(srXml);
			
			// Load the style sheet.
         XslCompiledTransform xslTransformer = new XslCompiledTransform();

         //load the Xsl from the assembly
         Stream xslStream = IOUtils.LoadResourceAsStream("DD4T.Templates.Resources.ConvertFirstCharToLowerCase.xslt");
         xslTransformer.Load(XmlReader.Create(xslStream));

			// Execute the transform and output the results to a file.
			StringWriter sw = new StringWriter();
			XmlWriter writer = new XmlTextWriter(sw);
         xslTransformer.Transform(readerXml, writer);
			writer.Close();
			sw.Close();

			package.Remove(outputItem);
			outputItem.SetAsString(sw.ToString());
			package.PushItem("Output", outputItem);
		}



		private void ConvertNodeToLowerCase(XmlNode n) {
			if (!(n is XmlElement)) {
				return;
			}
			string firstLetter = n.LocalName.Substring(0,1);
			log.Debug("elmt " + n.LocalName);
			if (!firstLetter.Equals(firstLetter.ToLower())) {

				XmlElement newElmt = n.OwnerDocument.CreateElement(firstLetter.ToLower() + n.LocalName.Substring(1));
				foreach (XmlNode c in n.ChildNodes) {
					newElmt.AppendChild(c.CloneNode(true));
				}
				XmlNodeList children = newElmt.ChildNodes;
				foreach (XmlNode c in children) {
					ConvertNodeToLowerCase(c);
				}
//				toBeRemoved.Add(n);
				n.ParentNode.RemoveChild(n);
				log.Debug("element is now " + newElmt.OuterXml);
			} else {
				XmlNodeList children = n.ChildNodes;
				foreach (XmlNode c in children) {
					ConvertNodeToLowerCase(c);
				}
			}

		}

	}

}