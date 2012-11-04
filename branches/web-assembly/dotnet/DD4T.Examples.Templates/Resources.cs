using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.ContentManagement;
using DD4T.Examples.Templates.ExtensionMethods;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.ContentManagement.Fields;

namespace DD4T.Examples.Templating
{
    [TcmTemplateTitle("Resources")]
    public class Resources : ExtendibleTemplate
    {
        public override void Transform(Tridion.ContentManager.Templating.Engine engine, Tridion.ContentManager.Templating.Package package)
        {
            this.Initialize(engine, package);

            Component c;
            if (this.IsPageTemplate())
            {
                c = this.GetPage().ComponentPresentations[0].Component;
            }
            else
            {
                c = this.GetComponent();
            }

            XmlDocument resourceDoc = null;
            resourceDoc = new XmlDocument();
            resourceDoc.LoadXml("<root/>");

            ItemFields fields = new ItemFields(c.Content, c.Schema);
            foreach (ItemField field in fields)
            {
                string name = field.Name;
                string value = ((TextField)field).Value;

                XmlElement data = resourceDoc.CreateElement("data");
                data.SetAttribute("name", name);
                XmlElement v = resourceDoc.CreateElement("value");
                v.InnerText = value;
                data.AppendChild(v);
                resourceDoc.DocumentElement.AppendChild(data);
            }
            /*

            Page p = this.GetPage();
            ItemFields fields = new ItemFields(p.Metadata, p.MetadataSchema);
            EmbeddedSchemaField resourcesField = fields["Resource"] as EmbeddedSchemaField;

            foreach (ItemFields innerFields in resourcesField.Values)
            {
                TextField nameField = innerFields["Name"] as TextField;
                TextField valueField = innerFields["Resource"] as TextField;

                XmlElement data = resourceDoc.CreateElement("data");
                data.SetAttribute("name", nameField.Value);
                XmlElement v = resourceDoc.CreateElement("value");
                v.InnerText = valueField.Value;
                data.AppendChild(v);
                resourceDoc.DocumentElement.AppendChild(data);
            }
             * 
             * */

            this.CreateStringItem("Output", resourceDoc.OuterXml, ContentType.Xml);
        }
    }
}
