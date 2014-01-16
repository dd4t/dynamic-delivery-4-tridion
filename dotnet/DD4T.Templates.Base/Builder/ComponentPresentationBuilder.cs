using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.ContentModel;
using TCM = Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using System.Xml.Serialization;
using System.IO;
using DD4T.Templates.Base.Utils;
using DD4T.Templates.Base.Serializing;

namespace DD4T.Templates.Base.Builder
{
    public class ComponentPresentationBuilder
    {

        public static Dynamic.ComponentPresentation BuildComponentPresentation(TCM.ComponentPresentation tcmComponentPresentation, Engine engine, int linkLevels, bool resolveWidthAndHeight, BuildManager manager)
        {
            Dynamic.ComponentPresentation cp = new Dynamic.ComponentPresentation();

            if (tcmComponentPresentation.ComponentTemplate.IsRepositoryPublishable)
            {
                // call render but ignore the output - render ensures componentlinking will be setup as normal.
                // don't bother with page flags because the end result is dynamically published so it needs to run with DCP settings
                engine.RenderComponentPresentation(tcmComponentPresentation.Component.Id, tcmComponentPresentation.ComponentTemplate.Id);

                // ignore the rendered CP, because it is already available in the broker
                // instead, we will render a very simple version without any links
                cp.Component = manager.BuildComponent(tcmComponentPresentation.Component, 0, false,false); // linkLevels = 0 means: only summarize the component
                cp.IsDynamic = true;
            }
            else
            {
                // render the component presentation using its own CT
                // but first, set a parameter in the context so that the CT will know it is beng called
                // from a DynamicDelivery page template
                if (engine.PublishingContext.RenderContext != null && !engine.PublishingContext.RenderContext.ContextVariables.Contains(BasePageTemplate.VariableNameCalledFromDynamicDelivery))
                {
                    engine.PublishingContext.RenderContext.ContextVariables.Add(BasePageTemplate.VariableNameCalledFromDynamicDelivery, BasePageTemplate.VariableValueCalledFromDynamicDelivery);
                }

                string renderedContent = engine.RenderComponentPresentation(tcmComponentPresentation.Component.Id, tcmComponentPresentation.ComponentTemplate.Id);
                engine.PublishingContext.RenderContext.ContextVariables.Remove(BasePageTemplate.VariableNameCalledFromDynamicDelivery);

                renderedContent = TridionUtils.StripTcdlTags(renderedContent);

                cp.IsDynamic = false;
                try
                {
                    cp.Component = (Dynamic.Component) new JSONSerializerService().Deserialize<Dynamic.Component>(renderedContent);
                }
                catch (Exception e)
                {
                    TemplatingLogger.GetLogger(typeof(ComponentPresentationBuilder)).Error("exception while deserializing into CP: " + e.Message);
                    // the component presentation could not be deserialized, this probably not a Dynamic Delivery template
                    // just store the output as 'RenderedContent' on the CP
                    cp.RenderedContent = renderedContent;
                    // because the CT was not a DD4T CT, we will generate the DD4T XML code here
                    cp.Component = manager.BuildComponent(tcmComponentPresentation.Component);
                }
            }
            cp.ComponentTemplate = manager.BuildComponentTemplate(tcmComponentPresentation.ComponentTemplate);
            return cp;
        }
    }
}
