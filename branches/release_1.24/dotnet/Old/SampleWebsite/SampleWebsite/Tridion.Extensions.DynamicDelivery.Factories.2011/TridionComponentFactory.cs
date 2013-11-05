using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Java.Lang;
using Tridion.ContentDelivery.DynamicContent.Query;
using Tridion.Extensions.DynamicDelivery.ContentModel;
using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;
using Java.Text;
using Java.Util;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using Tridion.Extensions.DynamicDelivery.ContentModel.Exceptions;

namespace Tridion.Extensions.DynamicDelivery.Factories
{
    [Export(typeof(IComponentFactory))]
	class TridionComponentFactory : TridionComponentFactoryBase
    {
        #region IMetaQueryFactory Members
        public IList<string> FindComponents(ExtendedQueryParameters queryParameters)
        {
            string[] basedOnSchemas = queryParameters.QuerySchemas;
            DateTime lastPublishedDate = queryParameters.LastPublishedDate;
            IList<MetaQueryItem> metaQueryItems = queryParameters.MetaQueryValues;
            ExtendedQueryParameters.QueryLogic metaQueryLogic = queryParameters.MetaQueryLogic;
            int maxmimumComponents = queryParameters.MaximumComponents;

            Query q = null;
            //PublicationCriteria publicationAndLastPublishedDateCriteria = new PublicationCriteria(PublicationId);
            PublicationCriteria publicationAndLastPublishedDateCriteria = new PublicationCriteria(PublicationId);
            //format DateTime // 00:00:00.000
            ItemLastPublishedDateCriteria dateLastPublished = new ItemLastPublishedDateCriteria(lastPublishedDate.ToString(), Criteria.GreaterThanOrEqual);
            //publicationAndLastPublishedDateCriteria.AddCriteria(dateLastPublished);

            Criteria basedOnSchemaAndInPublication;

            if (basedOnSchemas.Length > 0)
            {
                Criteria[] schemaCriterias = new Criteria[basedOnSchemas.Length];
                int i = 0;
                foreach (var schema in basedOnSchemas)
                {
                    TcmUri schemaUri = new TcmUri(schema);
                    schemaCriterias.SetValue(new ItemSchemaCriteria(schemaUri.ItemId), i);
                    i++;
                }
                Criteria basedOnSchema = CriteriaFactory.Or(schemaCriterias);
                basedOnSchemaAndInPublication = CriteriaFactory.And(publicationAndLastPublishedDateCriteria, basedOnSchema);
            }
            else
            {
                basedOnSchemaAndInPublication = publicationAndLastPublishedDateCriteria;
            }

            // Add filtering for meta data
            Criteria schemasAndMetaData;
            if (metaQueryItems.Count > 0)
            {
                Criteria metaQuery;
                Criteria[] metaCriterias = new Criteria[metaQueryItems.Count];
                int metaCount = 0;
                foreach (MetaQueryItem queryItem in metaQueryItems)
                {
                    CustomMetaKeyCriteria metaField = new CustomMetaKeyCriteria(queryItem.MetaField);
                    CustomMetaValueCriteria metaCriteria;
                    FieldOperator metaOperator = typeof(Criteria).GetField(queryItem.MetaOperator.ToString()).GetValue(null) as FieldOperator;

                    switch (queryItem.MetaValue.GetType().Name)
                    {
                        case "DateTime":
                            DateTime tempDate = (DateTime)queryItem.MetaValue;
                            metaCriteria = new CustomMetaValueCriteria(metaField, tempDate.ToString("yyyy-MM-dd HH:mm:ss.fff"), "yyyy-MM-dd HH:mm:ss.SSS", metaOperator);
                            break;
                        case "Float":
                            metaCriteria = new CustomMetaValueCriteria(metaField, (float)queryItem.MetaValue, metaOperator);
                            break;
                        case "String":
                            metaCriteria = new CustomMetaValueCriteria(metaField, queryItem.MetaValue as string, metaOperator);
                            break;
                        default:
                            throw new System.Exception("TridionMetaQueryFactory-Unexpected query item data type; " + queryItem.MetaValue.GetType().Name);
                            break;
                    }

                    metaCriterias.SetValue(metaCriteria, metaCount);
                    metaCount++;
                }

                if (queryParameters.MetaQueryLogic == ExtendedQueryParameters.QueryLogic.AllCriteriaMatch)
                {
                    metaQuery = CriteriaFactory.And(metaCriterias);
                }
                else
                {
                    metaQuery = CriteriaFactory.Or(metaCriterias);
                }
                schemasAndMetaData = CriteriaFactory.And(basedOnSchemaAndInPublication, metaQuery);
            }
            else
            {
                schemasAndMetaData = basedOnSchemaAndInPublication;
            }

            Criteria allConditions;
            if (queryParameters.KeywordValues.Count > 0)
            {
                Criteria[] keywordCriterias = new Criteria[queryParameters.KeywordValues.Count];
                int keywordCount = 0;
                foreach (KeywordItem keyCriteria in queryParameters.KeywordValues)
                {
                    TaxonomyKeywordCriteria keywordField = new TaxonomyKeywordCriteria(keyCriteria.CategoryUri, keyCriteria.KeywordUri, false);
                    keywordCriterias.SetValue(keywordField, keywordCount);
                    keywordCount++;
                }

                Criteria keyQuery;
                if (queryParameters.KeywordQueryLogic == ExtendedQueryParameters.QueryLogic.AllCriteriaMatch)
                {
                    keyQuery = CriteriaFactory.And(keywordCriterias);
                }
                else
                {
                    keyQuery = CriteriaFactory.Or(keywordCriterias);
                }
                allConditions = CriteriaFactory.And(schemasAndMetaData, keyQuery);
            }
            else
            {
                allConditions = schemasAndMetaData;
            }


            q = new Query(allConditions);
            if (maxmimumComponents != 0 && maxmimumComponents != int.MaxValue)
            {
                LimitFilter limitResults = new LimitFilter(maxmimumComponents);
                q.SetResultFilter(limitResults);
            }

            // Sort column should either be a standard or custom metaData field
            SortColumn paramSort;
            if (typeof(SortParameter).GetField(queryParameters.QuerySortField) != null)
            {
                paramSort = typeof(SortParameter).GetField(queryParameters.QuerySortField).GetValue(null) as SortColumn;
            }
            else
            {
                // Why do we need to tell Tridion what data type the field is! Its in the database already!
                paramSort = new CustomMetaKeyColumn(queryParameters.QuerySortField, typeof(MetadataType).GetField(queryParameters.SortType.ToString()).GetValue(null) as MetadataType);
            }
            SortDirection paramSortDirection = typeof(SortParameter).GetField(queryParameters.QuerySortOrder.ToString()).GetValue(null) as SortDirection;
            SortParameter sortParameter = new SortParameter(paramSort, paramSortDirection);
            q.AddSorting(sortParameter);
            string[] results = q.ExecuteQuery();

            return results;
        }
		public IComponent GetComponent(string componentUri) {
			IComponent component;
			if (!TryGetComponent(componentUri, out component)) {
				throw new ComponentNotFoundException();
			}

			return component;
		}
		public bool TryGetComponent(string componentUri, out IComponent component) {
			component = null;
			TcmUri uri = new TcmUri(componentUri);
			Tridion.ContentDelivery.DynamicContent.ComponentPresentationFactory cpFactory = new ContentDelivery.DynamicContent.ComponentPresentationFactory(PublicationId);
			Tridion.ContentDelivery.DynamicContent.ComponentPresentation cp = null;
			IList cps = cpFactory.FindAllComponentPresentations(uri.ItemId);

			if (cps.Count > 0 && cps[0] != null)
				cp = (Tridion.ContentDelivery.DynamicContent.ComponentPresentation)cps[0];

			if (cp != null) {
				var componentContent = new XmlDocument();
				componentContent.LoadXml(cp.Content);
				var serializer = new XmlSerializer(typeof(Component));
				using (var reader = new XmlNodeReader(componentContent.DocumentElement)) {
					component = (IComponent)serializer.Deserialize(reader);
				}
				return true;
			}

			return false;
		}
        #endregion
    }
}
