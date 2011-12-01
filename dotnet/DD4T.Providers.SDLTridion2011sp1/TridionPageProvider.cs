using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using Tridion.ContentDelivery.DynamicContent;
using Tridion.ContentDelivery.DynamicContent.Filters;
using Tridion.ContentDelivery.DynamicContent.Query;
using Query = Tridion.ContentDelivery.DynamicContent.Query.Query;
using Tridion.ContentDelivery.Meta;
using Tridion.ContentDelivery.Web.Linking;

using DD4T.ContentModel;
using DD4T.ContentModel.Exceptions;
using DD4T.ContentModel.Factories;
using System.Collections.Generic;

using System.Web.Caching;
using System.Web;
using DD4T.ContentModel.Contracts.Providers;

namespace DD4T.Providers.SDLTridion2011sp1
{
    /// <summary>
    /// 
    /// </summary>
    public class TridionPageProvider : BaseProvider, IPageProvider
    {

		private static IDictionary<string, DateTime> lastPublishedDates = new Dictionary<string, DateTime>();


        #region IPageProvider Members

        /// <summary>
        /// Get all urls of published pages
        /// </summary>
        /// <param name="includeExtensions"></param>
        /// <param name="pathStarts"></param>
        /// <param name="publicationID"></param>
        /// <returns></returns>
        public string[] GetAllPublishedPageUrls(string[] includeExtensions, string[] pathStarts)
        {
            Query pageQuery = new Query();
            ItemTypeCriteria isPage = new ItemTypeCriteria(64);  // TODO There must be an enum of these somewhere
            PublicationCriteria currentPublication = new PublicationCriteria(PublicationId); //Todo: add logic to determine site on url

            Criteria pageInPublication = CriteriaFactory.And(isPage, currentPublication);

            if (includeExtensions.Length > 0)
            {
                PageURLCriteria[] extensionsCriteria = new PageURLCriteria[includeExtensions.Length];
                int criteriaCount = 0;
                foreach (string pageExtension in includeExtensions)
                {
                    extensionsCriteria.SetValue(new PageURLCriteria("%" + pageExtension, Criteria.Like), criteriaCount);
                    criteriaCount++;
                }

                Criteria allExtensions = CriteriaFactory.Or(extensionsCriteria);
                pageInPublication = CriteriaFactory.And(pageInPublication, allExtensions);
            }

            if (pathStarts.Length > 0)
            {
                PageURLCriteria[] pathCriteria = new PageURLCriteria[pathStarts.Length];
                int criteriaCount = 0;
                foreach (string requiredPath in pathStarts)
                {
                    pathCriteria.SetValue(new PageURLCriteria(requiredPath + "%", Criteria.Like), criteriaCount);
                    criteriaCount++;
                }

                Criteria allPaths = CriteriaFactory.Or(pathCriteria);
                pageInPublication = CriteriaFactory.And(pageInPublication, allPaths);
            }

            Query findPages = new Query(pageInPublication);
            string[] pageUris = findPages.ExecuteQuery();

            // Need to get PageMeta data to find all the urls
            List<string> pageUrls = new List<string>();
            PageMetaFactory metaFactory = new PageMetaFactory(PublicationId); //Todo: add logic to determine site on url
            foreach (string uri in pageUris)
            {
                IPageMeta currentMeta = metaFactory.GetMeta(uri);
                pageUrls.Add(currentMeta.UrlPath);
            }
            return pageUrls.ToArray();
        }


        /// <summary>
        /// Gets the raw string (xml) from the broker db by URL
        /// </summary>
        /// <param name="Url">URL of the page</param>
        /// <returns>String with page xml or empty string if no page was found</returns>
        public string GetContentByUrl(string Url)
        {
            string retVal = string.Empty;

            Query pageQuery = new Query();
            ItemTypeCriteria isPage = new ItemTypeCriteria(64);  // TODO There must be an enum of these somewhere
            PageURLCriteria pageUrl = new PageURLCriteria(Url);

            Criteria allCriteria = CriteriaFactory.And(isPage, pageUrl);
            if (this.PublicationId != 0)
            {
                PublicationCriteria correctSite = new PublicationCriteria(this.PublicationId);
                allCriteria.AddCriteria(correctSite);
            }
            pageQuery.Criteria = allCriteria;

            string[] resultUris = pageQuery.ExecuteQuery();

            if (resultUris.Length > 0)
            {
                PageContentAssembler loadPage = new PageContentAssembler();
                retVal = loadPage.GetContent(resultUris[0]);
            }
            return retVal;
        }

        /// <summary>
        /// Gets the raw string (xml) from the broker db by URI
        /// </summary>
        /// <param name="Url">TCM URI of the page</param>
        /// <returns>String with page xml or empty string if no page was found</returns>
        public string GetContentByUri(string TcmUri)
        {
            string retVal = string.Empty;

            PageContentAssembler loadPage = new PageContentAssembler();
            retVal = loadPage.GetContent(TcmUri);

            return retVal;
        }


        public DateTime GetLastPublishedDateByUrl(string url)
        {
			PageMetaFactory pMetaFactory = new PageMetaFactory(PublicationId);
			var pageInfo = pMetaFactory.GetMetaByUrl(url);
		    
            if (pageInfo == null || pageInfo.Count <=0)
            {
                return DateTime.Now;
            }
            else
            {				
					IPageMeta pInfo = pageInfo[0] as IPageMeta;
					return pInfo.LastPublicationDate;
            }
        }

		public DateTime GetLastPublishedDateByUri(string uri) {
			PageMetaFactory pMetaFactory = new PageMetaFactory(PublicationId);
			var pageInfo = pMetaFactory.GetMeta(uri);

			if (pageInfo == null) {
				return DateTime.Now;
			} else {
				return pageInfo.LastPublicationDate;
			}
		}
        #endregion
    }
}
