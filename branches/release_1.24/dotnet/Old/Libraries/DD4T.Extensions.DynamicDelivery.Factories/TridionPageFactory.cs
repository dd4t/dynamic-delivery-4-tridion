using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Tridion.Extensions.DynamicDelivery.ContentModel;
using Tridion.Extensions.DynamicDelivery.ContentModel.Exceptions;
using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;

namespace DD4T.Extensions.DynamicDelivery.Factories
{
    [Export(typeof(IPageFactory))]
    public class TridionPageFactory : TridionFactoryBase, IPageFactory
    {
        #region IPageFactory Members
        public bool TryFindPage(string url, out IPage page)
        {
            page = null;
            string pageContentFromBroker = GetStringContentFromBrokerByUrl(url, PublicationId);

            if (!pageContentFromBroker.Equals(String.Empty))
            {
                page = GetIPageObject(pageContentFromBroker);
                return true;
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
            string tempPageContent = GetStringContentFromBrokerByUrl(url, PublicationId);
            if (tempPageContent != string.Empty)
            {
                pageContent = tempPageContent;
                return true;
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
            string tempPageContent = GetStringContentFromBrokerByUri(tcmUri);
            if (tempPageContent != string.Empty)
            {
                page = GetIPageObject(tempPageContent);
                return true;
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
            string tempPageContent = GetStringContentFromBrokerByUri(tcmUri);
            if (tempPageContent != string.Empty)
            {
                pageContent = tempPageContent;
                return true;
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

        //TODO: Implement
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
            using (var pa = new Com.Tridion.Broker.Pages.Meta.SQLPageMetaHome())
            {
                Java.Util.Collection pageCollection = pa.FindAll(PublicationId);
                var pageList = new List<Com.Tridion.Meta.PageMeta>();
                foreach (object pageMetaObject in pageCollection)
                {
                    pageList.Add((Com.Tridion.Meta.PageMeta)pageMetaObject);
                }

                using (var sqlBinMetaHome = new Com.Tridion.Broker.Binaries.Meta.SQLBinaryMetaHome())
                {
                    Java.Util.Collection binCollection = sqlBinMetaHome.FindAll(PublicationId);
                    var binList = new List<Com.Tridion.Meta.BinaryMeta>();
                    foreach (object binMetaObject in binCollection)
                    {
                        binList.Add((Com.Tridion.Meta.BinaryMeta)binMetaObject);
                    }
                    var pageUrls = pageList.Select(pm => pm.GetURLPath());
                    var binUrls = binList.Select(bm => bm.GetURLPath());
                    return pageUrls
                        .Concat(binUrls)
                        .Where(url =>
                            includeExtensions.Contains(url.Substring(url.LastIndexOf('.') + 1))
                            && pathStarts.Any(pathStart => url.StartsWith(pathStart)))
                        .ToArray();
                }
            }
        }
        #endregion IPageFactory Members

        #region private Helper Methods

        /// <summary>
        /// Returns an IPage object 
        /// </summary>
        /// <param name="pageStringContent">String to desirialize to an IPage object</param>
        /// <returns>IPage object</returns>
        private static IPage GetIPageObject(string pageStringContent)
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
            foreach (ComponentPresentation cp in page.ComponentPresentations)
            {
                cp.Component = (Component)factory.GetComponent(cp.Component.Id);
            }
        }

        /// <summary>
        /// Gets the raw string (xml) from the broker db by URL
        /// </summary>
        /// <param name="Url">URL of the page</param>
        /// <returns>String with page xml or empty string if no page was found</returns>
        private string GetStringContentFromBrokerByUrl(string Url, int publicationId)
        {
            string retVal = string.Empty;

            //TODO: Add usings instead of fully classified names            
            using (Com.Tridion.Broker.Pages.Meta.SQLPageMetaHome pa = new Com.Tridion.Broker.Pages.Meta.SQLPageMetaHome())
            {                
                Com.Tridion.Meta.PageMeta pm = pa.FindByURL(publicationId, Url);
                if (pm == null) return retVal;
                using (Com.Tridion.Broker.Pages.SQLPageHome pageHome = new Com.Tridion.Broker.Pages.SQLPageHome())
                {
                    Com.Tridion.Data.CharacterData data = pageHome.FindByPrimaryKey(publicationId, pm.GetId());
                    retVal = data.GetString();
                }
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

            //Get the publication ID an the itemID from the TcmUri
            using (var tcmUri = new Com.Tridion.Util.TCMURI(TcmUri))
            {
                int publicationID = tcmUri.GetPublicationId();
                int pageID = tcmUri.GetItemId();

                using (var pageHome = new Com.Tridion.Broker.Pages.SQLPageHome())
                {
                    var data = pageHome.FindByPrimaryKey(publicationID, pageID);

                    retVal = data.GetString();
                }
            }

            return retVal;
        }
        #endregion

        public DateTime GetLastPublishedDate(string url)
        {
            using (Com.Tridion.Broker.Pages.Meta.SQLPageMetaHome pa = new Com.Tridion.Broker.Pages.Meta.SQLPageMetaHome())
            {
                Com.Tridion.Meta.PageMeta pm = pa.FindByURL(PublicationId, url);
                if (pm == null)
                    return DateTime.Now;
                else
                    return DateTime.Parse(pm.GetLastPublicationDate().ToString());

            }
        }
    }
}
