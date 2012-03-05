using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Threading;
using DD4T.Extensions.DynamicDelivery.ContentModel.Factories;

namespace DD4T.Extensions.DynamicDelivery.Mvc.Caching
{
    public class SitemapCacheDependency : CacheDependency
    {
        private Timer timer;

        public DateTime LastPublishDate { get; private set; }

        public string SitemapUrlPath { get; private set; }

        public SitemapCacheDependency(int pollTime, string sitemapUrlPath)
        {
            timer = new Timer(
                new TimerCallback(CheckDependencyCallback),
                this, 0, pollTime);
            SitemapUrlPath = sitemapUrlPath;
            IPageFactory pageFactory = FactoryService.PageFactory;
            LastPublishDate = pageFactory.GetLastPublishedDateByUrl(SitemapUrlPath);
        }

        private void CheckDependencyCallback(object sender)
        {
            IPageFactory pageFactory = FactoryService.PageFactory;
            DateTime lastPublishedDate = pageFactory.GetLastPublishedDateByUrl(SitemapUrlPath);
            if (lastPublishedDate > LastPublishDate)
            {
                base.NotifyDependencyChanged(this, EventArgs.Empty);
                timer.Dispose();
            }
        }

        protected override void DependencyDispose()
        {
            timer.Dispose();
            base.DependencyDispose();
        }

    }
}
