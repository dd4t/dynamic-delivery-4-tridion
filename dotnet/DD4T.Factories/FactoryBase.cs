using System;
using DD4T.ContentModel.Contracts.Caching;
using DD4T.Factories.Caching;
using DD4T.Utils;
using DD4T.ContentModel.Contracts.Resolvers;
using DD4T.Factories.Resolvers;
using DD4T.ContentModel.Contracts.Serializing;
using DD4T.Serialization;

namespace DD4T.Factories
{
    /// <summary>
    /// Base class for all factories
    /// </summary>
    public abstract class FactoryBase
    {

        #region serialization
        private ISerializerService _serializerService = null;
        private object lock1 = new object();
        public ISerializerService SerializerService
        {
            get
            {

                if (_serializerService == null)
                {
                    lock (lock1)
                    {
                        if (_serializerService == null)
                            _serializerService = FindBestService();
                    }
                }
                return _serializerService;
            }
            set
            {
                _serializerService = value;
            }
        }

        private ISerializerService FindBestService()
        {
            ISerializerService s;
            if (ConfigurationHelper.SerializationFormat == SerializationFormats.JSON)
            {
                s = new JSONSerializerService();
                if (s.IsAvailable())
                    return s;
            }
            if (ConfigurationHelper.SerializationFormat == SerializationFormats.XML)
            {
                s = new XmlSerializerService();
                if (s.IsAvailable())
                    return s;
            }
            // service for the configured serialization format is unavailable, pick one of the available services
            s = new JSONSerializerService();
            if (s.IsAvailable())
                return s;
            s = new XmlSerializerService();
            if (s.IsAvailable())
                return s;

            throw new Exception("Unsupported serialization format: " + ConfigurationHelper.SerializationFormat);

        }
        #endregion

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
