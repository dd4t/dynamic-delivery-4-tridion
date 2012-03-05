using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.Extensions.DynamicDelivery.ContentModel;
using TComm = Tridion.ContentManager.CommunicationManagement;
using TCM = Tridion.ContentManager.ContentManagement;

namespace DD4T.Extensions.DynamicDelivery.Templates.Builder
{
	public class PageTemplateBuilder {
        public static Dynamic.PageTemplate BuildPageTemplate(TComm.PageTemplate tcmPageTemplate, BuildManager manager)
        {
			Dynamic.PageTemplate pt = new Dynamic.PageTemplate();
			pt.Title = tcmPageTemplate.Title;
			pt.Id = tcmPageTemplate.Id.ToString();
			pt.FileExtension = tcmPageTemplate.FileExtension;

         if (tcmPageTemplate.Metadata != null && tcmPageTemplate.MetadataSchema != null)
         {
            pt.MetadataFields = new Dynamic.SerializableDictionary<string, Dynamic.Field>();
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
