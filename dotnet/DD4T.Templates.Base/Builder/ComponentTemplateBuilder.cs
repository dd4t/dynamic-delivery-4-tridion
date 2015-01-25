﻿using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.ContentModel;
using TComm = Tridion.ContentManager.CommunicationManagement;
using TCM = Tridion.ContentManager.ContentManagement;

namespace DD4T.Templates.Base.Builder
{
	public class ComponentTemplateBuilder {
        public static Dynamic.ComponentTemplate BuildComponentTemplate(TComm.ComponentTemplate tcmComponentTemplate, BuildManager manager)
        {
            Dynamic.ComponentTemplate ct = new Dynamic.ComponentTemplate();
            ct.Title = tcmComponentTemplate.Title;
            ct.Id = tcmComponentTemplate.Id.ToString();
            ct.OutputFormat = tcmComponentTemplate.OutputFormat;
            ct.RevisionDate = tcmComponentTemplate.RevisionDate;
            if (tcmComponentTemplate.Metadata != null && tcmComponentTemplate.MetadataSchema != null)
            {
                ct.MetadataFields = new Dynamic.FieldSet();
                TCM.Fields.ItemFields tcmMetadataFields = new TCM.Fields.ItemFields(tcmComponentTemplate.Metadata, tcmComponentTemplate.MetadataSchema);
                ct.MetadataFields = manager.BuildFields(tcmMetadataFields, 0, false, false); // never follow links to components from component templates, never resolve binary widht/height
            }
            else
            {
                ct.MetadataFields = null;
            }


            if (!manager.BuildProperties.OmitContextPublications)
                ct.Publication = manager.BuildPublication(tcmComponentTemplate.ContextRepository);

            if (!manager.BuildProperties.OmitOwningPublications)
                ct.OwningPublication = manager.BuildPublication(tcmComponentTemplate.OwningRepository);

            if (!manager.BuildProperties.OmitFolders)
                ct.Folder = manager.BuildOrganizationalItem((TCM.Folder)tcmComponentTemplate.OrganizationalItem);

            return ct;
        }
	}
}
