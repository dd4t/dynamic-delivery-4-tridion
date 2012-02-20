using System;
using DD4T.ContentModel;
using DD4T.ContentModel.Contracts.Providers;
using DD4T.ContentModel.Exceptions;
using DD4T.ContentModel.Factories;

namespace DD4T.Factories
{

    /// <summary>
    /// Provides access to taxonomies. Note: this class is not tested in any implementation yet! It also is completely uncached. More testing is needed!
    /// </summary>
    public class TaxonomyFactory : FactoryBase, ITaxonomyFactory
    {
        private ITaxonomyProvider taxonomyProvider = null;
        public ITaxonomyProvider TaxonomyProvider { get; set; }

        public bool TryGetKeyword(string categoryUriToLookIn, string keywordName, out IKeyword keyword)
        {
            keyword = null;
            try
            {
                keyword = TaxonomyProvider.GetKeyword(categoryUriToLookIn, keywordName);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public IKeyword GetKeyword(string categoryUriToLookIn, string keywordName)
        {

            IKeyword keyword;
            if (TryGetKeyword(categoryUriToLookIn, keywordName, out keyword))
            {
                return keyword;
            }
            throw new KeywordNotFoundException();
        }


        public override DateTime GetLastPublishedDateCallBack(string key, object cachedItem)
        {
            throw new NotImplementedException();
        }
    }
}

