using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.Extensions.DynamicDelivery.ContentModel.Factories;
using DD4T.Extensions.DynamicDelivery.ContentModel;
using System.Web;
using DD4T.Extensions.DynamicDelivery.ContentModel.Exceptions;
using Tridion.ContentDelivery.DynamicContent.Query;
using Tridion.ContentDelivery.DynamicContent;

namespace DD4T.Extensions.DynamicDelivery.Factories
{
    public abstract class TridionBinaryFactoryBase : TridionFactoryBase, IBinaryFactory
    {
        public bool TryFindBinary(string url, out IBinary binary)
        {
            string encodedUrl = HttpUtility.UrlPathEncode(url);
            binary = null;
			//using (var sqlBinMetaHome = new Com.Tridion.Broker.Binaries.Meta.SQLBinaryMetaHome())
			//{
				
			//    Com.Tridion.Meta.BinaryMeta binaryMeta = sqlBinMetaHome.FindByURL(PublicationId, encodedUrl); // "/Images/anubis_pecunia160_tcm70-520973.jpg"                            
			//    if (binaryMeta != null)
			//    {
			//        using (var sqlBinaryHome = new Com.Tridion.Broker.Binaries.SQLBinaryHome())
			//        {
			//            Com.Tridion.Data.BinaryData binData = sqlBinaryHome.FindByPrimaryKey(PublicationId, (int)binaryMeta.GetId());
			//            if (binData != null)
			//            {
			//                binary = new Binary(this)
			//                {
			//                    Id = String.Format("tcm:{0}-{1}", binData.GetPublicationId(), binData.GetId()),
			//                    Url = url,
			//                    LastPublishedDate = DateTime.Now,
			//                    Multimedia = null,
			//                    VariantId = binData.GetVariantId()
			//                };
			//                return true;
			//            }
			//        }
			//    }
			//    return false;
			//}

			Query binaryQuery = new Query();
			ItemTypeCriteria isMultiMedia = new ItemTypeCriteria(32);
			
			PublicationCriteria inPub = new PublicationCriteria(PublicationId);
			Criteria allCriteria = CriteriaFactory.And(isMultiMedia, inPub);

			string[] results =  binaryQuery.ExecuteQuery();
			binary = null;
			return true;

			foreach (var item in results) {
				 				

			}


        }

        public IBinary FindBinary(string url)
        {
            IBinary binary;
            if (!TryFindBinary(url, out binary))
            {
                throw new BinaryNotFoundException();
            }

            return binary;
        }

        public bool TryGetBinary(string tcmUri, out IBinary binary)
        {
            binary = null;
			//using (var uri = new Com.Tridion.Util.TCMURI(tcmUri))
			//{
			//    using (var sqlBinHome = new Com.Tridion.Broker.Binaries.SQLBinaryHome())
			//    {
			//        var binData = sqlBinHome.FindByPrimaryKey(uri.GetPublicationId(), uri.GetItemId());
			//        using (var sqlBinMetaHome = new Com.Tridion.Broker.Binaries.Meta.SQLBinaryMetaHome())
			//        {
			//            using (Com.Tridion.Util.TCDURI tcdUri = new Com.Tridion.Util.TCDURI(uri))
			//            {
			//                Com.Tridion.Meta.BinaryMeta binaryMeta = sqlBinMetaHome.FindByPrimaryKey(tcdUri);

			//                if (binData != null)
			//                {
			//                    binary = new Binary(this)
			//                    {
			//                        Url = binaryMeta.GetURLPath(),
			//                        LastPublishedDate = DateTime.Now,
			//                        Multimedia = null,
			//                        VariantId = binData.GetVariantId()
			//                    };

			//                    return true;
			//                }
			//            }
			//        }
			//    }
			//}
            return false;
        }

        public IBinary GetBinary(string tcmUri)
        {
            IBinary binary;
            if (!TryGetBinary(tcmUri, out binary))
            {
                throw new BinaryNotFoundException();
            }
            return binary;
        }

        public bool TryFindBinaryContent(string url, out byte[] bytes)
        {
            string encodedUrl = HttpUtility.UrlPathEncode(url);
            bytes = null;
			//using (var sqlBinMetaHome = new Com.Tridion.Broker.Binaries.Meta.SQLBinaryMetaHome())
			//{
			//    var coll = sqlBinMetaHome.FindByURL(encodedUrl);
			//    using (var d = new Com.Tridion.Broker.Binaries.SQLBinaryHome())
			//    {
			//        foreach (Com.Tridion.Meta.BinaryMeta item in coll)
			//        {
			//            if (!item.GetPublicationId().Equals(PublicationId)) continue;

			//            Com.Tridion.Data.BinaryData binData = d.FindByPrimaryKey(PublicationId, (int)item.GetId());
			//            if (binData != null)
			//            {
			//                bytes = binData.GetBytes();
			//                return true;
			//            }
			//        }
			//    }
			//}

            return false;
        }

        public byte[] FindBinaryContent(string url)
        {
            byte[] bytes;
            if (!TryFindBinaryContent(url, out bytes))
            {
                throw new BinaryNotFoundException();
            }

            return bytes;
        }

        public bool TryGetBinaryContent(string tcmUri, out byte[] bytes)
        {
            bytes = null;
			//using (var uri = new Com.Tridion.Util.TCMURI(tcmUri))
			//{
			//    using (var sqlBinHome = new Com.Tridion.Broker.Binaries.SQLBinaryHome())
			//    {
			//        var binData = sqlBinHome.FindByPrimaryKey(uri.GetPublicationId(), uri.GetItemId());
			//        if (binData != null)
			//        {
			//            bytes = binData.GetBytes();
			//            return true;
			//        }
			//    }
                return false;
			//}

        }

        public byte[] GetBinaryContent(string tcmUri)
        {
            byte[] bytes;
            if (!TryGetBinaryContent(tcmUri, out bytes))
            {
                throw new BinaryNotFoundException();
            }

            return bytes;
        }

        public bool HasBinaryChanged(string url)
        {
            return true;
        }
    }
}
