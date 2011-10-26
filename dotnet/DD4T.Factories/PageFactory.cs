using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;


using DD4T.ContentModel;
using DD4T.ContentModel.Exceptions;
using DD4T.ContentModel.Factories;
using System.Collections.Generic;

using System.Web.Caching;
using System.Web;
using DD4T.ContentModel.Contracts.Providers;
using DD4T.Providers.SDLTridion2011;

namespace DD4T.Factories
{
    /// <summary>
    /// 
    /// </summary>
    public class PageFactory : FactoryBase, IPageFactory
    {

		private static IDictionary<string, DateTime> lastPublishedDates = new Dictionary<string, DateTime>();
        private IPageProvider pageProvider = null;
        public IPageProvider PageProvider
        {
            get
            {
                // TODO: implement DI
                if (pageProvider == null)
                {
                    pageProvider = new TridionPageProvider();
                    pageProvider.PublicationId = this.PublicationId;
                }
                return pageProvider;
            }
            set
            {
                pageProvider = value;
            }
        }

        private IComponentFactory _componentFactory = null;
        public IComponentFactory ComponentFactory
        {
            get
            {
                if (_componentFactory == null)
                {
                    _componentFactory = new ComponentFactory();
                  
                }
                return _componentFactory;
            }
        }

        private ILinkFactory _linkFactory = null;
        public ILinkFactory LinkFactory
        {
            get
            {
                if (_linkFactory == null)
                {
                    _linkFactory = new LinkFactory();

                }
                return _linkFactory;
            }
        }


        #region IPageFactory Members
        public virtual bool TryFindPage(string url, out IPage page)
        {
			page = null;


			string cacheKey = String.Format("Page_{0}_{1}", url, PublicationId);
			Cache cache = HttpContext.Current.Cache;
			DateTime lastPublishedDate = DateTime.MinValue;
			if (lastPublishedDates.ContainsKey(cacheKey))
                lastPublishedDate = lastPublishedDates[cacheKey];

			var dbLastPublishedDate = PageProvider.GetLastPublishedDateByUrl(url);

			if (cache[cacheKey] != null && lastPublishedDate != DateTime.MinValue && lastPublishedDate.Subtract(dbLastPublishedDate).TotalSeconds >= 0) {
				page = (IPage)cache[cacheKey];
				return true;
			} else {
                string pageContentFromBroker = PageProvider.GetContentByUrl(url);

				if (!pageContentFromBroker.Equals(String.Empty)) {
					page = GetIPageObject(pageContentFromBroker);
					cache.Insert(cacheKey, page);
					lastPublishedDates[cacheKey] = dbLastPublishedDate;
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

        public virtual bool TryFindPageContent(string url, out string pageContent)
        {
            pageContent = string.Empty;

            string cacheKey = String.Format("PageContent_{0}_{1}", url, PublicationId); 
            Cache cache = HttpContext.Current.Cache;
			DateTime lastPublishedDate = DateTime.MinValue;
            if (lastPublishedDates.ContainsKey(cacheKey))
                lastPublishedDate = lastPublishedDates[cacheKey];

			var dbLastPublishedDate = PageProvider.GetLastPublishedDateByUrl(url);

			if (cache[cacheKey] != null && lastPublishedDate != DateTime.MinValue && lastPublishedDate.Subtract(dbLastPublishedDate).TotalSeconds >= 0) 
			{
				pageContent = (string)cache[cacheKey];
				return true;
			} 
			else 
			{
				string tempPageContent = PageProvider.GetContentByUrl(url);
				if (tempPageContent != string.Empty) {
					pageContent = tempPageContent;
					cache.Insert(cacheKey, pageContent);
					lastPublishedDates[cacheKey] = dbLastPublishedDate;
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

			var dbLastPublishedDate = PageProvider.GetLastPublishedDateByUri(tcmUri);

			if (cache[cacheKey] != null && lastPublishedDate != DateTime.MinValue && lastPublishedDate.Subtract(dbLastPublishedDate).TotalSeconds >= 0) 
			{
				page = (IPage)cache[cacheKey];
				return true;
			}
			else
			{
				string tempPageContent = PageProvider.GetContentByUri(tcmUri);
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

			var dbLastPublishedDate = PageProvider.GetLastPublishedDateByUri(tcmUri);
			if (cache[cacheKey] != null && lastPublishedDate != DateTime.MinValue && lastPublishedDate.Subtract(dbLastPublishedDate).TotalSeconds >= 0)
			{
				pageContent = (string)cache[cacheKey];
				return true;
			} 
			else 
			{
				string tempPageContent = PageProvider.GetContentByUri(tcmUri);
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
            return true; // TODO: implement
        }


        /// <summary>
        /// Returns an IPage object 
        /// </summary>
        /// <param name="pageStringContent">String to desirialize to an IPage object</param>
        /// <returns>IPage object</returns>
        public IPage GetIPageObject(string pageStringContent)
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
                    // set order on page for each ComponentPresentation
                    int orderOnPage = 0;
                    foreach (IComponentPresentation cp in page.ComponentPresentations)
                    {
                        cp.OrderOnPage = orderOnPage++;
                    }
                    LoadComponentModelsFromComponentFactory(page);
                }
                catch (Exception)
                {
                    throw new FieldHasNoValueException();
                }
            }
            return page;
        }

        public DateTime GetLastPublishedDateByUrl(string url)
        {
            return PageProvider.GetLastPublishedDateByUrl(url);
        }

        public DateTime GetLastPublishedDateByUri(string uri)
        {
            return PageProvider.GetLastPublishedDateByUri(uri);
        }

        public string[] GetAllPublishedPageUrls(string[] includeExtensions, string[] pathStarts)
        {
            return PageProvider.GetAllPublishedPageUrls(includeExtensions, pathStarts);
        }


#endregion

        #region private helper methods
        private void LoadComponentModelsFromComponentFactory(IPage page)
        {
            foreach (DD4T.ContentModel.ComponentPresentation cp in page.ComponentPresentations)
            {
                // added by QS: only load DCPs from broker if they are in fact dynamic!
                if (cp.IsDynamic)
                {
                    cp.Component = (Component)ComponentFactory.GetComponent(cp.Component.Id);
                }

                foreach (Field tempField in cp.Component.Fields.Values.Where(item => item.FieldType == FieldType.Xhtml))
                {
                    resolveLinks(tempField, new TcmUri(page.Id));
                }
            }
        }

        private void resolveLinks(Field richTextField, TcmUri pageUri)
        {
            // Find any <a> nodes with xlink:href="tcm attribute
            string nodeText = richTextField.Value;
            XmlDocument tempDocument = new XmlDocument();
            tempDocument.LoadXml("<tempRoot>" + nodeText + "</tempRoot>");
           

            XmlNamespaceManager nsManager = new XmlNamespaceManager(tempDocument.NameTable);
            nsManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink");

            XmlNodeList linkNodes = tempDocument.SelectNodes("//*[local-name()='a'][@xlink:href[starts-with(string(.),'tcm:')]]", nsManager);

            foreach (XmlNode linkElement in linkNodes)
            {
                // TODO test with including the Page Uri, seems to always link with Source Page
                //string linkText = linkFactory.ResolveLink(pageUri.ToString(), linkElement.Attributes["xlink:href"].Value, "tcm:0-0-0");
                string linkText = LinkFactory.ResolveLink("tcm:0-0-0", linkElement.Attributes["xlink:href"].Value, "tcm:0-0-0");
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

        #endregion


    }
}
