using Tridion.ContentManager;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.ContentModel;

namespace DD4T.Templates
{
    [TcmTemplateTitle("Publish binaries for page")]
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
            // that could be:
            // - the current page
            // - any component on the current page
            // - any component linked to a component on the current page
            // - any component linked to that (the number of levels is configurable in a parameter)

            PublishAllBinaries(page);
        }
        #endregion

        #region Private Members
        private void PublishAllBinaries(Dynamic.Page page)
        {

            foreach (Dynamic.Field field in page.Metadata.Values)
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

            // do not publish binaries in the components, because they should be handled
            // by the component templates!

            //foreach (Dynamic.ComponentPresentation cp in page.ComponentPresentations)
            //{
            //   Dynamic.Component component = cp.Component;
            //   if (component.ComponentType.Equals(Dynamic.ComponentType.Multimedia))
            //   {
            //      component.Multimedia.Url = binaryPublisher.PublishMultimediaComponent(component.Id);
            //   }
            //   foreach (Dynamic.Field field in component.Fields.Values)
            //   {
            //      if (field.FieldType == Dynamic.FieldType.ComponentLink || field.FieldType == Dynamic.FieldType.MultiMediaLink)
            //      {
            //         foreach (Dynamic.Component linkedComponent in field.LinkedComponentValues)
            //         {
            //            PublishAllBinaries(linkedComponent);
            //         }
            //      }
            //   }
            //}
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
                    foreach (var linkedComponent in field.LinkedComponentValues)
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