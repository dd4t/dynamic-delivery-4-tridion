using System;
using System.Web.Compilation;
using System.Xml;
using System.Configuration;
using DD4T.Factories;
using DD4T.ContentModel.Contracts.Caching;
using DD4T.ContentModel.Contracts.Factories;
using DD4T.Utils;
using DD4T.Factories.Caching;

namespace DD4T.Web.ResourceManagement
{
    public class DynamicResourceProviderFactory : ResourceProviderFactory
    {
        protected virtual IResourceProvider GetResourceProvider(string resourceName)
        {
            return new DynamicResourceProvider(resourceName);

        }

        public override IResourceProvider CreateGlobalResourceProvider(string resourceName)
        {
            LoggerService.Debug(">>CreateGlobalResourceProvider({0})", resourceName);
            return GetResourceProvider(resourceName);
        }

        public override IResourceProvider CreateLocalResourceProvider (string virtualPath)
        {
            LoggerService.Debug(">>CreateLocalResourceProvider({0})", virtualPath);
            string resourceName = virtualPath;
            if (!string.IsNullOrEmpty(virtualPath))
            {
                virtualPath = virtualPath.Remove(0, 1);
                resourceName = virtualPath.Remove(0, virtualPath.IndexOf('/') + 1);
            }
            return GetResourceProvider(resourceName);
        }
    }
    public class DynamicResourceProvider : IResourceProvider
    {
        public DynamicResourceProvider(string resourceName)
        {
            _resourceName = resourceName;
        }
        public readonly static string ResourcePath = ConfigurationManager.AppSettings["DD4T.ResourcePath"];

        private IPageFactory _pageFactory = null;
        protected virtual IPageFactory PageFactory
        {
            get
            {
                if (_pageFactory == null)
                    _pageFactory = new PageFactory();
                return _pageFactory;
            }
        }
        private ICacheAgent _cacheAgent = null;
        protected virtual ICacheAgent CacheAgent
        {
            get
            {
                if (_cacheAgent == null)
                    _cacheAgent = new DefaultCacheAgent();
                return _cacheAgent;
            }
        }
        protected virtual string GetPathToResource(string resourceName)
        {
            return string.Format(ResourcePath, resourceName);
        }

        private string _resourceName; 
        private object lock1 = new object();
        private XmlDocument ResourceDocument
        {
            get
            {
                string resourcePath = GetPathToResource(_resourceName);
                string cacheKey = string.Format("Resource_{0}", resourcePath);
                XmlDocument xmlDoc = CacheAgent.Load(cacheKey) as XmlDocument;
                if (xmlDoc != null)
                    return xmlDoc;

                lock (lock1)
                {
                    xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(PageFactory.FindPageContent(resourcePath));
                    CacheAgent.Store(cacheKey, "System", xmlDoc);
                    return xmlDoc;
                }
            }
        }



        public object GetObject(string resourceKey, System.Globalization.CultureInfo culture)
        {
            LoggerService.Debug(">>DynamicResourceProvider({0})", resourceKey);

            XmlElement v = ResourceDocument.SelectSingleNode(string.Format("/root/data[@name='{0}']/value", resourceKey)) as XmlElement;
            if (v == null)
                throw new ArgumentException(string.Format("Resource {0} does not exist in bundle {1}", resourceKey, _resourceName));
            LoggerService.Debug("<<DynamicResourceProvider({0})", resourceKey);
            return v.InnerText;
        }

        public System.Resources.IResourceReader ResourceReader
        {
            get { throw new NotImplementedException(); }
        }
    }
}
