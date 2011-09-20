using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.Extensions.DynamicDelivery.ContentModel.Factories;
using DD4T.Extensions.DynamicDelivery.ContentModel;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using DD4T.Extensions.DynamicDelivery.ContentModel.Exceptions;
using Tridion.ContentDelivery.DynamicContent.Query;
using System.IO;
using Tridion.ContentDelivery.DynamicContent;
using System.Configuration;

namespace DD4T.Extensions.DynamicDelivery.Factories
{
    public abstract class TridionComponentFactoryBase : TridionFactoryBase, IComponentFactory
    {
        public bool TryGetComponent(string componentUri, out IComponent component)
        {
            component = null;
            TcmUri uri = new TcmUri(componentUri);
            Tridion.ContentDelivery.DynamicContent.ComponentPresentationFactory cpFactory = new Tridion.ContentDelivery.DynamicContent.ComponentPresentationFactory(PublicationId);
            Tridion.ContentDelivery.DynamicContent.ComponentPresentation cp = null;
            IList cps = cpFactory.FindAllComponentPresentations(uri.ItemId);

            if (cps.Count > 0 && cps[0] != null)
                cp = (Tridion.ContentDelivery.DynamicContent.ComponentPresentation)cps[0];

            if (cp != null)
            {
                var componentContent = new XmlDocument();
                componentContent.LoadXml(cp.Content);
                var serializer = new XmlSerializer(typeof(Component));
                using (var reader = new XmlNodeReader(componentContent.DocumentElement))
                {
                    component = (IComponent)serializer.Deserialize(reader);
                }
                return true;
            }

            return false;
        }

        public IComponent GetComponent(string componentUri)
        {
            IComponent component;
            if (!TryGetComponent(componentUri, out component))
            {
                throw new ComponentNotFoundException();
            }

            return component;
        }

        /// <summary>
        /// Returns the Component contents which could be found. Components that couldn't be found don't appear in the list. 
        /// </summary>
        /// <param name="componentUris"></param>
        /// <returns></returns>
        public List<string> GetComponents(string[] componentUris)
        {
            TcmUri uri = new TcmUri(componentUris.First());
            ComponentPresentationFactory cpFactory = new ComponentPresentationFactory(uri.PublicationId);
            var components =
                componentUris
                .Select(componentUri => (Tridion.ContentDelivery.DynamicContent.ComponentPresentation)cpFactory.FindAllComponentPresentations(componentUri)[0])
                .Where(cp => cp != null)
                .Select(cp => cp.Content)
                .ToList();

            return components;

        }

        public IList<IComponent> FindComponents(string schemaUri)
        {
            var serializer = new XmlSerializer(typeof(Component));
            return GetComponents(FindComponentUrisBySchemas(new[] { schemaUri }, null, null))
                .Select(component => (IComponent)serializer.Deserialize(new StringReader(component)))
                .ToList();
        }

        public IList<IComponent> FindComponents(string[] schemaUris)
        {
            var serializer = new XmlSerializer(typeof(Component));
            return GetComponents(FindComponentUrisBySchemas(schemaUris, null, null))
                .Select(component => (IComponent)serializer.Deserialize(new StringReader(component)))
                .ToList();
        }


        public IDictionary<string, IComponentMeta> FindComponentMetas(string[] schemaUris)
        {
            return
                (IDictionary<string, IComponentMeta>)GetComponentMetas(FindComponentUrisBySchemas(schemaUris, null, null))
                .ToDictionary(k => String.Format("tcm:{0}-{1}", PublicationId, k.Id), v => CreateComponentMeta(v));
        }

        private IComponentMeta CreateComponentMeta(Tridion.ContentDelivery.Meta.IComponentMeta v)
        {
            return new ComponentMeta()
            {
                ID = String.Format("tcm:{0}-{1}", PublicationId, v.Id),
                LastPublishedDate = v.LastPublicationDate,
                ModificationDate = v.ModificationDate,
                CreationDate = v.CreationDate
            };
        }

        private string[] FindComponentUrisBySchemas(string[] schemaUris, int? limit, DateTime? sinceLastPublished)
        {
            TcmUri schemaTcmUri = new TcmUri(schemaUris.First());

            PublicationCriteria publicationCriteria = new PublicationCriteria(schemaTcmUri.PublicationId);

            Criteria[] schemaCriterias = schemaUris
                .Select(uri => new ItemSchemaCriteria(new TcmUri(uri).ItemId))
                .ToArray();

            Criteria basedOnSchema = CriteriaFactory.Or(schemaCriterias);
            Criteria basedOnSchemaAndInPublication = CriteriaFactory.And(publicationCriteria, basedOnSchema);

            if (sinceLastPublished.HasValue)
            {
                ItemLastPublishedDateCriteria dateLastPublished = new ItemLastPublishedDateCriteria(sinceLastPublished.Value.ToString("yyyy-MM-dd hh:mm:ss"), Criteria.GreaterThanOrEqual);
                basedOnSchemaAndInPublication = CriteriaFactory.And(basedOnSchemaAndInPublication, dateLastPublished);
            }

            Query q = new Query(basedOnSchemaAndInPublication);

            SortParameter sortParameter = new SortParameter(SortParameter.ItemLastPublishedDate, SortParameter.Descending);
            q.AddSorting(sortParameter);

            if (limit.HasValue && limit.Value > 0)
            {
                q.AddLimitFilter(new LimitFilter(limit.Value));
            }
            else
            {
                q.AddLimitFilter(new LimitFilter(maximumComponent));
            }

            return q.ExecuteQuery();
        }

