using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.Extensions.DynamicDelivery.ContentModel;
using TCM = Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using DD4T.Extensions.DynamicDelivery.ContentModel.Exceptions;


namespace DD4T.Extensions.DynamicDelivery.Templates.Builder
{
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
            p.PageTemplate = manager.BuildPageTemplate(tcmPage.PageTemplate);
            p.Schema = manager.BuildSchema(tcmPage.MetadataSchema);
			p.Metadata = new Dynamic.SerializableDictionary<string, Dynamic.Field>();
			if (linkLevels > 0) {
				try {
					if (tcmPage.Metadata != null) {
						var tcmMetadataFields = new Tridion.ContentManager.ContentManagement.Fields.ItemFields(tcmPage.Metadata, tcmPage.MetadataSchema);
                        p.Metadata = manager.BuildFields(tcmMetadataFields, linkLevels, resolveWidthAndHeight);
					}
				} catch (ItemDoesNotExistException) {
					// fail silently if there is no metadata schema
				}
			}

			p.ComponentPresentations = new List<Dynamic.ComponentPresentation>();
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
