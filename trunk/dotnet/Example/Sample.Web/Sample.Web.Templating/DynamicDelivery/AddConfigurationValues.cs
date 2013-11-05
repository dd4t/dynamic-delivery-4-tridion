using System;
using System.Collections.Generic;
using Tridion.ContentManager.Templating.Assembly;
using TCM = Tridion.ContentManager.CommunicationManagement;
using Sample.Web.Templating.Base.DynamicDelivery;
using DD4T.ContentModel;
using DD4T.Templates.Base.Utils;

namespace Sample.Web.Templating.DynamicDelivery
{
    [TcmTemplateTitle("Add configuration values")]
    class AddConfigurationValues : ExtendiblePageTemplate
    {
        public const string StagingUrlFieldName = "StagingUrl";
        public const string GoogleAnalyticsTrackingIdFieldName = "GoogleAnalyticsUAID";

        protected override void TransformPage(Page page)
        {
            AddConfig(page, StagingUrlFieldName);
            AddConfig(page, GoogleAnalyticsTrackingIdFieldName);
        }

        /// <summary>
        /// Adds a value to the Page metadata from the Configuration Component on the Publication Metadata
        /// </summary>
        /// <param name="page">Dynamic Delivery Page</param>
        /// <param name="name">XML field name of value from Configuration Component</param>
        private void AddConfig(Page page, string name)
        {
            TCM.Publication tcmPub = (TCM.Publication)this.GetTcmPage().ContextRepository;
            string value = TridionConfigurationManager.GetInstance(Engine, tcmPub).AppSettings[name];

            Field field = new Field
            {
                FieldType = FieldType.Text,
                Name = name,
                Values = new List<string>() { value }
            };

            page.MetadataFields.Add(name, field);
            Logger.Info(string.Format("added field: {0} with value: {1} to Page Metadata", name, value));
        }
    }
}
