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
using System.Text.RegularExpressions;

namespace DD4T.Mvc.Providers
{
    public class TridionSiteMapNode : SiteMapNode
    {
        public TridionSiteMapNode(SiteMapProvider provider, string key, string uri, string url, string title, string description, IList roles, NameValueCollection attributes, NameValueCollection explicitResourceKeys, string implicitResourceKey):
            base(provider, key, url, title, description, roles, attributes, explicitResourceKeys, implicitResourceKey)
        {
            if (url.StartsWith("tcm:"))
            {
                url = MakeDummyUrl(url);
            }
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

        public string ResolvedUrl
        {
            get
            {
                if (string.IsNullOrEmpty(Uri))
                    return string.Empty;

                try
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
                catch
                {
                    return string.Empty;
                }            
            }
        }

        public string Uri { get; set; }

        public int Level { get; set; }

        private Regex reDummyUrl = new Regex("[^a-zA-Z0-9/\\-_\\.]");

        private string MakeDummyUrl(string inputUrl)
        {
            string tmp = reDummyUrl.Replace(inputUrl, "");
            return tmp.StartsWith("/") ? tmp : "/" + tmp;
        }
    }
}
