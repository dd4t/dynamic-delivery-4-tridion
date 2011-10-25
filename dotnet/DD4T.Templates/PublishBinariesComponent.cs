using Tridion.ContentManager;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.ContentModel;
using DD4T.Templates.Base;
using DD4T.Templates.Base.Utils;
using System.Collections.Generic;

namespace DD4T.Templates
{
    [TcmTemplateTitle("Publish binaries for component")]
    [TcmTemplateParameterSchema("resource:DD4T.Templates.Resources.Schemas.Dynamic Delivery Parameters.xsd")]
    public class PublishBinariesComponent : BaseComponentTemplate
    {
        BinaryPublisher binaryPublisher;

        public PublishBinariesComponent() : base(TemplatingLogger.GetLogger(typeof(PublishBinariesComponent))) { }

        #region DynamicDeliveryTransformer Members
        protected override void TransformComponent(Dynamic.Component component)
        {
            binaryPublisher = new BinaryPublisher(Package, Engine);


            // call helper function to publish all relevant multimedia components
            // that could be:
            // - the current component
            // - any component linked to the current component
            // - any component linked to that (the number of levels is configurable in a parameter)

            PublishAllBinaries(component);
        }
        #endregion

        #region Private Members
        private void PublishAllBinaries(Dynamic.FieldSet fieldSet)
        {
            foreach (Dynamic.Field field in fieldSet.Values)
            {
                if (field.FieldType == Dynamic.FieldType.ComponentLink || field.FieldType == Dynamic.FieldType.MultiMediaLink)
                {
                    foreach (Dynamic.Component linkedComponent in field.LinkedComponentValues)
                    {
                        PublishAllBinaries(linkedComponent);
                    }
                }
                if (field.FieldType == Dynamic.FieldType.Embedded)
                {
                    foreach (Dynamic.FieldSet embeddedFields in field.EmbeddedValues)
                    {
                        PublishAllBinaries(embeddedFields);
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
            PublishAllBinaries(component.Fields);
            PublishAllBinaries(component.MetadataFields);
        }

        #endregion

    }
}