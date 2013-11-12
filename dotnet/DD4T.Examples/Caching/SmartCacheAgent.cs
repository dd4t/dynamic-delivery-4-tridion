using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Configuration;
using System.Timers;
using DD4T.ContentModel.Contracts.Caching;
using DD4T.Factories.Caching;
using DD4T.Utils;

namespace DD4T.Examples.Caching
{
    public enum CacheRegions { Page, PageCustom, Component, ComponentCustom, View, System, Binary, Shop }
    public class SmartCacheAgent : ICacheAgent
    {

        public const int NoExpirationValue = 0;
        public const string DefaultRegion = "Default";
        public const int DefaultExpirationValue = 180;

        #region properties
        private static ObjectCache Cache
        {
            get
            {
                return MemoryCache.Default;
            }
        }

        private static int CheckPublishDateOffSet
        {
            get
            {
                return 10; // todo: make configurable
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
            // if there is no call back, we can do nothing but return the item
            if (this.GetLastPublishDateCallBack == null)
                return FindOrRemove(key);


            //DateTime? insertionDateScheduledCallBack = Cache[string.Format("{0}_S", key)] as DateTime?;
            //if (insertionDateScheduledCallBack != null)
            //{
            //    // do not call back now, it is already scheduled to happen!
            //    return FindOrRemove(key);
            //}

            object objectToReturn = FindOrRemove(key);
            if (objectToReturn == null)
            {
                return null;
            }
            DateTime? insertionDateDirectCallBack = Cache[string.Format("{0}_D", key)] as DateTime?;
            if (insertionDateDirectCallBack == null)
                return objectToReturn;

            DateTime lastPublishDate = GetLastPublishDateCallBack(key, objectToReturn);

            if (insertionDateDirectCallBack.Value < lastPublishDate)
            {
                LoggerService.Debug("removing item with key '{0}' from cache because it has been re/unpublished", key);
                Cache.Remove(key);
                Cache.Remove(string.Format("{0}_D", key));
                return null;
            }

            return Cache[key];
        }


        /// <summary>
        /// Store any object in the cache 
        /// </summary>
        /// <param name="key">Identification of the item</param>
        /// <param name="item">The object to store (can be a page, component, schema, etc) </param>
        /// <remarks>Since no region was specified, the region 'Default' is used. Absolute expiration can be 
        /// configured  by adding an appSetting to the config with key 'CacheSettings_REGION'. As value, enter 
        /// the number of seconds you would like the item to stay in the cache.</remarks>
        public void Store(string key, object item)
        {
            _Store(key, DefaultRegion, item, null);
        }

        /// <summary>
        /// Store any object in the cache with a dependency on other items in the cache
        /// </summary>
        /// <param name="key">Identification of the item</param>
        /// <param name="item">The object to store (can be a page, component, schema, etc) </param>
        /// <param name="dependOnItems">List of items on which the current item depends</param>
        /// <remarks>Since no region was specified, the region 'Default' is used. Absolute expiration can be 
        /// configured  by adding an appSetting to the config with key 'CacheSettings_REGION'. As value, enter 
        /// the number of seconds you would like the item to stay in the cache.</remarks>
        public void Store(string key, object item, List<string> dependOnItems)
        {
            _Store(key, DefaultRegion, item, dependOnItems);        }

        /// <summary>
        /// Store an object belonging to a specific region in the cache 
        /// </summary>
        /// <param name="key">Identification of the item</param>
        /// <param name="region">Identification of the region</param>
        /// <param name="item">The object to store (can be a page, component, schema, etc) </param>
        /// <remarks>Optionally, an absolute expiration time can be configured by adding an appSetting to the config with key 'CacheSettings_REGION' 
        /// (replace 'REGION' with the name of the region). As value, enter the number of seconds you 
        /// would like the item to stay in the cache. If this key does not exist, the item will not 
        /// expire at a fixed time. It may still be removed from the cache for other reasons.
        /// </remarks>
        public void Store(string key, string region, object item)
        {
            _Store(key, region, item, null);
        }

        /// <summary>
        /// Store an object belonging to a specific region in the cache with a dependency on other items in the cache.
        /// </summary>
        /// <param name="key">Identification of the item</param>
        /// <param name="region">Identification of the region</param>
        /// <param name="item">The object to store (can be a page, component, schema, etc) </param>
        /// <param name="dependOnItems">List of items on which the current item depends</param>
        /// <remarks>Optionally, an absolute expiration time can be configured by adding an appSetting to the config with key 'CacheSettings_REGION' 
        /// (replace 'REGION' with the name of the region). As value, enter the number of seconds you 
        /// would like the item to stay in the cache. If this key does not exist, the item will not 
        /// expire at a fixed time. It may still be removed from the cache for other reasons.
        /// </remarks>
        public void Store(string key, string region, object item, List<string> dependOnItems)
        {
            _Store(key, region, item, dependOnItems);
        }


        public GetLastPublishDate GetLastPublishDateCallBack { get; set; }

        #endregion


        #region private

        private void _Store(string key, string region, object item, List<string> dependOnItems)
        {
            // set up policy for the item being cached
            CacheItemPolicy policy = new CacheItemPolicy();

            // find the expiration for the current region (looked up in Web.config)
            int expirationInSeconds = GetExpirationForRegion(region);

            // if the expiration is less than zero, the item must not be cached at all
            if (expirationInSeconds < 0)
                return;

            // set up policy and store item in the cache
            policy.Priority = CacheItemPriority.Default;
            if (dependOnItems != null && dependOnItems.Count > 0)
            {
                bool goahead = true;
                foreach (string dep in dependOnItems)
                {
                    if (!Cache.Contains(dep))
                    {
                        LoggerService.Warning("trying to store item (key = {0}) in cache with a dependency on another key ({1}) which does not currently exist in the cache. The item will be cached without any dependencies", key, dep);
                        goahead = false;
                        break;
                    }
                }
                if (goahead)
                    policy.ChangeMonitors.Add(MemoryCache.Default.CreateCacheEntryChangeMonitor(dependOnItems));
            }
            policy.AbsoluteExpiration = expirationInSeconds == NoExpirationValue ? ObjectCache.InfiniteAbsoluteExpiration : DateTimeOffset.Now.AddSeconds(expirationInSeconds);
            Cache.Add(key, item, policy);


            // a number of seconds BEFORE the expiration occurs, we will check if the item is still valid
            // we call this number the 'check publish date offset'
            int checkPublishDateOffSet = CheckPublishDateOffSet;
            if (expirationInSeconds > NoExpirationValue + checkPublishDateOffSet && GetLastPublishDateCallBack != null)
            {
                // schedule callback 10 seconds before expiration
                policy.ChangeMonitors.Add(new CheckPublishDateChangeMonitor(key, item, GetLastPublishDateCallBack, expirationInSeconds - checkPublishDateOffSet));

                //// add marker to cache so that the call back will not be called every time the item is loaded from the cache
                //bool result = Cache.Add(string.Format("{0}_S", key), new DateTime?(GetLastPublishDateCallBack(key,item)), ObjectCache.InfiniteAbsoluteExpiration);
                LoggerService.Debug("created monitor to check publish date in the background for key {0}", key);
            }
            else if (GetLastPublishDateCallBack != null) // there is a callback, but no expiration, so treat this as 'direct callback' (called for every load)
            {
                bool result = Cache.Add(string.Format("{0}_D", key), new DateTime?(GetLastPublishDateCallBack(key, item)), ObjectCache.InfiniteAbsoluteExpiration);
                LoggerService.Debug("added insertion date to cache for key {0} with result {1}", key, result);
            }

        }

        private object FindOrRemove(string key)
        {
            object objectToReturn = Cache[key];
            if (objectToReturn == null)
            {
                object d = Cache[string.Format("{0}_D", key)];
                if (d != null)
                    Cache.Remove(string.Format("{0}_D", key));
                object s = Cache[string.Format("{0}_S", key)];
                if (s != null)
                    Cache.Remove(string.Format("{0}_S", key));
            }
            return objectToReturn;
        }
        private int GetExpirationForRegion(string region)
        {
            int expirationInSeconds = DefaultExpirationValue;
            string expirationSetting = ConfigurationManager.AppSettings["CacheSettings_" + region];
            if (string.IsNullOrEmpty(expirationSetting))
                expirationSetting = ConfigurationManager.AppSettings["CacheSettings_" + DefaultRegion];
            if (!int.TryParse(expirationSetting, out expirationInSeconds))
                LoggerService.Warning("incorrect expiration setting in key 'CacheSettings_{0}' or 'CacheSettings_{1}: {2}. Please modify Web.config", region, DefaultRegion, expirationSetting);
            return expirationInSeconds; 
        }

        //private CacheItemPolicy FindCacheItemPolicy(string key, object item, string region, List<string> dependOnItems)
        //{
        //    CacheItemPolicy policy = new CacheItemPolicy();
        //    policy.Priority = CacheItemPriority.Default;
        //    if (dependOnItems != null && dependOnItems.Count > 0)
        //        policy.ChangeMonitors.Add(MemoryCache.Default.CreateCacheEntryChangeMonitor(dependOnItems));

        //    string expirationSetting = null;
        //    int expirationInSeconds = NoExpirationValue;
        //    if (!string.IsNullOrEmpty(region))
        //    {
        //        expirationSetting = ConfigurationManager.AppSettings["CacheSettings_" + region];
        //    }
        //    if (!string.IsNullOrEmpty(expirationSetting))
        //    {
        //        if (!int.TryParse(expirationSetting, out expirationInSeconds))
        //            LoggerService.Warning("incorrect expiration setting in key 'CacheSettings_{0}': {1}. Please modify Web.config", region, expirationSetting);
        //    }
        //    policy.AbsoluteExpiration = expirationInSeconds == NoExpirationValue ? ObjectCache.InfiniteAbsoluteExpiration : DateTimeOffset.Now.AddSeconds(expirationInSeconds);
        //    return policy;
        //}

        //private void AddToCache(string key, object item, CacheItemPolicy policy)
        //{
        //    Cache.Add(key, item, policy);
        //    // in case the item is set not to expire, we must implement 'smart' logic 
        //    // to check if an item has been republished
        //    // For this logic, we need to know how often an item was requested.
        //    //if (policy.AbsoluteExpiration == ObjectCache.InfiniteAbsoluteExpiration && GetLastPublishDateCallBack != null)
        //    //{
        //    //    Cache.Add(GetHitCounterKey(key), 0, policy);
        //    //    Cache.Add(GetInsertionDateKey(key), GetLastPublishDateCallBack(key, item), policy);
        //    //}
        //}
        private string GetHitCounterKey(string key)
        {
            return string.Format("{0}_HitCounter", key);
        }
        private string GetInsertionDateKey(string key)
        {
            return string.Format("{0}_InsertionDate", key);
        }
        #endregion
    }

