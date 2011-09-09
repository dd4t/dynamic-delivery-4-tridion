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

using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Publishing.Rendering;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.Logging;
using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Text;
using System.Text.RegularExpressions;
using Dynamic = Tridion.Extensions.DynamicDelivery.ContentModel;
using System.Collections.Generic;
using System.Xml.Serialization;
using Tridion.Extensions.DynamicDelivery.Templates.Utils;

namespace Tridion.Extensions.DynamicDelivery.Templates {

	[TcmTemplateTitle("Publish binaries for component")]
	public class PublishBinariesComponent : BaseComponentTemplate {

		protected new TemplatingLogger log = TemplatingLogger.GetLogger(typeof(PublishBinariesComponent));
		TcmUri targetStructureGroup = null;
      BinaryPublisher binaryPublisher;

		#region DynamicDeliveryTransformer Members
		protected override void TransformComponent(Dynamic.Component component) {
         binaryPublisher = new BinaryPublisher(package, engine);


			// call helper function to publish all relevant multimedia components
			// that could be:
			// - the current component
			// - any component linked to the current component
			// - any component linked to that (the number of levels is configurable in a parameter)

			PublishAllBinaries(component);
		}
#endregion

		#region Private Members
		private void PublishAllBinaries(Dynamic.Component component) {
			if (component.ComponentType.Equals(Dynamic.ComponentType.Multimedia)) {
				component.Multimedia.Url = binaryPublisher.PublishMultimediaComponent(component.Id);
			}
         foreach (Dynamic.Field field in component.Fields.Values)
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
         foreach (Dynamic.Field field in component.MetadataFields.Values)
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