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


namespace Tridion.Extensions.DynamicDelivery.Templates.Builder {
	public class PageBuilder {
        public static Dynamic.Page BuildPage(TCM.Page tcmPage, Engine engine, BuildManager manager)
        {
			return BuildPage(tcmPage, engine, manager, 1, false);
		}
      public static Dynamic.Page BuildPage(TCM.Page tcmPage, Engine engine, BuildManager manager, int linkLevels, bool resolveWidthAndHeight)
      {
			Dynamic.Page p = new Dynamic.Page();
			p.Title = tcmPage.Title;
			p.Id = tcmPage.Id.ToString();
			p.Filename = tcmPage.FileName;
            p.Version = tcmPage.Version;
            p.PageTemplate = manager.BuildPageTemplate(tcmPage.PageTemplate);
            p.Schema = manager.BuildSchema(tcmPage.MetadataSchema);
			p.Metadata = new Dynamic.Fields();
			if (linkLevels > 0) {
				try {
					if (tcmPage.Metadata != null && tcmPage.MetadataSchema != null) {
						Tridion.ContentManager.ContentManagement.Fields.ItemFields tcmMetadataFields = new Tridion.ContentManager.ContentManagement.Fields.ItemFields(tcmPage.Metadata, tcmPage.MetadataSchema);
                        p.Metadata = manager.BuildFields(tcmMetadataFields, linkLevels, resolveWidthAndHeight);
					}
				} catch (Tridion.Extensions.DynamicDelivery.ContentModel.Exceptions.ItemDoesNotExistException) {
					// fail silently if there is no metadata schema
				}
			}

			p.ComponentPresentations = new List<Tridion.Extensions.DynamicDelivery.ContentModel.ComponentPresentation>();
			foreach (TCM.ComponentPresentation cp in tcmPage.ComponentPresentations) {
                Dynamic.ComponentPresentation dynCp = manager.BuildComponentPresentation(cp, engine, linkLevels - 1, resolveWidthAndHeight);
				p.ComponentPresentations.Add(dynCp);
			}
            p.StructureGroup = manager.BuildOrganizationalItem((TCM.StructureGroup)tcmPage.OrganizationalItem);
            p.Publication = manager.BuildPublication(tcmPage.ContextRepository);
            p.Categories = manager.BuildCategories(tcmPage);

			return p;
		}
	}
}
