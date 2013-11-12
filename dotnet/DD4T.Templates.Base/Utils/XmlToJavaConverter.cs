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

namespace DD4T.Templates.Base.Utils
{
   public static class XmlToJavaConverter
   {

      public static string Convert(string input)
      {

          //load the Xml doc
          StringReader xmlReader = new StringReader(input);
          XPathDocument myXPathDoc = new XPathDocument(xmlReader);

          XslCompiledTransform xslTransformer = new XslCompiledTransform();

          //load the Xsl from the assembly
          Stream xslStream = IOUtils.LoadResourceAsStream("DD4T.Templates.Base.Resources.ConvertToJava.xslt");
          xslTransformer.Load(XmlReader.Create(xslStream));

          //create the output stream 
          MemoryStream outputStream = new MemoryStream();

          //do the actual transform of Xml
          xslTransformer.Transform(myXPathDoc, null, outputStream);

          // read output into a string
          outputStream.Seek(0, SeekOrigin.Begin);
          StreamReader outputReader = new StreamReader(outputStream);
          return outputReader.ReadToEnd();

      }
   }
}
