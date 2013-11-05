using System;
using DD4T.ContentModel.Contracts.Providers;

namespace DD4T.Providers.Test
{

    public class TridionLinkProvider : BaseProvider, ILinkProvider, IDisposable
    {

        public string ResolveLink(string componentUri)
        {
            throw new NotImplementedException();
        }

        public string ResolveLink(string sourcePageUri, string componentUri, string excludeComponentTemplateUri)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
        }
    }
}

