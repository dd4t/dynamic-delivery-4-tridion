using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;
using Tridion.Extensions.DynamicDelivery.ContentModel;
using System.ComponentModel.Composition;

namespace Tridion.Extensions.DynamicDelivery.Factories.LocalDatabase
{
    [Export(typeof(ITaxonomyFactory))]
    public class TaxonomyFactory : ITaxonomyFactory
    {
        #region ITaxonomyFactory Members

        public bool TryGetKeyword(string categoryUriToLookIn, string keywordName, out IKeyword keyword)
        {
            throw new NotImplementedException();
        }

        public IKeyword GetKeyword(string categoryUriToLookIn, string keywordName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
