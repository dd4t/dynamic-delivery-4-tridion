using DD4T.Extensions.DynamicDelivery.ContentModel;
using DD4T.Extensions.DynamicDelivery.ContentModel.Factories;

namespace Tridion.Extensions.DynamicDelivery.Factories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.Composition;
    using Tridion.ContentDelivery.Web.Linking;
    using System.Web.Caching;
    using System.Web;

    [Export(typeof(ILinkFactory))]
    public class TridionLinkFactory : TridionFactoryBase, ILinkFactory, IDisposable
    {
        const string CACHEKEY_FORMAT = "Link_{0}";

        private ComponentLink componentLink;
        //private const string uriPrefix = "tcm:";
        private static TcmUri emptyTcmUri = new TcmUri("tcm:0-0-0");

        public TridionLinkFactory()
        {
            if (componentLink == null)
            {
                componentLink = new ComponentLink(PublicationId);
            }
        }

        public string ResolveLink(string componentUri)
        {
            TcmUri uri = new TcmUri(componentUri);
            
            if (!uri.Equals(emptyTcmUri))
            {
                Cache cache = HttpContext.Current.Cache;
                string cacheKey = String.Format(CACHEKEY_FORMAT, componentUri);
                if (cache[cacheKey] != null)
                {
                    return (String)cache[cacheKey];
                }
                else
                {
                    Link link = componentLink.GetLink(uri.ToString());
                    if (!link.IsResolved)
                    {
                        return null;
                    }
                    cache.Insert(cacheKey, link.Url, null, DateTime.Now.AddSeconds(30), TimeSpan.Zero); //TODO should this be configurable?
                    return link.Url;
                }
            }

            return null;
        }

        public string ResolveLink(string sourcePageUri, string componentUri, string excludeComponentTemplateUri)
        {
            TcmUri componentUriToLinkTo = new TcmUri(componentUri);
            TcmUri pageUri = new TcmUri(sourcePageUri);
            TcmUri componentTemplateUri = new TcmUri(excludeComponentTemplateUri);

            if (!componentUriToLinkTo.Equals(emptyTcmUri))
            {
                Link link = componentLink.GetLink(pageUri.ToString(), componentUriToLinkTo.ToString(), componentTemplateUri.ToString(), String.Empty, String.Empty, false, false);
                if (!link.IsResolved)
                {
                    return null;
                }
                return link.Url;
            }

            return null;
        }

        protected virtual void Dispose(bool isDisposed)
        {
            if (!isDisposed)
            {
                if (componentLink != null)
                {
                    componentLink.Dispose();
                    componentLink = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

