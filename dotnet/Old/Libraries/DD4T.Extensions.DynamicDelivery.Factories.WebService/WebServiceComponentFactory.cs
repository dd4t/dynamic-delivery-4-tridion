using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Tridion.Extensions.DynamicDelivery.ContentModel;
using Tridion.Extensions.DynamicDelivery.ContentModel.Exceptions;
using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;
using Tridion.Extensions.DynamicDelivery.Factories;
using Tridion.Extensions.DynamicDelivery.Factories.WebService.Tridion.Broker;

namespace Tridion.Extensions.DynamicDelivery.Factories.WebService
{
    [Export(typeof(IComponentFactory))]
    public class WebServiceComponentFactory : TridionFactoryBase, IComponentFactory
    {
        public IComponent GetComponent(string componentUri)
        {
            IComponent component;
            if (!TryGetComponent(componentUri, out component))
            {
                throw new ComponentNotFoundException();
            }

            return component;
        }

        public bool TryGetComponent(string componentUri, out IComponent component)
        {
            component = null;
            var serializer = new XmlSerializer(typeof(Component));

            using (var client = new TridionBrokerServiceClient())
            {
                //Create XML Document to hold Xml returned from WCF Client
                var componentContent = new XmlDocument();

                //TODO add GetComponent method in webservice
                string content = client.GetComponent(componentUri);
                if (!string.IsNullOrEmpty(content))
                {
                    componentContent.LoadXml(content);
                    using (var reader = new XmlNodeReader(componentContent.DocumentElement))
                    {
                        component = (IComponent)serializer.Deserialize(reader);
                        return true;
                    }
                }                
                return false;
            }            
        }

        public IList<IComponent> FindComponents(string schemaUri)
        {
            var serializer = new XmlSerializer(typeof(Component));
            string[] components;

            using (var client = new TridionBrokerServiceClient())
            {
                components = client.FindComponentsBySchema(schemaUri);
            }

            List<IComponent> deserializedComponents = components
                .Select(component => (IComponent)serializer.Deserialize(new StringReader(component)))
                .ToList();
            return deserializedComponents;
        }

        public IList<IComponent> FindComponents(string[] schemaUris)
        {
            Cache cache = HttpContext.Current.Cache;
            string cacheKey = String.Format("Components_{0}", String.Join(":", schemaUris));
            if (cache[cacheKey] != null)
            {
                return (List<IComponent>)cache[cacheKey];
            }
            else
            {
                var serializer = new XmlSerializer(typeof(Component));
                string[] components;

                using (var client = new TridionBrokerServiceClient())
                {
                    components = client.FindComponentsBySchemas(schemaUris);
                }

                List<IComponent> deserializedComponents = components
                    .Select(component => (IComponent)serializer.Deserialize(new StringReader(component)))
                    .ToList();
                cache.Insert(cacheKey, deserializedComponents, null, DateTime.Now.AddMinutes(5), TimeSpan.Zero);
                return deserializedComponents;
            }
        }

        public IDictionary<string, IComponentMeta> FindComponentMetas(string[] schemaUris)
        {
            int pubId = new TcmUri(schemaUris.First()).PublicationId;
            string[] componentMetas;
            using (var client = new TridionBrokerServiceClient())
            {
                componentMetas = client.FindComponentMetasBySchemas(schemaUris);
            }

            return
                componentMetas
                .Select(metaXml => XElement.Parse(metaXml))
                .ToDictionary(k => String.Format("tcm:{0}-{1}", pubId, k.Element("Id").Value), v => CreateComponentMeta(v));
        }

        public IDictionary<string, IComponentMeta> FindComponentMetas(string[] schemaUris, DateTime sinceLastPublished)
        {
            int pubId = new TcmUri(schemaUris.First()).PublicationId;
            string[] componentMetas;
            using (var client = new TridionBrokerServiceClient())
            {
                componentMetas = client.FindLatestComponentMetasBySchemas(schemaUris, sinceLastPublished);
            }

            return
                componentMetas
                .Select(metaXml => XElement.Parse(metaXml))
                .ToDictionary(k => String.Format("tcm:{0}-{1}", pubId, k.Element("Id").Value), v => CreateComponentMeta(v));
        }

