using DD4T.ContentModel;
using DD4T.ContentModel.Factories;

namespace DD4T.Providers.SDLTridion2009
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Tridion.ContentDelivery.Web.Linking;
    using System.Web.Caching;
    using System.Web;
    using DD4T.ContentModel.Contracts.Providers;
    using DD4T.Utils;

    public class TridionLinkProvider : BaseProvider, ILinkProvider, IDisposable
    {

        private ComponentLink componentLink;
        //private const string uriPrefix = "tcm:";
        private static TcmUri emptyTcmUri = new TcmUri("tcm:0-0-0");
        protected static bool LinkToAnchor
        {
            get
            {
                return ConfigurationHelper.LinkToAnchor;
            }
        }
        protected static bool UseUriAsAnchor
        {
            get
            {
                return ConfigurationHelper.UseUriAsAnchor;
            }
        }

        public TridionLinkProvider()
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
                Link link = componentLink.GetLink(uri.ToString());
                return link.IsResolved ? link.Url : null;
            }
    
            return null;
        }

        public virtual string ResolveLink(string sourcePageUri, string componentUri, string excludeComponentTemplateUri)
        {
            TcmUri componentUriToLinkTo = new TcmUri(componentUri);
            TcmUri pageUri = new TcmUri(sourcePageUri);
            TcmUri componentTemplateUri = new TcmUri(excludeComponentTemplateUri);

            if (!componentUriToLinkTo.Equals(emptyTcmUri))
            {
                Link link = componentLink.GetLink(pageUri.ToString(), componentUriToLinkTo.ToString(), componentTemplateUri.ToString(), String.Empty, String.Empty, false, LinkToAnchor);
                if (!link.IsResolved)
                {
                    return null;
                }
                return LinkToAnchor && link.Anchor != "0" ? string.Format("{0}#{1}", link.Url, TridionHelper.GetLocalAnchorTag(pageUri, componentUriToLinkTo, componentTemplateUri, link.Anchor)) : link.Url;
            }

            return null;
        }



        #region IDisposable
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
        #endregion
    }
}

