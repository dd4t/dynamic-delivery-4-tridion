namespace DD4T.Mvc.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml.Linq;
    using DD4T.ContentModel.Factories;
    using System.Collections;
    using DD4T.Mvc.Caching;
    using DD4T.Factories;
    using System.Diagnostics;
    using System.Configuration;

    public class TridionSiteMapProvider : StaticSiteMapProvider
    {
        
          // Sitemap attributes which are not considered part of route, you can add your custom attributes here.
        private string[] _excludedAttributes = { "title", "description", "roles", "page", "topmenuposition" };
        public static readonly string DefaultCacheKey = "rootNode";

        private bool ShouldResolveComponentLinks { get; set; }
        private int CacheTime { get; set; }
        private int PollTime { get; set; }
        public virtual IPageFactory PageFactory { get; set; }
        public virtual ILinkFactory LinkFactory { get; set; }
        public Dictionary<string, SiteMapNode> NodeDictionary { get; set; }

        protected string SiteMapPath
        {
            get
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains<string>("SitemapPath"))
                {
                    return ConfigurationManager.AppSettings["SitemapPath"];
                }
                return "/system/sitemap/sitemap.xml";
            }
        }


        private SiteMapNode ReadSitemapFromXml(string sitemapUrl)
        {
            SiteMapNode rootNode = null;
            NodeDictionary = new Dictionary<string, SiteMapNode>();

            // ideally, we would need a SiteMapSource property which is the file to read from, and a SiteMapFile property which is the file to write to
            // note: these two cannot be the same because the links in the sitemap are parsed, so the output is different from the input!
            //string sitemap = PageFactory.FindPageContent(sitemapUrl);
            string sitemap;
            if (!PageFactory.TryFindPageContent(sitemapUrl, out sitemap))
            {
                sitemap = emptySiteMapString();
            }
             
            XDocument xDoc = XDocument.Parse(sitemap);
            //XElement siteMapRoot = xDoc.Element("siteMap");
            XElement siteMapRoot = xDoc.Root;

            try
            {
                rootNode = new TridionSiteMapNode(this, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, new ArrayList(), new NameValueCollection(), new NameValueCollection(), String.Empty);
                AddNode(rootNode);

                //Fill down the hierarchy.
                AddChildren(rootNode, siteMapRoot.Elements(), 1);
            }
            catch (Exception e)
            {
                Exception e2 = e;
            }

            return rootNode;
        }

        private void AddChildren(SiteMapNode rootNode, IEnumerable<XElement> siteMapNodes, int currentLevel)
        {
            foreach (var element in siteMapNodes)
            {
                TridionSiteMapNode childNode;

                var attributes = new NameValueCollection();

                foreach (var a in element.Attributes())
                    attributes.Add(a.Name.ToString(), a.Value);

                childNode = new TridionSiteMapNode(this,
                    element.Attribute("id").Value, //key
                    element.Attribute("pageId")==null ? "" : element.Attribute("pageId").Value, //uri
                    element.Attribute("url").Value, //url
                    element.Attribute("title").Value, //title
                    element.Attribute("description").Value, //description
                    null, //roles
                    attributes, //attributes
                    null, //explicitresourceKeys
                    null) { Level = currentLevel }; // implicitresourceKey
                

                NodeDictionary.Add(childNode.Key, childNode);

                //Use the SiteMapNode AddNode method to add the SiteMapNode to the ChildNodes collection
                AddNode(childNode, rootNode);

                // Check for children in this node.
                AddChildren(childNode, element.Elements(), currentLevel + 1);
            }
        }

        public virtual bool IsInitialized
        {
            get;
            private set;
        }

        public override void Initialize(string name, NameValueCollection attributes) 
        {
            if (!IsInitialized)
            {
                base.Initialize(name, attributes);

                CacheTime = Int32.Parse(attributes["cacheTime"]);
                PollTime = Int32.Parse(attributes["pollTime"]);


                //if (! PageFactory.HasPageChanged(SitemapUrl))
                //{
                //    return;
                //}
                IsInitialized = true;
            }
        }

        public virtual string CacheKey
        {
            get
            {
                return DefaultCacheKey;
            }
        }

        public virtual SitemapCacheDependency GetSitemapCacheDependency (int PollTime, string SiteMapPath)
        {
            return new SitemapCacheDependency(PollTime, SiteMapPath);
        }

        public override SiteMapNode BuildSiteMap()
        {
            SiteMapNode rootNode;

            //lock (this) //KT: I don't think this is needed. 
            //{
            rootNode = HttpContext.Current.Cache[CacheKey] as SiteMapNode;
            if (rootNode == null)
            {
                base.Clear();
                //TODO: Get sitemap path from somewhere
                rootNode = ReadSitemapFromXml(SiteMapPath);
                if (rootNode == null) return rootNode; //TODO: Review errorHandling
                // Store the root node in the cache.
                HttpContext.Current.Cache.Insert(CacheKey, rootNode, GetSitemapCacheDependency(PollTime, SiteMapPath));
            }
            //}
            return rootNode;
        }

        private string emptySiteMapString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<siteMap publicationid=\"tcm:0-70-1\">");
            sb.Append("<siteMapNode title=\"website\" url=\"/\">");
            sb.Append("</siteMapNode>");
            sb.Append("</siteMap>");

            return sb.ToString();
        }

        protected override SiteMapNode GetRootNodeCore()
        {
            return BuildSiteMap();
        }

        public override SiteMapNode RootNode
        {
            get
            {
                return BuildSiteMap(); ;
            }
        }

        protected override void Clear()
        {
            lock (this)
            {
                HttpContext.Current.Cache.Remove("rootNode");
                base.Clear();
            }
        }

    }
}
