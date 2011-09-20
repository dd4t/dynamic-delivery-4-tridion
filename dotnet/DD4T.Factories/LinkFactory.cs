using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web;
using DD4T.ContentModel;
using DD4T.ContentModel.Factories;
using DD4T.ContentModel.Contracts.Providers;
using DD4T.Providers.SDLTridion2011;

namespace DD4T.Factories
{

    public class LinkFactory : FactoryBase, ILinkFactory
    {
        const string CACHEKEY_FORMAT = "Link_{0}";
        const string CACHEKEY_FORMAT_EXTENDED = "Link_{0}_{1}_{2}";
        const string CACHEVALUE_NULL = "UnresolvedLink_{0}_{1}_{2}";

        //private const string uriPrefix = "tcm:";
        private static TcmUri emptyTcmUri = new TcmUri("tcm:0-0-0");
        private ILinkProvider linkProvider;

        public ILinkProvider LinkProvider
        {
            get
            {
                // TODO: implement DI
                if (linkProvider == null)
                {
                    linkProvider = new TridionLinkProvider();
                    linkProvider.PublicationId = this.PublicationId;
                }
                return linkProvider;
            }
            set
            {
                linkProvider = value;
            }
        }
 
        public LinkFactory()
        {
        }

        public string ResolveLink(string componentUri)
        {
            Cache cache = HttpContext.Current.Cache;
            string cacheKey = String.Format(CACHEKEY_FORMAT, componentUri);
            if (cache[cacheKey] != null)
            {
                if (cache[cacheKey].Equals(CACHEVALUE_NULL))
                {
                    return null;
                }
                return (String)cache[cacheKey];
            }
            else
            {
                string resolvedUrl = LinkProvider.ResolveLink(componentUri);
                if (resolvedUrl == null)
                {
                    cache.Insert(cacheKey, CACHEVALUE_NULL, null, DateTime.Now.AddSeconds(30), TimeSpan.Zero); //TODO should this be configurable?
                }
                else
                {
                    cache.Insert(cacheKey, resolvedUrl, null, DateTime.Now.AddSeconds(30), TimeSpan.Zero); //TODO should this be configurable?
                }
                return resolvedUrl;
            }
        }

        public string ResolveLink(string sourcePageUri, string componentUri, string excludeComponentTemplateUri)
        {
            Cache cache = HttpContext.Current.Cache;
            string cacheKey = String.Format(CACHEKEY_FORMAT_EXTENDED, sourcePageUri, componentUri, excludeComponentTemplateUri);
            if (cache[cacheKey] != null)
            {
                if (cache[cacheKey].Equals(CACHEVALUE_NULL))
                {
                    return null;
                }
                return (String)cache[cacheKey];
            }
            else
            {
                string resolvedUrl = LinkProvider.ResolveLink(sourcePageUri, componentUri, excludeComponentTemplateUri);
                if (resolvedUrl == null)
                {
                    cache.Insert(cacheKey, CACHEVALUE_NULL, null, DateTime.Now.AddSeconds(30), TimeSpan.Zero); //TODO should this be configurable?
                }
                else
                {
                    cache.Insert(cacheKey, resolvedUrl, null, DateTime.Now.AddSeconds(30), TimeSpan.Zero); //TODO should this be configurable?
                }
                return resolvedUrl;
            }
        }


    }
}

