using System.Xml;
using DD4T.Templates.Base;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace DD4T.Templates
{
    /// <summary>
    /// Resource template for generating resource files as XML. 
    /// Example: <root><data name="keyname"><value>keyvalue</value></data></root>
    /// Schema should look like this: 1 embedded schema with 2 textfields: key and value (both NOT multivalue)
    ///                               1 normal schema with 1 field called 'resource' of type 'Embedded schema' and allow multiple values. 
    /// </summary>
    [TcmTemplateTitle("Generate Resources")]
    public class Resources : DefaultTemplate
    {       
        public override void Transform(Engine engine, Package package)
        {
            Engine = engine;
            Package = package;

            var c = IsPageTemplate() ? GetPage().ComponentPresentations[0].Component : GetComponent();

            XmlDocument resourceDoc = null;
            resourceDoc = new XmlDocument();
            resourceDoc.LoadXml("<root/>");

            var fields = new ItemFields(c.Content, c.Schema);
            var sourceField = fields["resource"] as EmbeddedSchemaField;
            foreach (var innerField in sourceField.Values)
            {

                var key = innerField["key"] as TextField;
                var value = innerField["value"] as TextField;

                var data = resourceDoc.CreateElement("data");
                data.SetAttribute("name", key.Value);
                var v = resourceDoc.CreateElement("value");
                v.InnerText = value.Value;
                data.AppendChild(v);
                resourceDoc.DocumentElement.AppendChild(data);
            }
            
            package.PushItem(Package.OutputName, package.CreateStringItem(ContentType.Xml, resourceDoc.OuterXml));
           
        }
    }
}
