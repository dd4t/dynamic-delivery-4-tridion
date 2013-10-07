using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.ContentModel;
using TCM = Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using DD4T.ContentModel.Exceptions;
using DD4T.Templates.Base.Utils;

namespace DD4T.Templates.Base.Builder
{
    public class PageBuilder
    {
        public static Dynamic.Page BuildPage(TCM.Page tcmPage, Engine engine, BuildManager manager)
        {
            return BuildPage(tcmPage, engine, manager, 1, false,false);
        }
        public static Dynamic.Page BuildPage(TCM.Page tcmPage, Engine engine, BuildManager manager, int linkLevels, bool resolveWidthAndHeight,bool publishEmptyFields)
        {
            Dynamic.Page p = new Dynamic.Page
                                 {
                                     Title = tcmPage.Title,
                                     Id = tcmPage.Id.ToString(),
                                     Filename = tcmPage.FileName,
                                     PageTemplate = manager.BuildPageTemplate(tcmPage.PageTemplate),
                                     Schema = manager.BuildSchema(tcmPage.MetadataSchema),
                                     Version = tcmPage.Version,
                                     RevisionDate = tcmPage.RevisionDate,
                                     MetadataFields = new Dynamic.FieldSet()
                                 };
            if (linkLevels > 0)
            {
                try
                {
                    if (tcmPage.Metadata != null && tcmPage.MetadataSchema != null)
                    {
                        GeneralUtils.TimedLog("doing metadata stuff");

                        var tcmMetadataFields = new Tridion.ContentManager.ContentManagement.Fields.ItemFields(tcmPage.Metadata, tcmPage.MetadataSchema);
                        p.MetadataFields = manager.BuildFields(tcmMetadataFields, linkLevels, resolveWidthAndHeight,publishEmptyFields);
                    }
                }
                catch (Exception)
                {
                    // fail silently if there is no metadata schema
                }
            }

            p.ComponentPresentations = new List<Dynamic.ComponentPresentation>();
            foreach (TCM.ComponentPresentation cp in tcmPage.ComponentPresentations)
            {
                Dynamic.ComponentPresentation dynCp = manager.BuildComponentPresentation(cp, engine, linkLevels - 1, resolveWidthAndHeight);
                p.ComponentPresentations.Add(dynCp);
            }
            p.StructureGroup = manager.BuildOrganizationalItem((TCM.StructureGroup)tcmPage.OrganizationalItem);
            p.Publication = manager.BuildPublication(tcmPage.ContextRepository);
            p.OwningPublication = manager.BuildPublication(tcmPage.OwningRepository);
            p.Categories = manager.BuildCategories(tcmPage);

            manager.AddXpathToFields(p.MetadataFields, "Metadata");

            return p;
        }
    }
}
