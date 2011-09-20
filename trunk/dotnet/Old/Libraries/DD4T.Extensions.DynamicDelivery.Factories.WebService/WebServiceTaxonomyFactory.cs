using System.ComponentModel.Composition;
using System.Xml.Linq;
using Tridion.Extensions.DynamicDelivery.ContentModel;
using Tridion.Extensions.DynamicDelivery.ContentModel.Exceptions;
using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;
using Tridion.Extensions.DynamicDelivery.Factories;
using Tridion.Extensions.DynamicDelivery.Factories.WebService.Tridion.Broker;

namespace RaboWebMvc.Development.Factories
{
    [Export(typeof(ITaxonomyFactory))]
    /// <summary>
    /// This is a development PageFactory. Intended to be used SOLELY on local development machine.
    /// </summary>
    public class WebServiceTaxonomyFactory : TridionFactoryBase, ITaxonomyFactory
    {
        public bool TryGetKeyword(string categoryUriToLookIn, string keywordName, out IKeyword keyword)
        {
            using (var client = new TridionBrokerServiceClient())
            {
                string result = client.GetKeywordHierarchy(categoryUriToLookIn, keywordName);
                if (!string.IsNullOrEmpty(result))
                {
                    XElement keyWord = XElement.Parse(result);
                    var tkeyword = new Keyword()
                    {
                        Id = keyWord.Element("Uri").Value,
                        Title = keyWord.Element("KeywordName").Value,
                        TaxonomyId = keyWord.Element("TaxonomyUri").Value
                    };
                    foreach (XElement parentElement in keyWord.Element("ParentKeywords").Elements())
                    {
                        tkeyword.ParentKeywords.Add(
                            new Keyword
                            {
                                Id = parentElement.Attribute("Uri").Value,
                                Title = parentElement.Value,
                                TaxonomyId = parentElement.Attribute("TaxonomyUri").Value
                            }
                        );
                    }
                    keyword = tkeyword;
                    return true;
                }
            }

            keyword = null;
            return false;
        }

        public IKeyword GetKeyword(string categoryUriToLookIn, string keywordName)
        {
            IKeyword keyword;
            if (!TryGetKeyword(categoryUriToLookIn, keywordName, out keyword))
            {
                throw new KeywordNotFoundException();
            }

            return keyword;
        }
    }
}
