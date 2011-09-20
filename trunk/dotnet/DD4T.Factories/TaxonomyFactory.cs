using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using DD4T.ContentModel;
using DD4T.ContentModel.Factories;
using DD4T.ContentModel.Exceptions;
using DD4T.ContentModel.Contracts.Providers;
using DD4T.Providers.SDLTridion2011;

namespace DD4T.Factories
{

    public class TaxonomyFactory : FactoryBase, ITaxonomyFactory
    {
        private ITaxonomyProvider taxonomyProvider = null;
        public ITaxonomyProvider TaxonomyProvider
        {
            get
            {
                // TODO: implement DI
                if (taxonomyProvider == null)
                {
                    taxonomyProvider = new TridionTaxonomyProvider();
                    taxonomyProvider.PublicationId = this.PublicationId;
                }
                return taxonomyProvider;
            }
            set
            {
                taxonomyProvider = value;
            }
        }

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

    }
}

