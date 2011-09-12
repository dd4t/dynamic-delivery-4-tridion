using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.ContentModel;
using TCM = Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using System.Xml.Serialization;
using System.IO;
using DD4T.Templates.Utils;

namespace DD4T.Templates.Builder
{
   public class ComponentPresentationBuilder
   {
      static XmlSerializer serializer = null;

      public static Dynamic.ComponentPresentation BuildComponentPresentation(TCM.ComponentPresentation tcmComponentPresentation, Engine engine, int linkLevels, bool resolveWidthAndHeight, BuildManager manager)
      {
         Dynamic.ComponentPresentation cp = new Dynamic.ComponentPresentation();


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

         if (tcmComponentPresentation.ComponentTemplate.IsRepositoryPublishable)
         {
            // ignore the rendered CP, because it is already available in the broker
            // instead, we will render a very simple version without any links
             cp.Component = manager.BuildComponent(tcmComponentPresentation.Component, 0, false); // linkLevels = 0 means: only summarize the component
             cp.IsDynamic = true;
         }
         else
         {
             cp.IsDynamic = false;
            TextReader tr = new StringReader(renderedContent);
            if (serializer == null)
            {
               serializer = new XmlSerializerFactory().CreateSerializer(typeof(Dynamic.Component));
            }
            try
            {
               cp.Component = (Dynamic.Component)serializer.Deserialize(tr);
            }
            catch
            {
               // the component presentation could not be deserialized, this probably not a Dynamic Delivery template
               // just store the output as 'RenderedContent' on the CP
               cp.RenderedContent = renderedContent;
            }
         }
         cp.ComponentTemplate = manager.BuildComponentTemplate(tcmComponentPresentation.ComponentTemplate);
         return cp;
      }
   }
}
