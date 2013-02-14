using System;
using DD4T.ContentModel.Contracts.Caching;
using DD4T.Factories.Caching;
using DD4T.Utils;
using DD4T.ContentModel.Contracts.Resolvers;
using DD4T.Factories.Resolvers;

namespace DD4T.Factories
{
    /// <summary>
    /// Base class for all factories
    /// </summary>
    public abstract class FactoryBase
    {

        #region publication resolving
        private int? _publicationId = null;

        /// <summary>
        /// Returns the current publicationId
        /// </summary>  
        protected virtual int PublicationId 
        {
            get
            {
                if (_publicationId == null)
                    return PublicationResolver.ResolvePublicationId();
                return _publicationId.Value;
            }
            set
            {
                _publicationId = value;
            }
        }

        private IPublicationResolver _publicationResolver = null;
        public IPublicationResolver PublicationResolver
        {
            get
            {
                if (_publicationResolver == null)
                {
                    _publicationResolver = new DefaultPublicationResolver();
                }
                return _publicationResolver;
            }
            set
            {
                _publicationResolver = value;
            }
        }

        #endregion

        #region caching
        private ICacheAgent _cacheAgent = null;
        /// <summary>
        /// Abstract method to be overridden by each implementation. The method should return the DateTime when the object in the cache was last published.
        /// </summary>
        /// <param name="key">Key of the object in the cache</param>
        /// <param name="cachedItem">The object in the cache</param>
        /// <returns></returns>
        public abstract DateTime GetLastPublishedDateCallBack(string key, object cachedItem);

        /// <summary>
        /// Get or set the CacheAgent
        /// </summary>  
        public virtual ICacheAgent CacheAgent
        {
            get
            {
                if (_cacheAgent == null)
                {
                    _cacheAgent = new DefaultCacheAgent();
                }
                return _cacheAgent;
            }
            set
            {
                _cacheAgent = value;
            }
        }

        #endregion

        #region private properties
        protected bool IncludeLastPublishedDate
        {
            get
            {
                return ConfigurationHelper.IncludeLastPublishedDate;
            }
        }
        #endregion


    }
}
