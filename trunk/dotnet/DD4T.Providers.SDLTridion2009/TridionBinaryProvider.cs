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
//using DD4T.Utils;
using System.Collections.Generic;

using System.Web.Caching;
using System.Web;
using DD4T.ContentModel.Contracts.Providers;
using Com.Tridion.Util;

namespace DD4T.Providers.SDLTridion2009
{
    /// <summary>
    /// 
    /// </summary>
    public class TridionBinaryProvider : BaseProvider, IBinaryProvider
    {

		private static IDictionary<string, DateTime> lastPublishedDates = new Dictionary<string, DateTime>();


        #region IBinaryProvider Members

        public byte[] GetBinaryByUri(string uri)
        {
            byte[] retVal = null;
            using (Com.Tridion.Broker.Binaries.Meta.SQLBinaryMetaHome bmHome = new Com.Tridion.Broker.Binaries.Meta.SQLBinaryMetaHome())
            {
                TCDURI tcdUri = new TCDURI(uri);
                Com.Tridion.Meta.BinaryMeta bm = bmHome.FindByPrimaryKey(tcdUri);
                if (bm == null) return retVal;
                using (Com.Tridion.Broker.Binaries.SQLBinaryHome binaryHome = new Com.Tridion.Broker.Binaries.SQLBinaryHome())
                {
                    Com.Tridion.Data.BinaryData data = binaryHome.FindByPrimaryKey(PublicationId, Convert.ToInt32(bm.GetId()), bm.GetVariantId());
                    retVal = data.GetBytes();
                }
            }
            return retVal;
        }

        public byte[] GetBinaryByUrl(string url)
        {
            string encodedUrl = HttpUtility.UrlPathEncode(url); // ?? why here? why now?
            byte[] retVal = null;
            //TODO: Add usings instead of fully classified names            
            using (Com.Tridion.Broker.Binaries.Meta.SQLBinaryMetaHome bmHome = new Com.Tridion.Broker.Binaries.Meta.SQLBinaryMetaHome())
            {
                Com.Tridion.Meta.BinaryMeta bm = bmHome.FindByURL(PublicationId, encodedUrl);
                if (bm == null) return retVal;
                using (Com.Tridion.Broker.Binaries.SQLBinaryHome binaryHome = new Com.Tridion.Broker.Binaries.SQLBinaryHome())
                {
                    Com.Tridion.Data.BinaryData data = binaryHome.FindByPrimaryKey(PublicationId, Convert.ToInt32(bm.GetId()), bm.GetVariantId());
                    retVal = data.GetBytes();
                }
            }
            return retVal;
        }

        public DateTime GetLastPublishedDateByUrl(string url)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastPublishedDateByUri(string uri)
        {
            throw new NotImplementedException();
        }
        #endregion


        public System.IO.Stream GetBinaryStreamByUri(string uri)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream GetBinaryStreamByUrl(string url)
        {
            throw new NotImplementedException();
        }


        public string GetUrlForUri(string uri)
        {
            throw new NotImplementedException();
        }
    }
}
