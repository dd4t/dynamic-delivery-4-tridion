using System;
using DD4T.ContentModel.Contracts.Caching;
using DD4T.Factories.Caching;
using DD4T.Utils;
using DD4T.ContentModel.Contracts.Resolvers;
using DD4T.Factories.Resolvers;
using DD4T.ContentModel.Contracts.Serializing;
using DD4T.Serialization;
using System.Collections.Generic;

namespace DD4T.Factories
{
    /// <summary>
    /// Base class for all factories
    /// </summary>
    public abstract class FactoryBase
    {

        #region serialization
        private Dictionary<SerializationFormats, ISerializerService> _serializerServices = new Dictionary<SerializationFormats,ISerializerService>();
        private object lock1 = new object();

        protected ISerializerService GetSerializationService(SerializationFormats format = SerializationFormats.UNKNOWN)
        {
            if (format == SerializationFormats.UNKNOWN)
            {
                format = ConfigurationHelper.SerializationFormat;
            }
            if (!_serializerServices.ContainsKey(format))
            {
                lock(lock1)
                {
                    _serializerServices.Add(format, CreateSerializationService(format));
                }
            }
            return _serializerServices[format];
        }

        private ISerializerService CreateSerializationService(SerializationFormats format)
        {
            ISerializerService service = null;
            switch(format)
            {
                case SerializationFormats.JSON:
                    service = new JSONSerializerService();
                    break;
                case SerializationFormats.XML:
                    service = new XmlSerializerService();
                    break;
            }
            if (service.IsAvailable())
            {
                return service;
            }
            throw new Exception("Unsupported serialization format: " + format);
        }

        protected SerializationFormats GetFormatFromContent(string content)
        {
            if (content.StartsWith("<"))
            {
                return SerializationFormats.XML;
            }
            return SerializationFormats.JSON;
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
