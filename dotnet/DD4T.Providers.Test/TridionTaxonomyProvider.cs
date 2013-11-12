using System;
using DD4T.ContentModel.Contracts.Providers;

namespace DD4T.Providers.Test
{
    public class TridionTaxonomyProvider : BaseProvider, ITaxonomyProvider, IDisposable
    {
        public ContentModel.IKeyword GetKeyword(string categoryUriToLookIn, string keywordName)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
