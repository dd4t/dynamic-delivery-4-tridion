using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using Tridion.ContentDelivery.DynamicContent;
using Tridion.ContentDelivery.DynamicContent.Query;
using Query = Tridion.ContentDelivery.DynamicContent.Query.Query;
using Tridion.ContentDelivery.Meta;
using Tridion.ContentDelivery.Web.Linking;

using DD4T.ContentModel;
using DD4T.ContentModel.Exceptions;
using DD4T.ContentModel.Factories;
//using DD4T.Utils;
using System.Collections.Generic;

using System.Web.Caching;
using System.Web;
using DD4T.ContentModel.Contracts.Providers;
using System.Collections;
using System.Configuration;

namespace DD4T.Providers.SDLTridion2011
{
    [Export(typeof(IComponentProvider))]
    /// <summary>
    /// 
    /// </summary>
    public class TridionComponentProvider : BaseProvider, IComponentProvider
    {
        private string selectByComponentTemplateId;
        private string selectByOutputFormat;
        public TridionComponentProvider()
        {
            selectByComponentTemplateId = ConfigurationManager.AppSettings["ComponentFactory.ComponentTemplateId"];
            selectByOutputFormat = ConfigurationManager.AppSettings["ComponentFactory.OutputFormat"];
        }

        public string GetContent(string uri)
        {
            
            TcmUri tcmUri = new TcmUri(uri);
            Tridion.ContentDelivery.DynamicContent.ComponentPresentationFactory cpFactory = new ComponentPresentationFactory(PublicationId);
            Tridion.ContentDelivery.DynamicContent.ComponentPresentation cp = null;


            if (!string.IsNullOrEmpty(selectByComponentTemplateId))
            {
                cp = cpFactory.GetComponentPresentation(tcmUri.ItemId, Convert.ToInt32(selectByComponentTemplateId));
                if (cp != null)
                    return cp.Content;
            }
            if (!string.IsNullOrEmpty(selectByOutputFormat))
            {
                cp = cpFactory.GetComponentPresentationWithOutputFormat(tcmUri.ItemId, selectByOutputFormat);
                if (cp != null)
                    return cp.Content;
            }
            IList cps = cpFactory.FindAllComponentPresentations(tcmUri.ItemId);

            foreach (Tridion.ContentDelivery.DynamicContent.ComponentPresentation _cp in cps)
            {
                if (_cp != null)
                {
                    if (_cp.Content.Contains("<Component"))
                    {
                        return _cp.Content;
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the Component contents which could be found. Components that couldn't be found don't appear in the list. 
        /// </summary>
        /// <param name="componentUris"></param>
        /// <returns></returns>
        public List<string> GetContentMultiple(string[] componentUris)
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
            ItemLastPublishedDateCriteria dateLastPublished = new ItemLastPublishedDateCriteria(lastPublishedDate.ToString("yyyy-MM-dd HH:mm:ss.fff"), Criteria.GreaterThanOrEqual);
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
                            throw new System.Exception("Unexpected query item data type; " + queryItem.MetaValue.GetType().Name);
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


        public DateTime GetLastPublishedDate(string uri)
        {
            throw new NotImplementedException();
        }
    }
}