        private IComponentMeta CreateComponentMeta(XElement element)
        {
            CultureInfo enUS = new CultureInfo("en-US");

            return new ComponentMeta()
            {
                ID = element.Element("Id").Value,
                LastPublishedDate = DateTime.Parse(element.Element("LastPublicationDate").Value, enUS.DateTimeFormat),
                CreationDate = DateTime.Parse(element.Element("CreationDate").Value, enUS.DateTimeFormat),
                ModificationDate = DateTime.Parse(element.Element("ModificationDate").Value, enUS.DateTimeFormat)
            };
        }

        public IComponentMeta GetComponentMeta(string componentUri)
        {
            ComponentMeta componentMeta = new ComponentMeta();
            using (var client = new TridionBrokerServiceClient())
            {
                string compMeta = client.GetComponentMeta(componentUri);
                XElement compMetaXml = XElement.Parse(compMeta);
                componentMeta.CreationDate = Convert.ToDateTime(compMetaXml.Element("CreationDate").Value, System.Globalization.CultureInfo.InvariantCulture);
                componentMeta.ModificationDate = Convert.ToDateTime(compMetaXml.Element("ModificationDate").Value, System.Globalization.CultureInfo.InvariantCulture);

                return componentMeta;
            }
        }

        public IComponent GetLastPublishedComponent(string schemaUri)
        {

            var serializer = new XmlSerializer(typeof(Component));
            string component;

            using (var client = new TridionBrokerServiceClient())
            {
                component = client.GetLastPublishedComponent(schemaUri);
            }

            IComponent deserializedComponent = (IComponent)serializer.Deserialize(new StringReader(component));
            return deserializedComponent;
        }

        public DateTime LastPublished(string[] schemaUris)
        {
            using (var client = new TridionBrokerServiceClient())
            {
                return client.LastPublished(schemaUris);
            }
        }


        public IList<IComponent> FindComponents(string[] schemaUris, DateTime sinceLastPublished)
        {
            var serializer = new XmlSerializer(typeof(Component));
            using (var client = new TridionBrokerServiceClient())
            {
                return client.FindComponentsSinceLastPublished(schemaUris, sinceLastPublished)
                    .Select(component => (IComponent)serializer.Deserialize(new StringReader(component)))
                    .ToList();
            }
        }


        public IList<IComponent> GetLastCreatedComponents(string[] schemaUris, DateTime lastCreatedDate)
        {
            string[] components = null;
            using (var client = new TridionBrokerServiceClient())
            {
                components = client.FindComponentsSinceLastCreated(schemaUris, lastCreatedDate);
            }
            var serializer = new XmlSerializer(typeof(Component));
            return components
                    .Select(component => (IComponent)serializer.Deserialize(new StringReader(component)))
                    .ToList();
        }


        public IList<IComponent> GetLastPublishedComponents(string[] schemaUris, DateTime lastPublished, Func<string, bool> filter)
        {
            string[] components = null;
            using (var client = new TridionBrokerServiceClient())
            {
                components = client.FindComponentsSinceLastPublished(schemaUris, lastPublished.AddDays(-8));
            }

            var serializer = new XmlSerializer(typeof(Component));
            return components
                .Where(comp => filter(comp))
                .Select(component => (IComponent)serializer.Deserialize(new StringReader(component)))
                .ToList();
        }

        public IList<IComponent> GetLastPublishedComponents(string[] schemaUris, DateTime lastPublishedDate)
        {
            string[] components = null;
            using (var client = new TridionBrokerServiceClient())
            {
                components = client.FindComponentsSinceLastPublished(schemaUris, lastPublishedDate);
            }

            var serializer = new XmlSerializer(typeof(Component));
            return components
                .Select(component => (IComponent)serializer.Deserialize(new StringReader(component)))
                .ToList();
        }


        public IList<string> FindComponents(ExtendedQueryParameters queryParameters)
        {
            throw new NotImplementedException();
        }
    }
}
