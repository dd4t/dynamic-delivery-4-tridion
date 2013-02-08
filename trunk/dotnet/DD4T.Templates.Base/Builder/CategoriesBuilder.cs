using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.ContentModel;
using TComm = Tridion.ContentManager.CommunicationManagement;
using TCM = Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.ContentManagement;
using DD4T.Templates.Base.Utils;

namespace DD4T.Templates.Base.Builder
{
    public class CategoriesBuilder
    {
        public static List<Dynamic.Category> BuildCategories(TComm.Page page, BuildManager manager)
        {
            // note that there might be multiple fields based on the same category, so we need to combine them
            // for that purpose we use a dictionary


            if (page.Metadata == null || page.MetadataSchema == null)
            {
                return new List<Dynamic.Category>();
            }

            Dictionary<string, Dynamic.Category> categories = new Dictionary<string, Dynamic.Category>();


            // first, add categires from the metadata
            ItemFields tcmMetadataFields = new TCM.Fields.ItemFields(page.Metadata, page.MetadataSchema);
            addFromItemFields(tcmMetadataFields, categories, manager);

            // finally, create a List from the Dictionary and return it
            List<Dynamic.Category> categoryList = new List<Dynamic.Category>();
            foreach (Dynamic.Category cat in categories.Values)
            {
                categoryList.Add(cat);
            }
            return categoryList;
        }

        public static List<Dynamic.Category> BuildCategories(TCM.Component component, BuildManager manager)
        {
            // note that there might be multiple fields based on the same category, so we need to combine them
            // for that purpose we use a dictionary
            Dictionary<string, Dynamic.Category> categories = new Dictionary<string, Dynamic.Category>();

            // first, add categories from the content (note that mm components have no content!)
            if (component.Content != null)
            {
                GeneralUtils.TimedLog("start creating tcm ItemFields from Content");
                ItemFields tcmFields = new TCM.Fields.ItemFields(component.Content, component.Schema);
                GeneralUtils.TimedLog("finished creating tcm ItemFields from Content");
                GeneralUtils.TimedLog("start converting content ItemFields to dynamic fields");

                addFromItemFields(tcmFields, categories, manager);
                GeneralUtils.TimedLog("finished converting content ItemFields to dynamic fields");
            }

            // next, add categories from the metadata
            if (component.Metadata != null)
            {
                GeneralUtils.TimedLog("start creating tcm ItemFields from Metadata");
                ItemFields tcmMetadataFields = new TCM.Fields.ItemFields(component.Metadata, component.Schema);
                GeneralUtils.TimedLog("finished creating tcm ItemFields from Metadata");
                GeneralUtils.TimedLog("start converting metadata ItemFields to dynamic fields");
                addFromItemFields(tcmMetadataFields, categories, manager);
                GeneralUtils.TimedLog("finished converting metadata ItemFields to dynamic fields");
            }

            // finally, create a List from the Dictionary and return it
            List<Dynamic.Category> categoryList = new List<Dynamic.Category>();
            foreach (Dynamic.Category cat in categories.Values)
            {
                categoryList.Add(cat);
            }
            return categoryList;
        }

        private static void addFromItemFields(ItemFields tcmFields, Dictionary<string, Dynamic.Category> categories, BuildManager manager)
        {
            foreach (ItemField f in tcmFields)
            {
                if (f is KeywordField)
                {
                    string categoryId = ((KeywordFieldDefinition)f.Definition).Category.Id;
                    Dynamic.Category dc;
                    if (!categories.ContainsKey(categoryId))
                    {
                        // create category since it doesn't exist yet
                        dc = new Dynamic.Category();
                        dc.Id = ((KeywordFieldDefinition)f.Definition).Category.Id;
                        dc.Title = ((KeywordFieldDefinition)f.Definition).Category.Title;
                        dc.Keywords = new List<Dynamic.Keyword>();
                        categories.Add(dc.Id, dc);
                    }
                    else
                    {
                        dc = categories[categoryId];
                    }
                    foreach (Keyword keyword in ((KeywordField)f).Values)
                    {
                        bool alreadyThere = false;
                        foreach (Dynamic.Keyword dk in dc.Keywords)
                        {
                            if (dk.Id.Equals(keyword.Id))
                            {
                                alreadyThere = true;
                                break;
                            }
                        }
                        if (!alreadyThere)
                        {
                            dc.Keywords.Add(manager.BuildKeyword(keyword));
                        }
                    }
                }

                if (f is EmbeddedSchemaField)
                {
                    try
                    {
                        IList<ItemFields> embeddedFieldsCollection = ((EmbeddedSchemaField)f).Values;
                        foreach (ItemFields embeddedFields in embeddedFieldsCollection)
                        {
                            foreach (ItemField embeddedField in embeddedFields)
                            {
                                if (embeddedField is KeywordField)
                                {
                                    GeneralUtils.TimedLog("FOUND EMBEDDED KEYWORD FIELD");
                                    string categoryId = ((KeywordFieldDefinition)embeddedField.Definition).Category.Id;

                                    Dynamic.Category dc;
                                    if (!categories.ContainsKey(categoryId))
                                    {
                                        // create category since it doesn't exist yet
                                        dc = new Dynamic.Category();
                                        dc.Id = ((KeywordFieldDefinition)embeddedField.Definition).Category.Id;
                                        dc.Title = ((KeywordFieldDefinition)embeddedField.Definition).Category.Title;
                                        dc.Keywords = new List<Dynamic.Keyword>();
                                        categories.Add(dc.Id, dc);
                                    }
                                    else
                                    {
                                        dc = categories[categoryId];
                                    }
                                    foreach (Keyword keyword in ((KeywordField)embeddedField).Values)
                                    {
                                        bool alreadyThere = false;
                                        foreach (Dynamic.Keyword dk in dc.Keywords)
                                        {
                                            if (dk.Id.Equals(keyword.Id))
                                            {
                                                alreadyThere = true;
                                                break;
                                            }
                                        }
                                        if (!alreadyThere)
                                        {
                                            dc.Keywords.Add(manager.BuildKeyword(keyword));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GeneralUtils.TimedLog("ERROR: " + ex.Message);
                    }
                }
            }

        }
    }
}
