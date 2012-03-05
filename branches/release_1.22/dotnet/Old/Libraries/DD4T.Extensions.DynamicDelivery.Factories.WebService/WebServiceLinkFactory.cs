using System.ComponentModel.Composition;
using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;
using System;
using System.Web;
using System.Web.Caching;
using Tridion.Extensions.DynamicDelivery.Factories;
using Tridion.Extensions.DynamicDelivery.Factories.WebService.Tridion.Broker;

namespace Tridion.Extensions.DynamicDelivery.Factories.WebService
{
    [Export(typeof(ILinkFactory))]
    public class WebServiceLinkFactory : TridionFactoryBase, ILinkFactory
    {
        const string CACHEKEY_FORMAT = "Link_{0}";
        System.Random rdn = new System.Random();

        public string ResolveLink(string componentUri)
        {
            Cache cache = HttpContext.Current.Cache;
            string cacheKey = String.Format(CACHEKEY_FORMAT, componentUri);
            if (cache[cacheKey] != null)
            {
                return (String)cache[cacheKey];
            }
            else
            {
                using (var client = new TridionBrokerServiceClient())
                {
                    string resolvedLink = client.ResolveLink(PublicationId, componentUri);
                    cache.Insert(cacheKey, resolvedLink, null, DateTime.Now.AddSeconds(30), TimeSpan.Zero); //TODO should this be configurable?
                    return resolvedLink;
                }
            }
        }

        public string ResolveLink(string sourcePageUri,string componentUri, string excludeComponentTemplateUri)
        {
            return ResolveLink(componentUri);
        }
    }
}
