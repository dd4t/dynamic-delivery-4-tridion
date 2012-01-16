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
using System.IO;

namespace DD4T.Factories
{
    public class BinaryFactory : FactoryBase, IBinaryFactory
    {
        private static IDictionary<string, DateTime> lastPublishedDates = new Dictionary<string, DateTime>();
        public IBinaryProvider BinaryProvider { get; set; }

        #region IBinaryFactory members
        public bool TryFindBinary(string url, out IBinary binary)
        {
            binary = new Binary();

            if (LoadBinariesAsStream)
            {
                binary.BinaryStream = BinaryProvider.GetBinaryStreamByUrl(url);
                if (binary.BinaryStream == null)
                    return false;
            }
            else
            {
                binary.BinaryData = BinaryProvider.GetBinaryByUrl(url);
                if (binary.BinaryData == null || binary.BinaryData.Length == 0)
                    return false;
            }

            return true;
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

        public DateTime FindLastPublishedDate(string url)
        {
            return BinaryProvider.GetLastPublishedDateByUrl(url);
        }

        public bool TryGetBinary(string tcmUri, out IBinary binary)
        {
            binary = new Binary();
            if (LoadBinariesAsStream)
            {
                binary.BinaryStream = BinaryProvider.GetBinaryStreamByUri(tcmUri);
                if (binary.BinaryStream == null)
                    return false;
            }
            else
            {
                binary.BinaryData = BinaryProvider.GetBinaryByUri(tcmUri);
                if (binary.BinaryData == null || binary.BinaryData.Length == 0)
                    return false;
            }
            ((Binary)binary).Id = tcmUri;
            return true;
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
        private IBinary GetIBinaryObject(Stream binaryStream, string url)
        {
            IBinary binary = new Binary();
            binary.BinaryStream = binaryStream;
            binary.Url = url;
            return binary;
        }
        #endregion

        
        public static bool DefaultLoadBinariesAsStream = false;
        private bool _loadBinariesAsStream = DefaultLoadBinariesAsStream;
        public bool LoadBinariesAsStream
        {
            get
            {
                return _loadBinariesAsStream;
            }
            set
            {
                _loadBinariesAsStream = value;
            }
        }

        public override DateTime GetLastPublishedDateCallBack(string key, object cachedItem)
        {
            throw new NotImplementedException();
        }



        public string GetUrlForUri(string uri)
        {
            return BinaryProvider.GetUrlForUri(uri);
        }
    }
}
