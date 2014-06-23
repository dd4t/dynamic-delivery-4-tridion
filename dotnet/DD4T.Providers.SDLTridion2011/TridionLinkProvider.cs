using DD4T.ContentModel;

namespace DD4T.Providers.SDLTridion2011
{
    using System;
    using System.Collections.Generic;
    using Tridion.ContentDelivery.Web.Linking;
    using DD4T.ContentModel.Contracts.Providers;
    using DD4T.Utils;

    
    public class TridionLinkProvider : BaseProvider, ILinkProvider, IDisposable
    {

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
        private Dictionary<int, ComponentLink> _componentLinks = new Dictionary<int, ComponentLink>();
        protected ComponentLink GetComponentLink(TcmUri uri)
        {
            if (!_componentLinks.ContainsKey(uri.PublicationId))
            {
                _componentLinks.Add(uri.PublicationId, new ComponentLink(uri.PublicationId));
            }
            return _componentLinks[uri.PublicationId];
        }



        public string ResolveLink(string componentUri)
        {
            return ResolveLink(TcmUri.NullUri.ToString(), componentUri, TcmUri.NullUri.ToString());

        }

        public virtual string ResolveLink(string sourcePageUri, string componentUri, string excludeComponentTemplateUri)
        {
            TcmUri componentUriToLinkTo = new TcmUri(componentUri);
            TcmUri pageUri = new TcmUri(sourcePageUri);
            TcmUri componentTemplateUri = new TcmUri(excludeComponentTemplateUri);

            if (!componentUriToLinkTo.Equals(emptyTcmUri))
            {
                Link link = GetComponentLink(componentUriToLinkTo).GetLink(pageUri.ToString(), componentUriToLinkTo.ToString(), componentTemplateUri.ToString(), String.Empty, String.Empty, false, LinkToAnchor);
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
                foreach (ComponentLink cl in _componentLinks.Values)
                {
                    if (cl != null)
                    {
                        cl.Dispose();
                    }
                }
                _componentLinks.Clear();
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

