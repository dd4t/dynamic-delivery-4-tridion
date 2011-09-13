using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.ContentModel;
using TComm = Tridion.ContentManager.CommunicationManagement;
using TCM = Tridion.ContentManager.ContentManagement;

namespace DD4T.Templates.Base.Builder
{
	public class ComponentTemplateBuilder {
		public static Dynamic.ComponentTemplate BuildComponentTemplate(TComm.ComponentTemplate tcmComponentTemplate, BuildManager manager) {
			Dynamic.ComponentTemplate ct = new Dynamic.ComponentTemplate();
			ct.Title = tcmComponentTemplate.Title;
			ct.Id = tcmComponentTemplate.Id.ToString();
			ct.OutputFormat = tcmComponentTemplate.OutputFormat;
         if (tcmComponentTemplate.Metadata != null && tcmComponentTemplate.MetadataSchema != null)
         {
            ct.MetadataFields = new Dynamic.SerializableDictionary<string, Dynamic.Field>();
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
