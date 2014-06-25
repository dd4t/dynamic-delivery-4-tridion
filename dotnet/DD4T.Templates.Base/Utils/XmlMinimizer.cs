using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;

namespace DD4T.Templates.Base.Utils
{
    public class XmlMinimizer
    {
        public static string Convert(string input, string minimizeSettings)
        {

            //load the Xml doc
            StringReader xmlReader = new StringReader(input);
            XPathDocument myXPathDoc = new XPathDocument(xmlReader);

            XslCompiledTransform xslTransformer = new XslCompiledTransform();

            //load the Xsl from the assembly
            Stream xslStream = IOUtils.LoadResourceAsStream("DD4T.Templates.Base.Resources.Minimize.xslt");
            xslTransformer.Load(XmlReader.Create(xslStream));

            //create the output stream 
            MemoryStream outputStream = new MemoryStream();

            //set up parameters and do the actual transform of Xml

            if (string.IsNullOrEmpty(minimizeSettings))
            {
                xslTransformer.Transform(myXPathDoc, null, outputStream);
            }
            else
            {
                XsltArgumentList argsList = new XsltArgumentList();
                if (minimizeSettings.ToLower().Contains("publication"))
                    argsList.AddParam("remove-publications", "", true);
                if (minimizeSettings.ToLower().Contains("folder"))
                    argsList.AddParam("remove-folders", "", true);
                xslTransformer.Transform(myXPathDoc, argsList, outputStream);
            }

            // read output into a string
            outputStream.Seek(0, SeekOrigin.Begin);
            StreamReader outputReader = new StreamReader(outputStream);
            return outputReader.ReadToEnd();

        }
    }
}
