using System;
using DD4T.ContentModel.Contracts.Providers;

namespace DD4T.Providers.Test
{
    /// <summary>
    /// Provide access to binaries in a Tridion broker instance
    /// </summary>
    public class TridionBinaryProvider : BaseProvider, IBinaryProvider
    {

        public byte[] GetBinaryByUri(string uri)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBinaryByUrl(string url)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream GetBinaryStreamByUri(string uri)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream GetBinaryStreamByUrl(string url)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastPublishedDateByUrl(string url)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastPublishedDateByUri(string uri)
        {
            throw new NotImplementedException();
        }


        public string GetUrlForUri(string uri)
        {
            throw new NotImplementedException();
        }
    }
}
