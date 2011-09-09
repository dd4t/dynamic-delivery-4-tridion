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
using TComm = Tridion.ContentManager.CommunicationManagement;
using TCM = Tridion.ContentManager.ContentManagement;

namespace Tridion.Extensions.DynamicDelivery.Templates.Builder {
	public class ComponentTemplateBuilder {
		public static Dynamic.ComponentTemplate BuildComponentTemplate(TComm.ComponentTemplate tcmComponentTemplate, BuildManager manager) {
			Dynamic.ComponentTemplate ct = new Dynamic.ComponentTemplate();
			ct.Title = tcmComponentTemplate.Title;
			ct.Id = tcmComponentTemplate.Id.ToString();
			ct.OutputFormat = tcmComponentTemplate.OutputFormat;
         if (tcmComponentTemplate.Metadata != null && tcmComponentTemplate.MetadataSchema != null)
         {
            ct.MetadataFields = new Dynamic.Fields();
            TCM.Fields.ItemFields tcmMetadataFields = new TCM.Fields.ItemFields(tcmComponentTemplate.Metadata, tcmComponentTemplate.MetadataSchema);
            ct.MetadataFields = manager.BuildFields(tcmMetadataFields, 0, false); // never follow links to comopnents from component templates, never resolve binary widht/height
         }
         else
         {
            ct.MetadataFields = null;
         }
         ct.Folder = manager.BuildOrganizationalItem((TCM.Folder)tcmComponentTemplate.OrganizationalItem);
         ct.Publication = manager.BuildPublication(tcmComponentTemplate.ContextRepository);
			return ct;
		}
	}
}
