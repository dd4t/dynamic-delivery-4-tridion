///   
/// Copyright 2011 Capgemini & SDL
///
///   Licensed under the Apache License, Version 2.0 (the "License");
///   you may not use this file except in compliance with the License.
///   You may obtain a copy of the License at
///
///       http://www.apache.org/licenses/LICENSE-2.0
///
///   Unless required by applicable law or agreed to in writing, software
///   distributed under the License is distributed on an "AS IS" BASIS,
///   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
///   See the License for the specific language governing permissions and
///   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml;
using System.IO;
using System.Reflection;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.Extensions.DynamicDelivery.Templates.Utils;

namespace Tridion.Extensions.DynamicDelivery.Templates.XML
{
   [TcmTemplateTitle("Convert XML to Java")]
   public class ConvertXmlToJava : ITemplate
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

         //load the Xml doc
         StringReader xmlReader = new StringReader(inputValue);
         XPathDocument myXPathDoc = new XPathDocument(xmlReader);

         XslCompiledTransform xslTransformer = new XslCompiledTransform();

         //load the Xsl from the assembly
         Stream xslStream = IOUtils.LoadResourceAsStream("Tridion.Extensions.DynamicDelivery.Templates.Resources.ConvertToJava.xslt");
         xslTransformer.Load(XmlReader.Create(xslStream));

         //create the output stream 
         MemoryStream outputStream = new MemoryStream();            

         //do the actual transform of Xml
         xslTransformer.Transform(myXPathDoc, null, outputStream);

         // read output into a string
         outputStream.Seek(0, SeekOrigin.Begin);
         StreamReader outputReader = new StreamReader(outputStream);
         string outputValue = outputReader.ReadToEnd();

         // replace the Output item in the package
         package.Remove(outputItem);
         outputItem.SetAsString(outputValue);
         package.PushItem("Output", outputItem);
      }

   }
}
