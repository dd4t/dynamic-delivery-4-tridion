using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections;
using System.Collections.Specialized;
using DD4T.Extensions.DynamicDelivery.ContentModel.Factories;

namespace DD4T.Extensions.DynamicDelivery.Mvc.Providers
{
    public class TridionSiteMapNode : SiteMapNode
    {
        public TridionSiteMapNode(SiteMapProvider provider, string key, string uri, string title, string description, IList roles, NameValueCollection attributes, NameValueCollection explicitResourceKeys, string implicitResourceKey):
            base(provider, key, String.Empty, title, description, roles, attributes, explicitResourceKeys, implicitResourceKey)
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
                SiteMapNodeCollection resultChildNodes = new SiteMapNodeCollection();

                SiteMapNodeCollection childNodes = base.ChildNodes;
                foreach (TridionSiteMapNode childNode in childNodes)
                {
                    if (!String.IsNullOrEmpty(childNode.ResolvedUrl))
                    {
                        resultChildNodes.Add(childNode);
                    }
                }
                return resultChildNodes;
            }
            set
            {
                base.ChildNodes = value;
            }
        }

        public override string Url
        {
            get
            {
                return null; //Must return null to allow node to be added to sitemap. Resolving happens in ResolvedUrl
            }
            set
            {
                base.Url = value;
            }
        }

        public string ResolvedUrl
        {
            get
            {
                if (!String.IsNullOrEmpty(Uri))
                {
                    ILinkFactory linkFactory = FactoryService.LinkFactory;
                    string resolvedLink = linkFactory.ResolveLink(Uri);
                    if (!String.IsNullOrEmpty(resolvedLink))
                    {
                        return resolvedLink;
                    }
                }
                return null;
            }
        }

        public string Uri { get; set; }

        public int Level { get; set; }
    }
}
