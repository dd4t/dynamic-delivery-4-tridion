using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using DD4T.ContentModel;
using System.Timers;
using DD4T.ContentModel.Factories;
using DD4T.ContentModel.Contracts.Caching;
using System.Configuration;
using DD4T.Utils;

namespace DD4T.Factories.Caching
{
    /// <summary>
    /// Implementation of ChangeMonitor (part of System.Runtime.Caching API introduced in .NET 4).
    /// Monitors the publication date of an item, and drops it from the cache if the item has been republished. The actual logic to look up the publish date is delegated to the class that instantiates this monitor (e.g. the DefaultCacheAgent, which in turn gets the delegate from the factory).
    /// </summary>
    public class LastPublishDateChangeMonitor : ChangeMonitor
    {
        public const int DefaultCallBackInterval = 60; // configured in seconds

        private string _key;
        private object _cachedItem;
        private DateTime _lastPublished;
        private Timer _timer;
        private GetLastPublishDate _getLastPublishDateCallBack;

        public LastPublishDateChangeMonitor(string key, object cachedItem, GetLastPublishDate getLastPublishDateCallBack)
        {
            LoggerService.Debug(">>LastPublishDateChangeMonitor({0}, {1})", key, cachedItem.ToString());

            this._key = key;
            this._cachedItem = cachedItem;
            _getLastPublishDateCallBack = getLastPublishDateCallBack;

            bool initializationComplete = false;
            try
            {
                _lastPublished = _getLastPublishDateCallBack(key, cachedItem);

                _timer = new Timer();

                int interval = DefaultCallBackInterval;
                string configuredInterval = ConfigurationHelper.GetSetting("CacheSettings_CallBackInterval");
                if (! string.IsNullOrEmpty(configuredInterval))
                {
                    try
                    {
                        interval = Convert.ToInt32(configuredInterval);
                    }
                    catch
                    {
                        // fall back to default setting
                    }
                }

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
            if (_lastPublished.CompareTo(lastPublishedNow) < 0)
            {

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
