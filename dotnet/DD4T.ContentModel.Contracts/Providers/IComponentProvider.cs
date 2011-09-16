using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel.Factories;

namespace DD4T.ContentModel.Contracts.Providers
{
    public interface IComponentProvider : IProvider
    {
        string GetContent(string uri);
        DateTime GetLastPublishedDate(string uri);
        List<string> GetContentMultiple(string[] componentUris);
        IList<string> FindComponents(ExtendedQueryParameters queryParameters);
    }
}
