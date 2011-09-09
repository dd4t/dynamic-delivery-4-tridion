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
	public class PageTemplateBuilder {
        public static Dynamic.PageTemplate BuildPageTemplate(TComm.PageTemplate tcmPageTemplate, BuildManager manager)
        {
			Dynamic.PageTemplate pt = new Dynamic.PageTemplate();
			pt.Title = tcmPageTemplate.Title;
			pt.Id = tcmPageTemplate.Id.ToString();
			pt.FileExtension = tcmPageTemplate.FileExtension;

         if (tcmPageTemplate.Metadata != null && tcmPageTemplate.MetadataSchema != null)
         {
            pt.MetadataFields = new Dynamic.Fields();
            TCM.Fields.ItemFields tcmMetadataFields = new TCM.Fields.ItemFields(tcmPageTemplate.Metadata, tcmPageTemplate.MetadataSchema);
            pt.MetadataFields = manager.BuildFields(tcmMetadataFields, 0, false); // never follow links to comopnents from page templates, never resolve binary widht/height
         }
         else
         {
            pt.MetadataFields = null;
         }
         pt.Publication = manager.BuildPublication(tcmPageTemplate.ContextRepository);
         pt.Folder = manager.BuildOrganizationalItem((TCM.Folder)tcmPageTemplate.OrganizationalItem);
         return pt;
		}
	}
}
