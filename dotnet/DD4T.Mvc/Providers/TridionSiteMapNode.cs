using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections;
using System.Collections.Specialized;
using DD4T.ContentModel.Factories;
using DD4T.Factories;
using DD4T.ContentModel;

namespace DD4T.Mvc.Providers
{
    public class TridionSiteMapNode : SiteMapNode
    {
        public TridionSiteMapNode(SiteMapProvider provider, string key, string uri, string url, string title, string description, IList roles, NameValueCollection attributes, NameValueCollection explicitResourceKeys, string implicitResourceKey):
            base(provider, key, url, title, description, roles, attributes, explicitResourceKeys, implicitResourceKey)
        {
            Uri = uri;
        }

        public override bool HasChildNodes
        {
            get
            {
                return ChildNodes.Count > 0;
            }
        }
        public override SiteMapNodeCollection ChildNodes
        {
            get
            {
                return base.ChildNodes;
            }
            set
            {
                base.ChildNodes = value;
            }
        }

        //public override string Url
        //{
        //    get
        //    {
        //        return null; //Must return null to allow node to be added to sitemap. Resolving happens in ResolvedUrl
        //    }
        //    set
        //    {
        //        base.Url = value;
        //    }
        //}

        public string ResolvedUrl
        {
            get
            {
                TcmUri tcmUri = new TcmUri(Uri);
                if (tcmUri.ItemTypeId == 16)
                {
                    if (!String.IsNullOrEmpty(Uri))
                    {
                        ILinkFactory linkFactory = new LinkFactory();
                        string resolvedLink = linkFactory.ResolveLink(Uri);
                        if (!String.IsNullOrEmpty(resolvedLink))
                        {
                            return resolvedLink;
                        }
                    }
                    return null;
                }
                
                return Url;
            }
        }

        public string Uri { get; set; }

        public int Level { get; set; }
    }
}
