using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Threading;
using DD4T.Extensions.DynamicDelivery.ContentModel.Factories;

namespace DD4T.Extensions.DynamicDelivery.Mvc.Caching
{
    public class SchemaComponentsCacheDependency : CacheDependency
    {
        private Timer timer;

        public string[] SchemaUris { get; private set; }

        public DateTime LastPublishDate { get; private set; }

        public SchemaComponentsCacheDependency(int pollTime, string[] schemaUris)
        {
            timer = new Timer(
                new TimerCallback(CheckDependencyCallback),
                this, 0, pollTime);
            SchemaUris = schemaUris;
            IComponentFactory componentFactory = FactoryService.ComponentFactory;
            LastPublishDate = componentFactory.LastPublished(schemaUris);
        }

        private void CheckDependencyCallback(object sender)
        {
            IComponentFactory componentFactory = FactoryService.ComponentFactory;
            DateTime lastPublishedDate = componentFactory.LastPublished(SchemaUris);
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
