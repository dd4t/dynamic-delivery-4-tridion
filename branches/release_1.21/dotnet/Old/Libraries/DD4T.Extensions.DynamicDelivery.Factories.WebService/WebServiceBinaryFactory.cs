using System;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using DD4T.Extensions.DynamicDelivery.ContentModel;
using DD4T.Extensions.DynamicDelivery.ContentModel.Exceptions;
using DD4T.Extensions.DynamicDelivery.ContentModel.Factories;
using DD4T.Extensions.DynamicDelivery.Factories;
using DD4T.Extensions.DynamicDelivery.Factories.WebService.Tridion.Broker;

namespace DD4T.Extensions.DynamicDelivery.Factories.WebService
{
    [Export(typeof(IBinaryFactory))]
    /// <summary>
    /// This is a development PageFactory. Intended to be used SOLELY on local development machine.
    /// </summary>
    public class WebServiceBinaryFactory : TridionFactoryBase ,IBinaryFactory, IDisposable
    {
        public bool TryFindBinary(string url, out IBinary binary)
        {
            using (var client = new TridionBrokerServiceClient())
            {
                binary = null;
                string binaryMetaXml = client.FindBinaryMetaByUrl(url, PublicationId);

                if (binaryMetaXml != "")
                {
                    XElement binaryMeta = XElement.Parse(binaryMetaXml);
                    DateTime lastPublishedDate = DateTime.Parse(binaryMeta.Element("LastPublishDate").Value);

                    binary = new Binary(this)
                    {
                        Url = binaryMeta.Element("Url").Value,
                        VariantId = binaryMeta.Element("VariantId").Value,
                        Id = String.Format("tcm:{0}-{1}", PublicationId, Convert.ToInt32(binaryMeta.Element("Id").Value)),
                        LastPublishedDate = lastPublishedDate
                    };

                    return true;
                }
                return false;
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
            using (var client = new TridionBrokerServiceClient())
            {
                binary = null; if (tcmUri == "") { return false; }
                string binaryMetaXml = client.GetBinaryMeta(tcmUri);
                if (binaryMetaXml != "")
                {
                    XElement binaryMeta = XElement.Parse(binaryMetaXml);
                    DateTime lastPublishedDate = DateTime.Parse(binaryMeta.Element("LastPublishDate").Value);
                    TcmUri uri = new TcmUri(tcmUri);

                    binary = new Binary(this)
                    {
                        Url = binaryMeta.Element("Url").Value,
                        VariantId = binaryMeta.Element("VariantId").Value,
                        Id = uri.ToString(),
                        LastPublishedDate = lastPublishedDate
                    };

                    return true;
                }

                return false;
            }
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
            using (var client = new TridionBrokerServiceClient())
            {
                bytes = null;
                byte[] tempBytes = client.FindBinaryByUrl(url, PublicationId);
                if (tempBytes != null)
                {
                    bytes = tempBytes;
                    return true;
                }

                return false;
            }
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
            using (var client = new TridionBrokerServiceClient())
            {
                bytes = null;
                byte[] tempBytes = client.GetBinary(tcmUri);
                if (tempBytes != null)
                {
                    bytes = tempBytes;
                    return true;
                }

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
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool isDisposed)
        {
            // Cleanup native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
