using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xml.Serialization.GeneratedAssembly;
using System.Text.RegularExpressions;

namespace DD4T.Templates.Base.Serializing
{
    public class XmlSerializerService : ISerializerService
    {
        //public string Serialize(Page page)
        //{
        //    return Serialize(page, GetXmlSerializer<PageSerializer>());
        //}

        //public string Serialize(Component component)
        //{
        //    return Serialize(component, GetXmlSerializer<ComponentSerializer>());
        //}

        //public object Deserialize<T>(string inputValue) where T: IItem
        //{
        //    TextReader tr = new StringReader(inputValue);

        //    XmlSerializer serializer = null;

        //    if (typeof(T) == typeof(Page) || typeof(T) == typeof(IPage))
        //        serializer = GetXmlSerializer<PageSerializer>();
        //    else if (typeof(T) == typeof(Component) || typeof(T) == typeof(IComponent))
        //        serializer = GetXmlSerializer<ComponentSerializer>();

        //    return (T) serializer.Deserialize(tr);
        //}

        private static Dictionary<Type, XmlSerializer> _xmlSerializers = new Dictionary<Type, XmlSerializer>();
        private XmlSerializer GetXmlSerializer<T>() where T: XmlSerializer
        {
            if (! _xmlSerializers.ContainsKey(typeof(T)))
            {
                XmlSerializer serializer = (T)Activator.CreateInstance(typeof(T));
                _xmlSerializers.Add(typeof(T), serializer);
            }
            return _xmlSerializers[typeof(T)];
        }

        private string Serialize(object o, XmlSerializer serializer)
        {
            StringWriter sw = new StringWriter();
            MemoryStream ms = new MemoryStream();
            XmlWriter writer = new XmlTextWriterFormattedNoDeclaration(ms, Encoding.UTF8);
            string outputValue;
            //Create our own namespaces for the output
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            ns.Add("", "");
                       
            serializer.Serialize(writer, o, ns);
            outputValue = Encoding.UTF8.GetString(ms.ToArray());

            // for some reason, the .NET serializer leaves an invalid character at the start of the string
            // we will remove everything up to the first < so that the XML can be deserialized later!
            Regex re = new Regex("^[^<]+");
            outputValue = re.Replace(outputValue, "");
            return outputValue;
        }


        public string Serialize<T>(T input) where T : RepositoryLocalItem
        {
            if (input is Page || input is IPage)
                return Serialize(input, GetXmlSerializer<PageSerializer>());
            if (input is Component || input is IComponent)
                return Serialize(input, GetXmlSerializer<ComponentSerializer>());
            throw new Exception("cannot serialize object of type " + typeof(T));
        }

        public T Deserialize<T>(string input)
        {
            TextReader tr = new StringReader(input);

            XmlSerializer serializer = null;

            if (typeof(T) == typeof(Page) || typeof(T) == typeof(IPage))
                serializer = GetXmlSerializer<PageSerializer>();
            else if (typeof(T) == typeof(Component) || typeof(T) == typeof(IComponent))
                serializer = GetXmlSerializer<ComponentSerializer>();

            return (T)serializer.Deserialize(tr);
        }
    }
}