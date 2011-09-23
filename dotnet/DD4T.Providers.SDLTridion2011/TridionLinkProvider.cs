using DD4T.ContentModel;
using DD4T.ContentModel.Factories;

namespace DD4T.Providers.SDLTridion2011
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.Composition;
    using Tridion.ContentDelivery.Web.Linking;
    using System.Web.Caching;
    using System.Web;
    using DD4T.ContentModel.Contracts.Providers;
    using System.Diagnostics;

    [Export(typeof(ILinkProvider))]
    public class TridionLinkProvider : BaseProvider, ILinkProvider, IDisposable
    {

        private ComponentLink componentLink = null;
        //private const string uriPrefix = "tcm:";
        private static TcmUri emptyTcmUri = new TcmUri("tcm:0-0-0");

        public ComponentLink ComponentLink
        {
            get
            {
                if (componentLink == null) 
                    componentLink = new ComponentLink(PublicationId);
                return componentLink;
            }
        }


        public string ResolveLink(string componentUri)
        {
            TcmUri uri = new TcmUri(componentUri);

            if (!uri.Equals(emptyTcmUri))
            {
                Link link = ComponentLink.GetLink(uri.ToString());
                return link.IsResolved ? link.Url : null;
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
                Link link = ComponentLink.GetLink(pageUri.ToString(), componentUriToLinkTo.ToString(), componentTemplateUri.ToString(), String.Empty, String.Empty, false, false);
                if (!link.IsResolved)
                {
                    return null;
                }
                return link.Url;
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

