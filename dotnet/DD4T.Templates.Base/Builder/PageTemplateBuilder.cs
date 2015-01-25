﻿using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.ContentModel;
using TComm = Tridion.ContentManager.CommunicationManagement;
using TCM = Tridion.ContentManager.ContentManagement;

namespace DD4T.Templates.Base.Builder
{
	public class PageTemplateBuilder {
        public static Dynamic.PageTemplate BuildPageTemplate(TComm.PageTemplate tcmPageTemplate, BuildManager manager)
        {
			Dynamic.PageTemplate pt = new Dynamic.PageTemplate();
			pt.Title = tcmPageTemplate.Title;
			pt.Id = tcmPageTemplate.Id.ToString();
			pt.FileExtension = tcmPageTemplate.FileExtension;
            pt.RevisionDate = tcmPageTemplate.RevisionDate;

         if (tcmPageTemplate.Metadata != null && tcmPageTemplate.MetadataSchema != null)
         {
             pt.MetadataFields = new Dynamic.FieldSet();
            TCM.Fields.ItemFields tcmMetadataFields = new TCM.Fields.ItemFields(tcmPageTemplate.Metadata, tcmPageTemplate.MetadataSchema);
            pt.MetadataFields = manager.BuildFields(tcmMetadataFields, 0, false,false); // never follow links to components from page templates, never resolve binary widht/height
         }
         else
         {
            pt.MetadataFields = null;
         }

         if (!manager.BuildProperties.OmitContextPublications)
             pt.Publication = manager.BuildPublication(tcmPageTemplate.ContextRepository);

         if (!manager.BuildProperties.OmitOwningPublications)
             pt.OwningPublication = manager.BuildPublication(tcmPageTemplate.OwningRepository);

         if (!manager.BuildProperties.OmitFolders)
             pt.Folder = manager.BuildOrganizationalItem((TCM.Folder)tcmPageTemplate.OrganizationalItem);
         
         return pt;
		}
	}
}
