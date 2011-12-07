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
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace DD4T.Providers.SDLTridion2011sp1
{
    /// <summary>
    /// Provide access to binaries in a Tridion broker instance
    /// </summary>
    public class TridionBinaryProvider : BaseProvider, IBinaryProvider
    {

		private static IDictionary<string, DateTime> lastPublishedDates = new Dictionary<string, DateTime>();

        // NOTE: the BinaryFactory referenced here is part of the Tridion.ContentDelivery namespace
        // Not to be confused with the BinaryFactory from DD4T. The usage chain is:
        // DD4T.Factories.BinaryFactory >>> DD4T.Providers.*.TridionBinaryProvider >>> Tridion.ContentDelivery.DynamicContent.BinaryFactory
        private BinaryFactory _tridionBinaryFactory = null;
        private BinaryFactory TridionBinaryFactory
        {
            get
            {
                if (_tridionBinaryFactory == null)
                    _tridionBinaryFactory = new BinaryFactory();
                return _tridionBinaryFactory;
            }
        }

        private Dictionary<int,ComponentMetaFactory > _tridionComponentMetaFactories = new Dictionary<int,ComponentMetaFactory>();
        private ComponentMetaFactory GetTridionComponentMetaFactory(int publicationId)
        {
                if (! _tridionComponentMetaFactories.ContainsKey(publicationId))
                    _tridionComponentMetaFactories.Add(publicationId,new ComponentMetaFactory(publicationId));
                return _tridionComponentMetaFactories[publicationId];
        }

        #region IBinaryProvider Members

        public byte[] GetBinaryByUri(string uri)
        {
            Tridion.ContentDelivery.DynamicContent.BinaryFactory factory = new BinaryFactory();
            BinaryData binaryData = factory.GetBinary(uri.ToString());
            return binaryData == null ? null : binaryData.Bytes;
        }

        public byte[] GetBinaryByUrl(string url)
        {
            string encodedUrl = HttpUtility.UrlPathEncode(url); // ?? why here? why now?

            BinaryMetaFactory bmFactory = new BinaryMetaFactory();
            BinaryMeta binaryMeta = this.PublicationId == 0 ? (bmFactory.GetMetaByUrl(encodedUrl)[0] as BinaryMeta) : bmFactory.GetMetaByUrl(this.PublicationId, encodedUrl);
            TcmUri uri = new TcmUri(binaryMeta.PublicationId,binaryMeta.Id,16,0);

            Tridion.ContentDelivery.DynamicContent.BinaryFactory factory = new BinaryFactory();

            BinaryData binaryData = string.IsNullOrEmpty(binaryMeta.VariantId) ? factory.GetBinary(uri.ToString()) : factory.GetBinary(uri.ToString(),binaryMeta.VariantId);
            return binaryData == null ? null : binaryData.Bytes;
        }

        public DateTime GetLastPublishedDateByUrl(string url)
        {
            string encodedUrl = HttpUtility.UrlPathEncode(url); // ?? why here? why now?
            BinaryMetaFactory bmFactory = new BinaryMetaFactory();
            BinaryMeta binaryMeta = this.PublicationId == 0 ? (bmFactory.GetMetaByUrl(encodedUrl)[0] as BinaryMeta) : bmFactory.GetMetaByUrl(this.PublicationId, encodedUrl);

            Tridion.ContentDelivery.Meta.IComponentMeta componentMeta = GetTridionComponentMetaFactory(binaryMeta.PublicationId).GetMeta(binaryMeta.Id);
            return componentMeta == null ? DateTime.MinValue : componentMeta.LastPublicationDate;
        }

        public DateTime GetLastPublishedDateByUri(string uri)
        {
            TcmUri tcmUri = new TcmUri(uri);
            Tridion.ContentDelivery.Meta.IComponentMeta componentMeta = GetTridionComponentMetaFactory(tcmUri.PublicationId).GetMeta(tcmUri.ItemId);
            return componentMeta == null ? DateTime.MinValue : componentMeta.LastPublicationDate;
        }

        #endregion
    }
}
