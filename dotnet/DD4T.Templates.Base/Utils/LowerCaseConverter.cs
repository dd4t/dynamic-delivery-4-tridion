using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml.Serialization;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Publishing.Rendering;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.Logging;
using Dynamic = DD4T.ContentModel;
using DD4T.Templates.Base.Utils;
using DD4T.Templates.Base;

namespace DD4T.Templates.Base.Utils
{

    public static class LowerCaseConverter
    {

        public static string Convert(string input)
        {

            StringReader srXml = new StringReader(input);
            XmlReader readerXml = new XmlTextReader(srXml);

            // Load the style sheet.
            XslCompiledTransform xslTransformer = new XslCompiledTransform();

            //load the Xsl from the assembly
            Stream xslStream = IOUtils.LoadResourceAsStream("DD4T.Templates.Base.Resources.ConvertFirstCharToLowerCase.xslt");
            xslTransformer.Load(XmlReader.Create(xslStream));

            // Execute the transform and output the results to a file.
            StringWriter sw = new StringWriter();
            XmlWriter writer = new XmlTextWriter(sw);
            xslTransformer.Transform(readerXml, writer);
            writer.Close();
            sw.Close();

            return sw.ToString();
        }

        private static void ConvertNodeToLowerCase(XmlNode n)
        {
            if (!(n is XmlElement))
            {
                return;
            }
            string firstLetter = n.LocalName.Substring(0, 1);
            if (!firstLetter.Equals(firstLetter.ToLower()))
            {

                XmlElement newElmt = n.OwnerDocument.CreateElement(firstLetter.ToLower() + n.LocalName.Substring(1));
                foreach (XmlNode c in n.ChildNodes)
                {
                    newElmt.AppendChild(c.CloneNode(true));
                }
                XmlNodeList children = newElmt.ChildNodes;
                foreach (XmlNode c in children)
                {
                    ConvertNodeToLowerCase(c);
                }
                n.ParentNode.RemoveChild(n);
            }
            else
            {
                XmlNodeList children = n.ChildNodes;
                foreach (XmlNode c in children)
                {
                    ConvertNodeToLowerCase(c);
                }
            }

        }
    }
}