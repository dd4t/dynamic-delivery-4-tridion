using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel.Factories;

namespace DD4T.ContentModel.Contracts.Providers
{
    public interface IBinaryProvider : IProvider
    {
        byte[] GetBinaryByUri(string uri);
        byte[] GetBinaryByUrl(string url);
        DateTime GetLastPublishedDateByUrl(string url);
        DateTime GetLastPublishedDateByUri(string uri);
    }
}
