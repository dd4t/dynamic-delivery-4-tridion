using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel.Factories;
using DD4T.ContentModel;
using System.Web;
using DD4T.ContentModel.Exceptions;
using DD4T.ContentModel.Contracts.Providers;
using DD4T.Providers.SDLTridion2011sp1;
using System.Web.Caching;

namespace DD4T.Factories
{
    public class BinaryFactory : FactoryBase, IBinaryFactory
    {
        private static IDictionary<string, DateTime> lastPublishedDates = new Dictionary<string, DateTime>();
        public IBinaryProvider BinaryProvider { get; set; }

        #region IBinaryFactory members
        public bool TryFindBinary(string url, out IBinary binary)
        {
            binary = null;
            string cacheKey = String.Format("Binary_{0}_{1}", url, PublicationId);
            Cache cache = HttpContext.Current.Cache;
            DateTime lastPublishedDate = DateTime.MinValue;
            if (lastPublishedDates.ContainsKey(url))
                lastPublishedDate = lastPublishedDates[url];

            var dbLastPublishedDate = BinaryProvider.GetLastPublishedDateByUrl(url);

            if (cache[cacheKey] != null && lastPublishedDate != DateTime.MinValue && lastPublishedDate.Subtract(dbLastPublishedDate).TotalSeconds >= 0)
            {
                binary = (IBinary)cache[cacheKey];
                return true;
            }
            else
            {
                byte[] binaryContent = BinaryProvider.GetBinaryByUrl(url);

                if (!(binaryContent == null || binaryContent.Length == 0))
                {
                    binary = GetIBinaryObject(binaryContent, url);
                    cache.Insert(cacheKey, binary);
                    lastPublishedDates[url] = dbLastPublishedDate;
                    return true;
                }
            }
            return false;
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
            string cacheKey = String.Format("Binary_{0}_{1}", tcmUri, PublicationId);
            Cache cache = HttpContext.Current.Cache;
            DateTime lastPublishedDate = DateTime.MinValue;
            if (lastPublishedDates.ContainsKey(tcmUri))
                lastPublishedDate = lastPublishedDates[tcmUri];

            var dbLastPublishedDate = BinaryProvider.GetLastPublishedDateByUri(tcmUri);

            if (cache[cacheKey] != null && lastPublishedDate != DateTime.MinValue && lastPublishedDate.Subtract(dbLastPublishedDate).TotalSeconds >= 0)
            {
                binary = (IBinary)cache[cacheKey];
                return true;
            }
            else
            {
                byte[] binaryContent = BinaryProvider.GetBinaryByUri(tcmUri);

                if (!binaryContent.Equals(String.Empty))
                {
                    binary = GetIBinaryObject(binaryContent, tcmUri);
                    cache.Insert(cacheKey, binary);
                    lastPublishedDates[tcmUri] = dbLastPublishedDate;
                    return true;
                }
            }
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
            bytes = null;
            try
            {
                bytes = BinaryProvider.GetBinaryByUrl(url);
                return true;
            }
            catch
            {
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

        public bool TryGetBinaryContent(string uri, out byte[] bytes)
        {
            bytes = null;
            try
            {
                bytes = BinaryProvider.GetBinaryByUri(uri);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public byte[] GetBinaryContent(string uri)
        {
            byte[] bytes;
            if (!TryFindBinaryContent(uri, out bytes))
            {
                throw new BinaryNotFoundException();
            }
            return bytes;
        }

        public bool HasBinaryChanged(string url)
        {
            return true; // TODO: implement
        }
        #endregion

        #region private
        private IBinary GetIBinaryObject(byte[] binaryContent, string url)
        {
            IBinary binary = new Binary();
            binary.BinaryData = binaryContent;
            binary.Url = url;
            return binary;
        }

        #endregion

    }
}
