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
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
namespace Tridion.Extensions.DynamicDelivery.ContentModel {

	/// <summary
	/// The System.Collections.Generic.Dictionary class from .NET 2.0 is unfortunately not serializable.
	/// This class was written by Paul Welter and is used with gratitude. See http://weblogs.asp.net/pwelter34/archive/2006/05/03/444961.aspx for more details.
	/// </summary>
	/// <typeparam name="TKey">dictionary key</typeparam>
	/// <typeparam name="TValue">dictionary value</typeparam>
	[XmlRoot("dictionary")]
	public class SerializableDictionary<TKey, TValue>
		: Dictionary<TKey, TValue>, IXmlSerializable {
		#region IXmlSerializable Members
		public XmlSchema GetSchema() {
			return null;
		}

		public void ReadXml(XmlReader reader) {
			XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
				return;

			while (reader.NodeType != XmlNodeType.EndElement) {
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

		public void WriteXml(XmlWriter writer) {
			XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

			foreach (TKey key in this.Keys) {
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
