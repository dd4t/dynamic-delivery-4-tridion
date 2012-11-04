using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Compilation;
using System.Diagnostics;
using System.Xml;
using System.Configuration;
using DD4T.Factories;
using DD4T.ContentModel.Factories;
using DD4T.ContentModel.Contracts.Caching;
using DD4T.Utils;

namespace DD4T.Web.ResourceManagement.DynamicResourceProviderFactory
{
    public class DynamicResourceProviderFactory : ResourceProviderFactory
    {
        public override IResourceProvider CreateGlobalResourceProvider (string classKey)
        {
            LoggerService.Debug(">>CreateGlobalResourceProvider({0})", classKey);
            DynamicResourceProvider resourceProvider = new DynamicResourceProvider(classKey);
            LoggerService.Debug("<<CreateGlobalResourceProvider({0})", classKey);
            return resourceProvider;
        }

        public override IResourceProvider CreateLocalResourceProvider (string virtualPath)
        {
            string classKey = virtualPath;
            if (!string.IsNullOrEmpty(virtualPath))
            {
                virtualPath = virtualPath.Remove(0, 1);
                classKey = virtualPath.Remove(0, virtualPath.IndexOf('/') + 1);
            }
            return new DynamicResourceProvider(classKey);
        }
    }
    public class DynamicResourceProvider : IResourceProvider
    {
        #region configuration settings
        public readonly static string ResourcePath = ConfigurationManager.AppSettings["ResourcesPath"];
        #endregion

        public ICacheAgent CacheAgent { get; set; }

        #region private
        private Dictionary<string, XmlDocument> _resourceDocuments = new Dictionary<string, XmlDocument>();
        private string _classKey;
        private IPageFactory PageFactory { get; set; }
 
        private object lock1 = new object();
        private XmlDocument ResourceDocument
        {
            get
            {
                string cacheKey = string.Format("Resource_{0}", this._classKey);
                XmlDocument xmlDoc = CacheAgent.Load(cacheKey) as XmlDocument;
                if (xmlDoc != null)
                    return xmlDoc;

                string currentResourcePath = string.Format(ResourcePath, _classKey);
                xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(PageFactory.FindPageContent(currentResourcePath));
                CacheAgent.Store(cacheKey, "System", xmlDoc);
                return xmlDoc;
            }
        }

        private object lock2 = new object();

        #endregion

        #region constructor
        public DynamicResourceProvider(string classKey) 
        {
            _classKey = classKey;
        }
        #endregion


        public object GetObject(string resourceKey, System.Globalization.CultureInfo culture)
        {
            LoggerService.Debug(">>DynamicResourceProvider({0})", resourceKey);

            XmlElement v = ResourceDocument.SelectSingleNode(string.Format("/root/data[@name='{0}']/value", resourceKey)) as XmlElement;
            if (v == null)
                throw new ArgumentException(string.Format("Resource {0} does not exist in bundle {1}", resourceKey, _classKey));
            LoggerService.Debug("<<DynamicResourceProvider({0})", resourceKey);
            return v.InnerText;
        }

        public System.Resources.IResourceReader ResourceReader
        {
            get { throw new NotImplementedException(); }
        }
    }
}
