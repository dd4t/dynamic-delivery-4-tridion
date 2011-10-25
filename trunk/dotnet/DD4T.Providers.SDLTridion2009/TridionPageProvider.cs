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

using DD4T.ContentModel;
using DD4T.ContentModel.Exceptions;
using DD4T.ContentModel.Factories;
//using DD4T.Utils;
using System.Collections.Generic;

using System.Web.Caching;
using System.Web;
using DD4T.ContentModel.Contracts.Providers;

namespace DD4T.Providers.SDLTridion2009
{
    [Export(typeof(IPageProvider))]
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


        /// <summary>
        /// Gets the raw string (xml) from the broker db by URL
        /// </summary>
        /// <param name="Url">URL of the page</param>
        /// <returns>String with page xml or empty string if no page was found</returns>
        public string GetContentByUrl(string Url)
        {
            string retVal = string.Empty;


            //TODO: Add usings instead of fully classified names            
            using (Com.Tridion.Broker.Pages.Meta.SQLPageMetaHome pa = new Com.Tridion.Broker.Pages.Meta.SQLPageMetaHome())
            {
                Com.Tridion.Meta.PageMeta pm = pa.FindByURL(PublicationId, Url);
                if (pm == null) return retVal;
                using (Com.Tridion.Broker.Pages.SQLPageHome pageHome = new Com.Tridion.Broker.Pages.SQLPageHome())
                {
                    Com.Tridion.Data.CharacterData data = pageHome.FindByPrimaryKey(PublicationId, pm.GetId());
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
        public string GetContentByUri(string uri)
        {
            string retVal = string.Empty;

            //Get the publication ID an the itemID from the TcmUri
            TcmUri tcmUri = new TcmUri(uri);
            int publicationID = tcmUri.PublicationId;
            int pageID = tcmUri.ItemId;

            using (var pageHome = new Com.Tridion.Broker.Pages.SQLPageHome())
            {
                var data = pageHome.FindByPrimaryKey(publicationID, pageID);

                retVal = data.GetString();
            }
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
