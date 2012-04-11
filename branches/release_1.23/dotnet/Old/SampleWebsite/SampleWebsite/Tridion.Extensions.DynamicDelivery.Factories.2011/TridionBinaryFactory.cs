using System;
using System.ComponentModel.Composition;
using System.Web;

using DD4T.Extensions.DynamicDelivery.ContentModel;
using DD4T.Extensions.DynamicDelivery.ContentModel.Exceptions;
using DD4T.Extensions.DynamicDelivery.ContentModel.Factories;

using Tridion.ContentDelivery.DynamicContent;
using Tridion.ContentDelivery.DynamicContent.Filters;
using Tridion.ContentDelivery.DynamicContent.Query;
using Query = Tridion.ContentDelivery.DynamicContent.Query.Query;
using Tridion.ContentDelivery.Meta;
using Com.Tridion.Broker.Querying.Criteria.Content;
using ComponentPresentation = Tridion.ContentDelivery.DynamicContent.ComponentPresentation;

namespace Tridion.Extensions.DynamicDelivery.Factories
{
    [Export(typeof(IBinaryFactory))]
    /// <summary>
    /// This is a temporary PageFactory, to be replaced by the DynamicDelivery.PageFactory
    /// </summary>
    public class TridionBinaryFactory2011 : TridionFactoryBase,  IBinaryFactory
    {
        public bool TryFindBinary(string url, out IBinary binary)
        {
            string encodedUrl = HttpUtility.UrlPathEncode(url);
            binary = null;
            return false;

            Query findBinary = new Query(); 
            PublicationURLCriteria urlCriteria = new PublicationURLCriteria(url);
            //MultimediaCriteria isBinary = new MultimediaCriteria(true);

            //Criteria allCriteria = CriteriaFactory.And(isBinary, urlCriteria);
            Criteria allCriteria = urlCriteria;
            findBinary.Criteria = allCriteria;

            string[] binaryUri = findBinary.ExecuteQuery();

            if (binaryUri.Length == 0)
            {
                return false;
            }
            else
            {
                ComponentPresentation binaryComponent ;
                

                
                return true;
            }

            /*
            using (var sqlBinMetaHome = new Com.Tridion.Broker.Binaries.Meta.SQLBinaryMetaHome())
            {
                Com.Tridion.Meta.BinaryMeta binaryMeta = sqlBinMetaHome.FindByURL(PublicationId, encodedUrl); // "/Images/anubis_pecunia160_tcm70-520973.jpg"                            
                if (binaryMeta != null)
                { 
                    using (var sqlBinaryHome = new Com.Tridion.Broker.Binaries.SQLBinaryHome())
                    {
                        Com.Tridion.Data.BinaryData binData = sqlBinaryHome.FindByPrimaryKey(PublicationId, (int)binaryMeta.GetId());
                        if (binData != null)
                        {
                            binary = new Binary(this)
                            {
                                Url = url,
                                LastPublishedDate = DateTime.Now,
                                Multimedia = null,
                                VariantId = binData.GetVariantId()
                            };
                            return true;
                        }                        
                    }
                }
                return false;
            }
            */
           
        }

        public IBinary FindBinary(string url)
        {
           IBinary binary;
           if(!TryFindBinary(url, out binary))
           {
               throw new BinaryNotFoundException();
           }

           return binary;
        }

        public bool TryGetBinary(string tcmUri, out IBinary binary)
        {
            binary = null;

            Query findBinary = new Query();
            MultimediaCriteria isBinary = new MultimediaCriteria(true);
            return false;

            /*
            using (var uri = new Com.Tridion.Util.TCMURI(tcmUri))
            {
                using (var sqlBinHome = new Com.Tridion.Broker.Binaries.SQLBinaryHome())
                {
                    var binData = sqlBinHome.FindByPrimaryKey(uri.GetPublicationId(), uri.GetItemId());
                    var sqlBinMetaHome = new Com.Tridion.Broker.Binaries.Meta.SQLBinaryMetaHome();
                    Com.Tridion.Util.TCDURI tcdUri = new Com.Tridion.Util.TCDURI(uri);
                    Com.Tridion.Meta.BinaryMeta binaryMeta = sqlBinMetaHome.FindByPrimaryKey(tcdUri);
                    
                    if (binData != null)
                    {
                        binary = new Binary(this)
                        {
                            Url = binaryMeta.GetURLPath(),
                            LastPublishedDate = DateTime.Now,
                            Multimedia = null,
                            VariantId = binData.GetVariantId()
                        };

                        return true;
                    }
                }
            }
            */

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
            return false;
            /*
            using (var sqlBinMetaHome = new Com.Tridion.Broker.Binaries.Meta.SQLBinaryMetaHome())
            {
                var coll = sqlBinMetaHome.FindByURL(encodedUrl); 
                using (var d = new Com.Tridion.Broker.Binaries.SQLBinaryHome())
                {
                    foreach (Com.Tridion.Meta.BinaryMeta item in coll)
                    {
                        if (!item.GetPublicationId().Equals(PublicationId)) continue;

                        Com.Tridion.Data.BinaryData binData = d.FindByPrimaryKey(PublicationId, (int)item.GetId());
                        if (binData != null)
                        {
                            bytes = binData.GetBytes();
                            return true;
                        }                                              
                    }
                }
            }
            */
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
            return false;
            
            using (var uri = new Com.Tridion.Util.TCMURI(tcmUri))
            {
                /*
                using (var sqlBinHome = new Com.Tridion.Broker.Binaries.SQLBinaryHome())
                {
                    var binData = sqlBinHome.FindByPrimaryKey(uri.GetPublicationId(), uri.GetItemId());
                    if (binData != null)
                    {
                        bytes = binData.GetBytes();
                        return true;
                    }
                }
                */ 
                return false;
            }
            
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