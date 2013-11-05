using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using Tridion.ContentDelivery.DynamicContent;
using Tridion.ContentDelivery.DynamicContent.Filters;
using Tridion.ContentDelivery.DynamicContent.Query;
using Query = Tridion.ContentDelivery.DynamicContent.Query.Query;
using Tridion.ContentDelivery.Meta;
using Tridion.ContentDelivery.Web.Linking;

using Tridion.Extensions.DynamicDelivery.ContentModel;
using Tridion.Extensions.DynamicDelivery.ContentModel.Exceptions;
using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;
//using Tridion.Extensions.DynamicDelivery.Utils;
using System.Collections.Generic;

using System.Web.Caching;
using System.Web;

namespace Tridion.Extensions.DynamicDelivery.Factories
{
    [Export(typeof(IPageFactory))]
    /// <summary>
    /// 
    /// </summary>
    public class TridionPageFactory : TridionFactoryBase, IPageFactory
    {

		private static IDictionary<string, DateTime> lastPublishedDates = new Dictionary<string, DateTime>();

        #region IPageFactory Members
        public bool TryFindPage(string url, out IPage page)
        {
			page = null;
			
			string cacheKey = String.Format("Page_{0}", url);
			Cache cache = HttpContext.Current.Cache;
			DateTime lastPublishedDate = DateTime.MinValue;
			if (lastPublishedDates.ContainsKey(url))
				lastPublishedDate = lastPublishedDates[url];

			var dbLastPublishedDate = GetLastPublishedDateByUrl(url);

			if (cache[cacheKey] != null && lastPublishedDate != DateTime.MinValue && lastPublishedDate.Subtract(dbLastPublishedDate).TotalSeconds >= 0) {
				page = (IPage)cache[cacheKey];
				return true;
			} else {
				string pageContentFromBroker = GetStringContentFromBrokerByUrl(url);

				if (!pageContentFromBroker.Equals(String.Empty)) {
					page = GetIPageObject(pageContentFromBroker);
					cache.Insert(cacheKey, page);
					lastPublishedDates[url] = dbLastPublishedDate;
					return true;
				}
			}

            return false;
        }
        public IPage FindPage(string url)
        {            
            IPage page;
            if (!TryFindPage(url, out page))
            {
                throw new PageNotFoundException();
            }
            return page;
        }

        public bool TryFindPageContent(string url, out string pageContent)
        {
            pageContent = string.Empty;

			string cacheKey = String.Format("PageContent_{0}", url);
			Cache cache = HttpContext.Current.Cache;
			DateTime lastPublishedDate = DateTime.MinValue;
			if (lastPublishedDates.ContainsKey(url))
				lastPublishedDate = lastPublishedDates[url];

			var dbLastPublishedDate = GetLastPublishedDateByUrl(url);

			if (cache[cacheKey] != null && lastPublishedDate != DateTime.MinValue && lastPublishedDate.Subtract(dbLastPublishedDate).TotalSeconds >= 0) 
			{
				pageContent = (string)cache[cacheKey];
				return true;
			} 
			else 
			{
				string tempPageContent = GetStringContentFromBrokerByUrl(url);
				if (tempPageContent != string.Empty) {
					pageContent = tempPageContent;
					cache.Insert(cacheKey, pageContent);
					lastPublishedDates[url] = dbLastPublishedDate;
					return true;
				}
			}

            

            return false;
        }
        public string FindPageContent(string url)
        {
            string pageContent;
            if (!TryFindPageContent(url, out pageContent))
            {
                throw new PageNotFoundException();
            }

            return pageContent;
        }

        public bool TryGetPage(string tcmUri, out IPage page)
        {
            page = null;

			string cacheKey = String.Format("PageByUri_{0}", tcmUri);
			Cache cache = HttpContext.Current.Cache;
			DateTime lastPublishedDate = DateTime.MinValue;
			if (lastPublishedDates.ContainsKey(tcmUri))
				lastPublishedDate = lastPublishedDates[tcmUri];

			var dbLastPublishedDate = GetLastPublishedDateByUri(tcmUri);

			if (cache[cacheKey] != null && lastPublishedDate != DateTime.MinValue && lastPublishedDate.Subtract(dbLastPublishedDate).TotalSeconds >= 0) 
			{
				page = (IPage)cache[cacheKey];
				return true;
			}
			else
			{
				string tempPageContent = GetStringContentFromBrokerByUri(tcmUri);
				if (tempPageContent != string.Empty) {
					page = GetIPageObject(tempPageContent);
					cache.Insert(cacheKey, page);
					lastPublishedDates[tcmUri] = dbLastPublishedDate;

					return true;
				}
			}
            

            return false;
            
        }
        public IPage GetPage(string tcmUri)
        {
            IPage page;
            if(!TryGetPage(tcmUri, out page))
            {
                throw new PageNotFoundException();
            }

            return page;
        }

