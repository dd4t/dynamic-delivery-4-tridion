using Tridion.ContentManager;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.ContentModel;
using DD4T.Templates.Base;
using DD4T.Templates.Base.Utils;
using DD4T.ContentModel;

namespace DD4T.Templates
{
    [TcmTemplateTitle("Publish binaries for page")]
    [TcmTemplateParameterSchema("resource:DD4T.Templates.Resources.Schemas.Dynamic Delivery Parameters.xsd")]
    public class PublishBinariesPage : BasePageTemplate
    {
        BinaryPublisher binaryPublisher;

        public PublishBinariesPage() : base(TemplatingLogger.GetLogger(typeof(PublishBinariesPage))) { }
        public PublishBinariesPage(TemplatingLogger log) : base(log) { }

        #region DynamicDeliveryTransformer Members
        protected override void TransformPage(Dynamic.Page page)
        {
            binaryPublisher = new BinaryPublisher(Package, Engine);


            // call helper function to publish all relevant multimedia components
            // Note: this template only published mm components that are found in the metadata of the page
            // MM components that are used as component presentation, or that are linked from a component that is 
            // used as a component presentation, will be published from the component template
            // (e.g. by adding 'Publish binaries for components' to that CT)

            PublishAllBinaries(page);
        }
        #endregion

        #region Private Members
        private void PublishAllBinaries(Dynamic.Page page)
        {

            GeneralUtils.TimedLog("page: " + page);
            GeneralUtils.TimedLog("page metadata fields property: " + (page.MetadataFields == null ? "null" : page.MetadataFields.ToString()));
            GeneralUtils.TimedLog("page metadata fields count: " + page.MetadataFields.Count);
            foreach (Dynamic.Field field in page.MetadataFields.Values)
            {
                if (field.FieldType == Dynamic.FieldType.ComponentLink || field.FieldType == Dynamic.FieldType.MultiMediaLink)
                {
                    foreach (Dynamic.Component linkedComponent in field.LinkedComponentValues)
                    {
                        PublishAllBinaries(linkedComponent);
                    }
                }
                if (field.FieldType == Dynamic.FieldType.Xhtml)
                {
                    for (int i = 0; i < field.Values.Count; i++)
                    {
                        string xhtml = field.Values[i];
                        field.Values[i] = binaryPublisher.PublishBinariesInRichTextField(xhtml);
                    }
                }
            }

        }

        private void PublishAllBinaries(Dynamic.Component component)
        {
            if (component.ComponentType.Equals(Dynamic.ComponentType.Multimedia))
            {
                component.Multimedia.Url = binaryPublisher.PublishMultimediaComponent(component.Id);
            }
            foreach (var field in component.Fields.Values)
            {
                if (field.FieldType == Dynamic.FieldType.ComponentLink || field.FieldType == Dynamic.FieldType.MultiMediaLink)
                {
                    foreach (IComponent linkedComponent in field.LinkedComponentValues)
                    {
                        PublishAllBinaries(linkedComponent as Component);
                    }
                }
                if (field.FieldType == Dynamic.FieldType.Xhtml)
                {
                    for (int i = 0; i < field.Values.Count; i++)
                    {
                        string xhtml = field.Values[i];
                        field.Values[i] = binaryPublisher.PublishBinariesInRichTextField(xhtml);
                    }
                }
            }
            foreach (var field in component.MetadataFields.Values)
            {
                if (field.FieldType == Dynamic.FieldType.ComponentLink || field.FieldType == Dynamic.FieldType.MultiMediaLink)
                {
                    foreach (Dynamic.Component linkedComponent in field.LinkedComponentValues)
                    {
                        PublishAllBinaries(linkedComponent);
                    }
                }
                if (field.FieldType == Dynamic.FieldType.Xhtml)
                {
                    for (int i = 0; i < field.Values.Count; i++)
                    {
                        string xhtml = field.Values[i];
                        field.Values[i] = binaryPublisher.PublishBinariesInRichTextField(xhtml);
                    }
                }
            }
        }

        #endregion

    }
}