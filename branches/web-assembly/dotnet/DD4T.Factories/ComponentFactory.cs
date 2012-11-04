using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using DD4T.ContentModel;
using DD4T.ContentModel.Contracts.Caching;
using DD4T.ContentModel.Contracts.Providers;
using DD4T.ContentModel.Exceptions;
using DD4T.ContentModel.Factories;
using DD4T.ContentModel.Logging;
using DD4T.ContentModel.Querying;
using DD4T.Factories.Caching;
using DD4T.Utils;

namespace DD4T.Factories
{
    /// <summary>
    /// Factory for the creation of IComponents
    /// </summary>
    public class ComponentFactory : FactoryBase, IComponentFactory
    {
        public const string CacheKeyFormatByUri = "ComponentByUri_{0}";
        public const string CacheRegion = "Component";
        private ICacheAgent _cacheAgent = null;

        private IComponentProvider _componentProvider = null;
        public IComponentProvider ComponentProvider
        {
            get
            {
                if (_componentProvider == null)
                {
                    _componentProvider = (IComponentProvider)ProviderLoader.LoadProvider<IComponentProvider>();
                }
                return _componentProvider;
            }
            set
            {
                _componentProvider = value;
            }
        }

        private static XmlSerializer _componentSerializer = null;
        private static XmlSerializer ComponentSerializer
        {
            get
            {
                if (_componentSerializer == null)
                    _componentSerializer = new XmlSerializer(typeof(Component));
                return _componentSerializer;
            }
        }

        #region IComponentFactory members
        public bool TryGetComponent(string componentUri, out IComponent component)
        {
            LoggerService.Debug(">>TryGetComponent ({0})", LoggingCategory.Performance, componentUri);

            component = null;

            string cacheKey = String.Format(CacheKeyFormatByUri, componentUri);
            component = (IComponent)CacheAgent.Load(cacheKey);

            if (component != null)
            {
                LoggerService.Debug("<<TryGetComponent ({0}) - from cache", LoggingCategory.Performance, componentUri);
                return true;
            }

            string content = ComponentProvider.GetContent(componentUri);

            if (string.IsNullOrEmpty(content))
            {
                LoggerService.Debug("<<TryGetComponent ({0}) - from provider", LoggingCategory.Performance, componentUri);
                return false;
            }

            LoggerService.Debug("about to create IComponent from content ({0})", LoggingCategory.Performance, componentUri);
            component = GetIComponentObject(content);
            LoggerService.Debug("finished creating IComponent from content ({0})", LoggingCategory.Performance, componentUri);

            if (IncludeLastPublishedDate)
                ((Component)component).LastPublishedDate = ComponentProvider.GetLastPublishedDate(componentUri);
            LoggerService.Debug("about to store IComponent in cache ({0})", LoggingCategory.Performance, componentUri);
            CacheAgent.Store(cacheKey, CacheRegion, component);
            LoggerService.Debug("finished storing IComponent in cache ({0})", LoggingCategory.Performance, componentUri);
            LoggerService.Debug("<<TryGetComponent ({0})", LoggingCategory.Performance, componentUri);
            return true;
        }


        public IComponent GetComponent(string componentUri)
        {
            LoggerService.Debug(">>GetComponent ({0})", LoggingCategory.Performance, componentUri);
            IComponent component;
            if (!TryGetComponent(componentUri, out component))
            {
                LoggerService.Debug("<<GetComponent ({0}) -- not found", LoggingCategory.Performance, componentUri);
                throw new ComponentNotFoundException();
            }

            LoggerService.Debug("<<GetComponent ({0})", LoggingCategory.Performance, componentUri);
            return component;
        }


        /// <summary>
        /// Create IComponent from a string representing that IComponent (XML)
        /// </summary>
        /// <param name="componentStringContent">XML content to deserialize into an IComponent</param>
        /// <returns></returns>
        public IComponent GetIComponentObject(string componentStringContent)
        {
            XmlDocument componentContent = new XmlDocument();
            componentContent.LoadXml(componentStringContent);
            
            IComponent component = null;
            using (var reader = new XmlNodeReader(componentContent.DocumentElement))
            {
                component = (IComponent)ComponentSerializer.Deserialize(reader);
            }
            return component;
        }


        /// <summary>
        /// Returns the Component contents which could be found. Components that couldn't be found don't appear in the list. 
        /// </summary>
        /// <param name="componentUris"></param>
        /// <returns></returns>
        public IList<IComponent> GetComponents(string[] componentUris)
        {
            List<IComponent> components = new List<IComponent>();
            foreach (string content in ComponentProvider.GetContentMultiple(componentUris))
            {
                components.Add(GetIComponentObject(content));
            }
            return components;
        }

        public IList<IComponent> FindComponents(IQuery queryParameters, int pageIndex, int pageSize, out int totalCount)
        {
            LoggerService.Debug(">>FindComponents ({0},{1})", LoggingCategory.Performance, queryParameters.ToString(), Convert.ToString(pageIndex));
            totalCount = 0;
            IList<string> results = ComponentProvider.FindComponents(queryParameters);
            totalCount = results.Count;

            var pagedResults = results
                .Skip(pageIndex*pageSize)
                .Take(pageSize)
                .Select(c => { IComponent comp = null; TryGetComponent(c, out comp); return comp; })
                .Where(c => c!= null)
                .ToList();

            LoggerService.Debug("<<FindComponents ({0},{1})", LoggingCategory.Performance, queryParameters.ToString(), Convert.ToString(pageIndex));
            return pagedResults;

        }

        public IList<IComponent> FindComponents(IQuery queryParameters)
        {
            LoggerService.Debug(">>FindComponents ({0})", LoggingCategory.Performance, queryParameters.ToString());

            var results = ComponentProvider.FindComponents(queryParameters)
                .Select(c => { IComponent comp = null; TryGetComponent(c, out comp); return comp; })
                .Where(c => c!= null)
                .ToList();
            LoggerService.Debug("<<FindComponents ({0})", LoggingCategory.Performance, queryParameters.ToString());
            return results;
        }

        public DateTime GetLastPublishedDate(string uri)
        {
            return ComponentProvider.GetLastPublishedDate(uri);
        }

        public override DateTime GetLastPublishedDateCallBack(string key, object cachedItem)
        {
            if (cachedItem == null)
                return DateTime.Now; // this will force the item to be removed from the cache
            if (cachedItem is IComponent)
            {
                return GetLastPublishedDate(((IComponent)cachedItem).Id);
            }
            throw new Exception(string.Format("GetLastPublishedDateCallBack called for unexpected object type '{0}' or with unexpected key '{1}'", cachedItem.GetType(), key));
        }
        /// <summary>
        /// Get or set the CacheAgent
        /// </summary>  
        public override ICacheAgent CacheAgent
        {
            get
            {
                if (_cacheAgent == null)
                {
                    _cacheAgent = new DefaultCacheAgent();
                    // the next line is the only reason we are overriding this property: to set a callback
                    _cacheAgent.GetLastPublishDateCallBack = GetLastPublishedDateCallBack;
                }
                return _cacheAgent;
            }
            set
            {
                _cacheAgent = value;
                _cacheAgent.GetLastPublishDateCallBack = GetLastPublishedDateCallBack;
            }
        }

		#endregion
	}
}
