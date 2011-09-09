///   
/// Copyright 2011 Capgemini & SDL
///
///   Licensed under the Apache License, Version 2.0 (the "License");
///   you may not use this file except in compliance with the License.
///   You may obtain a copy of the License at
///
///       http://www.apache.org/licenses/LICENSE-2.0
///
///   Unless required by applicable law or agreed to in writing, software
///   distributed under the License is distributed on an "AS IS" BASIS,
///   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
///   See the License for the specific language governing permissions and
///   limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = Tridion.Extensions.DynamicDelivery.ContentModel;
using TCM = Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using System.Xml.Serialization;
using System.IO;
using Tridion.Extensions.DynamicDelivery.Templates.Utils;

namespace Tridion.Extensions.DynamicDelivery.Templates.Builder
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
         }
         else
         {
            TextReader tr = new StringReader(renderedContent);
            if (serializer == null)
            {
               serializer = new Microsoft.Xml.Serialization.GeneratedAssembly.ComponentPresentationBuilderSerializer();
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
