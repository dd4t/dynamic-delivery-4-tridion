using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel.Contracts.Caching;
using System.Runtime.Caching;
using DD4T.ContentModel;
using DD4T.ContentModel.Factories;
using System.Configuration;


namespace DD4T.Factories.Caching
{
    /// <summary>
    /// Default implementation of ICacheAgent, as used by the factories in DD4T.Factories. It uses the System.Runtime.Caching API introduced in .NET 4. This will run in a web environment as well as a windows service, console application or any other type of environment.
    /// If you are unable to run .NET 4, you can use the WebCacheAgent which is part of DD4T.Mvc.
    /// </summary>
    public class DefaultCacheAgent : ICacheAgent
    {
        public const int DefaultExpirationInSeconds = 60;

        #region properties
        private static ObjectCache Cache
        {
            get
            {
                return MemoryCache.Default;
            }
        }

        #endregion properties

        #region ICacheAgent

        /// <summary>
        /// Load object from the cache
        /// </summary>
        /// <param name="key">Identification of the object</param>
        /// <returns></returns>
        public object Load(string key)
        {
            return Cache[key];
        }


        /// <summary>
        /// Store any object in the cache 
        /// </summary>
        /// <param name="key">Identification of the item</param>
        /// <param name="item">The object to store (can be a page, component, schema, etc) </param>
        public void Store(string key, object item)
        {
            Cache.Add(key, item, FindCacheItemPolicy(key, item, null, null));
        }

        /// <summary>
        /// Store any object in the cache with a dependency on other items in the cache
        /// </summary>
        /// <param name="key">Identification of the item</param>
        /// <param name="item">The object to store (can be a page, component, schema, etc) </param>
        /// <param name="dependOnItems">List of items on which the current item depends</param>
        public void Store(string key, object item, List<string> dependOnItems)
        {
            Cache.Add(key, item, FindCacheItemPolicy(key, item, null, dependOnItems));
        }

        /// <summary>
        /// Store an object belonging to a specific region in the cache 
        /// </summary>
        /// <param name="key">Identification of the item</param>
        /// <param name="region">Identification of the region</param>
        /// <param name="item">The object to store (can be a page, component, schema, etc) </param>
        /// <remarks>The expiration time can be configured by adding an appSetting to the config with key 'CacheSettings_REGION' 
        /// (replace 'REGION' with the name of the region). If this key does not exist, the key 'CacheSettings_Default' will be used.
        /// </remarks>
        public void Store(string key, string region, object item)
        {
            Cache.Add(key, item, FindCacheItemPolicy(key, item, region, null));
        }

        /// <summary>
        /// Store an object belonging to a specific region in the cache with a dependency on other items in the cache.
        /// </summary>
        /// <param name="key">Identification of the item</param>
        /// <param name="region">Identification of the region</param>
        /// <param name="item">The object to store (can be a page, component, schema, etc) </param>
        /// <param name="dependOnItems">List of items on which the current item depends</param>
        /// <remarks>The expiration time can be configured by adding an appSetting to the config with key 'CacheSettings_REGION' 
        /// (replace 'REGION' with the name of the region). If this key does not exist, the key 'CacheSettings_Default' will be used.
        /// </remarks>
        public void Store(string key, string region, object item, List<string> dependOnItems)
        {
            Cache.Add(key, item, FindCacheItemPolicy(key, item, region, dependOnItems));
        }


        public GetLastPublishDate GetLastPublishDateCallBack { get; set; }

        #endregion

        #region private
        private CacheItemPolicy FindCacheItemPolicy(string key, object item, string region, List<string> dependOnItems)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.Default;

            if (GetLastPublishDateCallBack != null)
                policy.ChangeMonitors.Add (new LastPublishDateChangeMonitor(key, item, GetLastPublishDateCallBack));

            if (dependOnItems != null && dependOnItems.Count > 0)
            {
                policy.ChangeMonitors.Add(Cache.CreateCacheEntryChangeMonitor(dependOnItems));
            }

            if (!string.IsNullOrEmpty(region))
            {
                int expirationInSeconds = DefaultExpirationInSeconds;
                // if a region is specified, check for specific expiration settings, or else use default expiration
                string setting = ConfigurationManager.AppSettings["CacheSettings_" + region];
                if (string.IsNullOrEmpty(setting))
                {
                    setting = ConfigurationManager.AppSettings["CacheSettings_Default"];
                }
                if (string.IsNullOrEmpty(setting))
                {
                    try
                    {
                        expirationInSeconds = Convert.ToInt32(setting);
                    }
                    catch
                    {
                        // if the value is not a proper number, we will use the default set in the code automatically
                    }
                }
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(expirationInSeconds); 
            }
            return policy;

        }
        #endregion

    }
}
