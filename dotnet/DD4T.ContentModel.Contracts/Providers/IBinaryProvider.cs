using System;
using System.IO;

namespace DD4T.ContentModel.Contracts.Providers
{
    public interface IBinaryProvider : IProvider
    {
        byte[] GetBinaryByUri(string uri);
        byte[] GetBinaryByUrl(string url);
        Stream GetBinaryStreamByUri(string uri);
        Stream GetBinaryStreamByUrl(string url);
        DateTime GetLastPublishedDateByUrl(string url);
        DateTime GetLastPublishedDateByUri(string uri);
        string GetUrlForUri(string uri);
    }
}