        public List<Tridion.ContentDelivery.Meta.IComponentMeta> GetComponentMetas(string[] componentUris)
        {
            Com.Tridion.Util.TCMURI uri = new Com.Tridion.Util.TCMURI(componentUris.First());
            using (Tridion.ContentDelivery.Meta.ComponentMetaFactory fac = new Tridion.ContentDelivery.Meta.ComponentMetaFactory(uri.GetPublicationId()))
            {
                return componentUris
                    .Select(compUri => fac.GetMeta(compUri))
                    .ToList();
            }
        }

        /// <summary>
        /// Gets the meta information for a given component
        /// </summary>
        /// <param name="componentUri"></param>
        /// <returns>ComponentMeta object holding the meta information</returns>
        public IComponentMeta GetComponentMeta(string componentUri)
        {
            ComponentMeta compMeta = new ComponentMeta();
            using (Com.Tridion.Util.TCMURI uri = new Com.Tridion.Util.TCMURI(componentUri))
            {
                using (Com.Tridion.Meta.ComponentMetaFactory fac = new Com.Tridion.Meta.ComponentMetaFactory(PublicationId))
                {
                    Com.Tridion.Meta.ComponentMeta componentMeta = fac.GetMeta(uri.GetItemId());
                    //Convert Java.Util.Date to System.Date //TODO: check if correct
                    string creationDate = componentMeta.GetCreationDate().ToString();
                    string modificationDate = componentMeta.GetModificationDate().ToString();

                    compMeta.CreationDate = Convert.ToDateTime(creationDate, System.Globalization.CultureInfo.InvariantCulture);
                    compMeta.ModificationDate = Convert.ToDateTime(modificationDate, System.Globalization.CultureInfo.InvariantCulture);

                    return compMeta;
                }
            }

        }

        public IComponent GetLastPublishedComponent(string schemaUri)
        {
            string[] componentContents = GetComponents(FindComponentUrisBySchemas(new[] { schemaUri }, 1, null)).ToArray();
            if (componentContents.Length > 0 && !String.IsNullOrEmpty(componentContents.First()))
            {
                var serializer = new XmlSerializer(typeof(Component));
                return (IComponent)serializer.Deserialize(new StringReader(componentContents.First()));
            }
            return null;
        }

        public DateTime LastPublished(string[] schemaUris)
        {
            TcmUri schemaTcmUri = new TcmUri(schemaUris.First());

            PublicationCriteria publicationCriteria = new PublicationCriteria(schemaTcmUri.PublicationId);

            Criteria[] schemaCriterias = schemaUris
                .Select(uri => new ItemSchemaCriteria(new TcmUri(uri).ItemId))
                .ToArray();

            Criteria basedOnSchema = CriteriaFactory.Or(schemaCriterias);
            Criteria basedOnSchemaAndInPublication = CriteriaFactory.And(publicationCriteria, basedOnSchema);

            Query q = new Query(basedOnSchemaAndInPublication);

            SortParameter sortParameter = new SortParameter(SortParameter.ItemLastPublishedDate, SortParameter.Descending);
            q.AddSorting(sortParameter);
            q.AddLimitFilter(new LimitFilter(1));

            string[] foundUris = q.ExecuteQuery();

            if (foundUris.Length > 0)
            {
                using (Tridion.ContentDelivery.Meta.ComponentMetaFactory fac = new Tridion.ContentDelivery.Meta.ComponentMetaFactory(schemaTcmUri.PublicationId))
                {
                    Tridion.ContentDelivery.Meta.IComponentMeta meta = fac.GetMeta(foundUris[0]);
                    return meta.LastPublicationDate;
                }
            }

            return DateTime.MinValue;
        }

        public IList<IComponent> FindComponents(string[] schemaUris, DateTime sinceLastPublished)
        {
            var serializer = new XmlSerializer(typeof(Component));
            return GetComponents(FindComponentUrisBySchemas(schemaUris, null, null))
                .Select(component => (IComponent)serializer.Deserialize(new StringReader(component)))
                .ToList();
        }

        public IDictionary<string, IComponentMeta> FindComponentMetas(string[] schemaUris, DateTime sinceLastPublished)
        {
            return
                (IDictionary<string, IComponentMeta>)GetComponentMetas(FindComponentUrisBySchemas(schemaUris, null, sinceLastPublished))
                .ToDictionary(k => String.Format("tcm:{0}-{1}", PublicationId, k.Id), v => CreateComponentMeta(v));
        }

        private int maximumComponent
        {
            get
            {
                string maxComponents = ConfigurationManager.AppSettings["ComponentFactory.MaxComponents"];
                return String.IsNullOrEmpty(maxComponents) ? 200 : Convert.ToInt32(maxComponents);
            }
        }

		#region IComponentFactory Members


		public IList<string> FindComponents(ExtendedQueryParameters queryParameters) {
			throw new NotImplementedException();
		}

		#endregion
	}
}