        public bool TryGetPageContent(string tcmUri, out string pageContent)
        {
            pageContent = string.Empty;

			string cacheKey = String.Format("PageContentByUri_{0}", tcmUri);
			Cache cache = HttpContext.Current.Cache;
			DateTime lastPublishedDate = DateTime.MinValue;
			if (lastPublishedDates.ContainsKey(tcmUri))
				lastPublishedDate = lastPublishedDates[tcmUri];

			var dbLastPublishedDate = GetLastPublishedDateByUri(tcmUri);
			if (cache[cacheKey] != null && lastPublishedDate != DateTime.MinValue && lastPublishedDate.Subtract(dbLastPublishedDate).TotalSeconds >= 0)
			{
				pageContent = (string)cache[cacheKey];
				return true;
			} 
			else 
			{
				string tempPageContent = GetStringContentFromBrokerByUri(tcmUri);
				if (tempPageContent != string.Empty) {
					pageContent = tempPageContent;
					cache.Insert(cacheKey, pageContent);
					lastPublishedDates[tcmUri] = dbLastPublishedDate;
					return true;
				}
			}
            

            return false;
        }

        public string GetPageContent(string tcmUri)
        {
            string pageContent;
            if (!TryGetPageContent(tcmUri, out pageContent))
            {
                throw new PageNotFoundException();
            }

            return pageContent;
        }

        public bool HasPageChanged(string url)
        {
            return true;
        }

        /// <summary>
        /// 
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

        #endregion

        #region private Helper Methods

        /// <summary>
        /// Returns an IPage object 
        /// </summary>
        /// <param name="pageStringContent">String to desirialize to an IPage object</param>
        /// <returns>IPage object</returns>
        protected static IPage GetIPageObject(string pageStringContent)
        {
            IPage page;
            //Create XML Document to hold Xml returned from WCF Client
            XmlDocument pageContent = new XmlDocument();
            pageContent.LoadXml(pageStringContent);

            //Load XML into Reader for deserialization
            using (var reader = new XmlNodeReader(pageContent.DocumentElement))
            {
                var serializer = new XmlSerializer(typeof(Page));

                try
                {
                    page = (IPage)serializer.Deserialize(reader);
                    LoadComponentModelsFromComponentFactory(page);
                }
                catch (Exception)
                {
                    throw new FieldHasNoValueException();
                }
            }
            return page;
        }

        private static void LoadComponentModelsFromComponentFactory(IPage page)
        {
            TridionComponentFactory factory = new TridionComponentFactory();
            foreach (Tridion.Extensions.DynamicDelivery.ContentModel.ComponentPresentation cp in page.ComponentPresentations)
            {
                cp.Component = (Component)factory.GetComponent(cp.Component.Id);

                foreach (Field tempField in cp.Component.Fields.Values.Where(item => item.FieldType == FieldType.Xhtml))
                {
                    resolveLinks(tempField, new TcmUri(page.Id));
                }
            }
        }

        private static void resolveLinks(Field richTextField, TcmUri pageUri)
        {
            // Find any <a> nodes with xlink:href="tcm attribute
            string nodeText = richTextField.Value;
            XmlDocument tempDocument = new XmlDocument();
            tempDocument.LoadXml("<tempRoot>" + nodeText + "</tempRoot>");
            ILinkFactory linkFactory = new TridionLinkFactory();

            XmlNamespaceManager nsManager = new XmlNamespaceManager(tempDocument.NameTable);
            nsManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink");

            XmlNodeList linkNodes = tempDocument.SelectNodes("//*[local-name()='a'][@xlink:href[starts-with(string(.),'tcm:')]]", nsManager);

            foreach (XmlNode linkElement in linkNodes)
            {
                // TODO test with including the Page Uri, seems to always link with Source Page
                //string linkText = linkFactory.ResolveLink(pageUri.ToString(), linkElement.Attributes["xlink:href"].Value, "tcm:0-0-0");
                string linkText = linkFactory.ResolveLink("tcm:0-0-0", linkElement.Attributes["xlink:href"].Value, "tcm:0-0-0");
                if (!string.IsNullOrEmpty(linkText))
                {
                    XmlAttribute linkUrl = tempDocument.CreateAttribute("href");
                    linkUrl.Value = linkText;
                    linkElement.Attributes.Append(linkUrl);

                    // Remove the other xlink attributes from the a element
                    for (int attribCount = linkElement.Attributes.Count - 1; attribCount >= 0; attribCount--)
                    {
                        if (!string.IsNullOrEmpty(linkElement.Attributes[attribCount].NamespaceURI))
                        {
                            linkElement.Attributes.RemoveAt(attribCount);
                        }
                    }
                }
            }

            if (linkNodes.Count > 0)
            {
                richTextField.Values.Clear();
                richTextField.Values.Add(tempDocument.DocumentElement.InnerXml);
            }
        }

        /// <summary>
        /// Gets the raw string (xml) from the broker db by URL
        /// </summary>
        /// <param name="Url">URL of the page</param>
        /// <returns>String with page xml or empty string if no page was found</returns>
        private string GetStringContentFromBrokerByUrl(string Url)
        {
            string retVal = string.Empty;

            Query pageQuery = new Query();
            ItemTypeCriteria isPage = new ItemTypeCriteria(64);  // TODO There must be an enum of these somewhere
            PageURLCriteria pageUrl = new PageURLCriteria(Url);
			PublicationCriteria correctSite = new PublicationCriteria(PublicationId); //Todo: add logic to determine site on url

             

            Criteria allCriteria = CriteriaFactory.And(isPage, pageUrl);
            allCriteria.AddCriteria(correctSite);

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
        private string GetStringContentFromBrokerByUri(string TcmUri)
        {
            string retVal = string.Empty;

            PageContentAssembler loadPage = new PageContentAssembler();
            retVal = loadPage.GetContent(TcmUri);

            return retVal;
        }
        #endregion


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
    }
}
