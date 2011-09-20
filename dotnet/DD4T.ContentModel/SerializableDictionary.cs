using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Runtime.Serialization;
namespace DD4T.ContentModel
{

    /// <summary
    /// The System.Collections.Generic.Dictionary class from .NET 2.0 is unfortunately not serializable.
    /// This class was written by Paul Welter and is used with gratitude. See http://weblogs.asp.net/pwelter34/archive/2006/05/03/444961.aspx for more details.
    /// </summary>
    /// <typeparam name="TKey">dictionary key</typeparam>
    /// <typeparam name="TValue">dictionary value</typeparam>
    [XmlRoot("dictionary")]
    [Serializable]
    public class SerializableDictionary<TKey, TValue, TImplValue>
        : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public SerializableDictionary() : base() { }
        protected SerializableDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #region IXmlSerializable Members
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TImplValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TImplValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
        #endregion
    }
}
