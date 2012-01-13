using DD4T.ContentModel.Contracts.Caching;
using DD4T.Factories.Caching;
using System;
using DD4T.Utils;

namespace DD4T.Factories
{
    /// <summary>
    /// Base class for all factories
    /// </summary>
    public abstract class FactoryBase 
    {

        private int? _publicationId = null;
        private ICacheAgent _cacheAgent = null;

        /// <summary>
        /// Returns the current publicationId
        /// </summary>  
        protected virtual int PublicationId 
        {
            get
            {
                if (_publicationId == null)
                    _publicationId = TridionHelper.PublicationId;
                return (int)_publicationId;
            }
            set
            {
                _publicationId = value;
            }
        }

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
    }
}