    /// <summary>
    /// Implementation of ChangeMonitor (part of System.Runtime.Caching API introduced in .NET 4).
    /// Monitors the publication date of an item, and drops it from the cache if the item has been republished. The actual logic to look up the publish date is delegated to the class that instantiates this monitor (e.g. the DefaultCacheAgent, which in turn gets the delegate from the factory).
    /// </summary>
    public class CheckPublishDateChangeMonitor : ChangeMonitor
    {

        private string _key;
        private object _cachedItem;
        private DateTime _lastPublished;
        private Timer _timer;
        private GetLastPublishDate _getLastPublishDateCallBack;

        public CheckPublishDateChangeMonitor(string key, object cachedItem, GetLastPublishDate getLastPublishDateCallBack, int interval)
        {
            
            LoggerService.Debug(">>CheckPublishDateChangeMonitor({0}, {1})", key, cachedItem.ToString());

            this._key = key;
            this._cachedItem = cachedItem;
            _getLastPublishDateCallBack = getLastPublishDateCallBack;

            bool initializationComplete = false;
            try
            {
                _lastPublished = _getLastPublishDateCallBack(key, cachedItem);

                _timer = new Timer();
                _timer.Interval = interval * 1000; // interval is configured in seconds but must be set in milliseconds!
                _timer.Elapsed += new ElapsedEventHandler(CheckForChanges);
                _timer.Enabled = true;
                _timer.Start();
                initializationComplete = true;
            }
            finally
            {
                base.InitializationComplete();
                if (!initializationComplete)
                {
                    Dispose(true);
                }
            }
            base.InitializationComplete();
        }



        /// <summary> 
        ///	Check if underlying item has been republished/unpublished.
        /// </summary> 
        /// <returns> 
        ///	Returns true if the data is republished otherwise false 
        /// </returns> 
        public void CheckForChanges(object sender, System.Timers.ElapsedEventArgs args)
        {
            DateTime lastPublishedNow = _getLastPublishDateCallBack(_key, _cachedItem);
            LoggerService.Debug("checking for changes (scheduled callback) to item with key {0}, original last publish date {1}", _key, _lastPublished);
            if (_lastPublished.CompareTo(lastPublishedNow) < 0)
            {
                LoggerService.Debug("item was (re/un)published since insertion, removing it from the cache (key = {0}, last published now {1})", _key, lastPublishedNow);

                // stop the timer, otherwise the check will continue
                _timer.Stop();
                _timer.Dispose();

                // one of the items has been republished in Tridion
                base.OnChanged(null);
            }
        }

        #region ChangeMonitor members
        protected override void Dispose(bool disposing)
        {
            _cachedItem = null;
            _key = null;
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        public override string UniqueId
        {
            get { return new Guid().ToString(); }
        }
        #endregion
    }
}


