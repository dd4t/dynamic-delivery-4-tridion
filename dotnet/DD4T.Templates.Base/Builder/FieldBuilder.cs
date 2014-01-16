using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.ContentModel;
using TCM = Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using DD4T.ContentModel.Exceptions;
using DD4T.ContentModel;
using Tridion.Logging;
using Tridion.ContentManager.Templating;
using DD4T.Templates.Base.Utils;
using System.Xml;

namespace DD4T.Templates.Base.Builder
{
    public class FieldBuilder
    {
        protected static TemplatingLogger log = TemplatingLogger.GetLogger(typeof(FieldBuilder));
        
        public static Dynamic.Field BuildField(TCM.Fields.ItemField tcmItemField, int linkLevels, bool resolveWidthAndHeight, bool publishEmptyFields, BuildManager manager)
        {
            Dynamic.Field f = new Dynamic.Field();

            if (tcmItemField == null&&publishEmptyFields)
            {
                GeneralUtils.TimedLog("item field is null");
                //throw new FieldHasNoValueException();
                f.Values.Add("");
                return f;                
            }
            else if (tcmItemField == null && !publishEmptyFields)
            {
                throw new FieldHasNoValueException();
            }
            f.Name = tcmItemField.Name;
            if (tcmItemField is TCM.Fields.XhtmlField)
            {
                TCM.Fields.XhtmlField sField = (TCM.Fields.XhtmlField)tcmItemField;
                GeneralUtils.TimedLog(string.Format("item field {0} has {1} values", tcmItemField.Name, sField.Values.Count));
                if (sField.Values.Count == 0 && publishEmptyFields)
                {
                    //throw new FieldHasNoValueException();
                    f.Values.Add("");
                }
                else if (sField.Values.Count == 0 && !publishEmptyFields)
                {
                    throw new FieldHasNoValueException();
                }
                else
                {
                    foreach (string v in sField.Values)
                    {
                        f.Values.Add(v);
                    }                    
                }
                f.FieldType = FieldType.Xhtml;
                return f;
            }
            if (tcmItemField is TCM.Fields.MultiLineTextField)
            {
                TCM.Fields.TextField sField = (TCM.Fields.MultiLineTextField)tcmItemField;
                GeneralUtils.TimedLog(string.Format("item field {0} has {1} values", tcmItemField.Name, sField.Values.Count));
                //if (sField.Values.Count == 0)
                  //  throw new FieldHasNoValueException();
                if (sField.Values.Count == 0 && publishEmptyFields)
                {
                    //throw new FieldHasNoValueException();
                    f.Values.Add("");
                }
                else if (sField.Values.Count == 0 && !publishEmptyFields)
                {
                    throw new FieldHasNoValueException();
                }
                else
                {
                    foreach (string v in sField.Values)
                    {
                        f.Values.Add(v);
                    }
                }
                f.FieldType = FieldType.MultiLineText;
                return f;
            }
            if (tcmItemField is TCM.Fields.TextField)
            {
                TCM.Fields.TextField sField = (TCM.Fields.TextField)tcmItemField;
                GeneralUtils.TimedLog(string.Format("item field {0} has {1} values", tcmItemField.Name, sField.Values.Count));
                //if (sField.Values.Count == 0)
                  //  throw new FieldHasNoValueException();
                if (sField.Values.Count == 0 && publishEmptyFields)
                {
                    //throw new FieldHasNoValueException();
                    f.Values.Add("");
                }
                else if (sField.Values.Count == 0 && !publishEmptyFields)
                {
                    throw new FieldHasNoValueException();
                }
                else
                {
                    foreach (string v in sField.Values)
                    {
                        f.Values.Add(v);
                    }
                }
                f.FieldType = FieldType.Text;
                return f;
            }
            if (tcmItemField is TCM.Fields.KeywordField)
            {
                TCM.Fields.KeywordField sField = (TCM.Fields.KeywordField)tcmItemField;
                GeneralUtils.TimedLog(string.Format("item field {0} has {1} values", tcmItemField.Name, sField.Values.Count));
                //if (sField.Values.Count == 0)
                //    throw new FieldHasNoValueException();
                if (sField.Values.Count == 0 && publishEmptyFields)
                {
                    //throw new FieldHasNoValueException();
                    f.Values.Add("");
                }
                else if (sField.Values.Count == 0 && !publishEmptyFields)
                {
                    throw new FieldHasNoValueException();
                }
                else
                {
                    // add keyword values
                    f.Keywords = new List<Keyword>();
                    // we will wrap each linked component in a ContentModel component
                    f.Values = new List<string>();
                    foreach (TCM.Keyword kw in sField.Values)
                    {
                        // todo: add binary to package, and add BinaryUrl property to the component
                        f.Values.Add(kw.Title);
                        f.Keywords.Add(manager.BuildKeyword(kw));
                    }
                    
                    KeywordFieldDefinition fieldDef = (KeywordFieldDefinition)sField.Definition;
                    f.CategoryId = fieldDef.Category.Id;
                    f.CategoryName = fieldDef.Category.Title;
                }
                f.FieldType = FieldType.Keyword;
                return f;
            }
            if (tcmItemField is TCM.Fields.NumberField)
            {
                TCM.Fields.NumberField sField = (TCM.Fields.NumberField)tcmItemField;
                GeneralUtils.TimedLog(string.Format("item field {0} has {1} values", tcmItemField.Name, sField.Values.Count));

                //if (sField.Values.Count == 0)
                  //  throw new FieldHasNoValueException();
                if (sField.Values.Count == 0 && publishEmptyFields)
                {
                    //throw new FieldHasNoValueException();
                    f.Values.Add("");
                }
                 else if (sField.Values.Count == 0 && !publishEmptyFields)
                 {
                     throw new FieldHasNoValueException();
                 }
                 else
                 {
                     f.NumericValues = (List<double>)sField.Values;
                     f.Values = new List<string>();
                     foreach (double d in f.NumericValues)
                     {
                         f.Values.Add(Convert.ToString(d));
                     }
                 }
                f.FieldType = FieldType.Number;
                return f;
            }
            if (tcmItemField is TCM.Fields.DateField)
            {
                TCM.Fields.DateField sField = (TCM.Fields.DateField)tcmItemField;
                GeneralUtils.TimedLog(string.Format("item field {0} has {1} values", tcmItemField.Name, sField.Values.Count));
                //if (sField.Values.Count == 0)
                //  throw new FieldHasNoValueException();

                if (sField.Values.Count == 0 && publishEmptyFields)
                {
                    //throw new FieldHasNoValueException();
                    f.Values.Add("");
                }
                else if (sField.Values.Count == 0 && !publishEmptyFields)
                {
                    throw new FieldHasNoValueException();
                }
                else
                {

                    f.DateTimeValues = (List<DateTime>)sField.Values;
                    f.Values = new List<string>();
                    foreach (DateTime dt in f.DateTimeValues)
                    {
                        f.Values.Add(Convert.ToString(dt));
                    }
                }
                f.FieldType = FieldType.Date;
                return f;
            }
            if (tcmItemField is TCM.Fields.MultimediaLinkField)
            {
                TCM.Fields.MultimediaLinkField sField = (TCM.Fields.MultimediaLinkField)tcmItemField;
                GeneralUtils.TimedLog(string.Format("item field {0} has {1} values", tcmItemField.Name, sField.Values.Count));
                if (sField.Values.Count == 0 && publishEmptyFields)
                {
                    f.Values.Add(tcmItemField.Name);
                }
                else if (sField.Values.Count == 0 && !publishEmptyFields)
                {
                    throw new FieldHasNoValueException();
                }
                else
                {

                    // we will wrap each linked component in a ContentModel component
                    f.LinkedComponentValues = new List<Dynamic.Component>();
                    int nextLinkLevel = linkLevels - 1;
                    if (MustFollowField(sField, manager))
                    {
                        GeneralUtils.TimedLog(string.Format("found component link field named {0} with global followLinksPerField property set to false OR followLink set to true for this field", sField.Name));
                    }
                    else
                    {
                        GeneralUtils.TimedLog(string.Format("found component link field named {0} with followLink set to false for this field", sField.Name));
                        nextLinkLevel = 0;
                    }
                    foreach (TCM.Component comp in sField.Values)
                    {
                        f.LinkedComponentValues.Add(manager.BuildComponent(comp, nextLinkLevel, resolveWidthAndHeight, publishEmptyFields));
                    }

                    f.Values = new List<string>();
                    foreach (Dynamic.Component c in f.LinkedComponentValues)
                    {
                        f.Values.Add(c.Id);
                    }
                }
                f.FieldType = FieldType.MultiMediaLink;
                return f;
            }
            if (tcmItemField is TCM.Fields.ComponentLinkField)
            {
                TCM.Fields.ComponentLinkField sField = (TCM.Fields.ComponentLinkField)tcmItemField;
                GeneralUtils.TimedLog(string.Format("item field {0} has {1} values", tcmItemField.Name, sField.Values.Count));
                //if (sField.Values.Count == 0)
                //  throw new FieldHasNoValueException();
                if (sField.Values.Count == 0 && publishEmptyFields)
                {
                    f.Values.Add(tcmItemField.Name);
                }
                else if (sField.Values.Count == 0 && !publishEmptyFields)
                {
                    throw new FieldHasNoValueException();
                }
                else
                {
                    // we will wrap each linked component in a ContentModel component
                    f.LinkedComponentValues = new List<Dynamic.Component>();
                    int nextLinkLevel = linkLevels - 1;
                    if (MustFollowField(sField, manager))
                    {
                        GeneralUtils.TimedLog(string.Format("found component link field named {0} with global followLinksPerField property set to false OR followLink set to true for this field", sField.Name));
                    }
                    else
                    {
                        GeneralUtils.TimedLog(string.Format("found component link field named {0} with followLink set to false for this field", sField.Name));
                        nextLinkLevel = 0;
                    }

                    foreach (TCM.Component comp in sField.Values)
                    {
                        f.LinkedComponentValues.Add(manager.BuildComponent(comp, nextLinkLevel, resolveWidthAndHeight, publishEmptyFields));
                    }


                    f.Values = new List<string>();
                    foreach (Dynamic.Component c in f.LinkedComponentValues)
                    {
                        f.Values.Add(c.Id);
                    }
                }
                f.FieldType = FieldType.ComponentLink;
                return f;
            }

            if (tcmItemField is TCM.Fields.EmbeddedSchemaField)
            {
                
                TCM.Fields.EmbeddedSchemaField sField = (TCM.Fields.EmbeddedSchemaField)tcmItemField;
                GeneralUtils.TimedLog(string.Format("item field {0} has {1} values", tcmItemField.Name, sField.Values.Count));
                //if (sField.Values.Count == 0)
                  //throw new FieldHasNoValueException();
                f.FieldType = FieldType.Embedded;
                f.EmbeddedValues = new List<Dynamic.FieldSet>();
                
                if (sField.Values.Count == 0 && publishEmptyFields)
                {
                    Dynamic.FieldSet fields = new FieldSet();
                    Dynamic.Field fe = new Dynamic.Field();
                    f.EmbeddedSchema = manager.BuildSchema(((EmbeddedSchemaFieldDefinition)sField.Definition).EmbeddedSchema);
                    EmbeddedSchemaFieldDefinition linksFieldDefinition = sField.Definition as EmbeddedSchemaFieldDefinition;
                    ItemFields newItemField = new ItemFields(linksFieldDefinition.EmbeddedSchema);
                    
                    for (int i = 0; i < newItemField.Count; i++)
                    {

                        if (newItemField[i] is TCM.Fields.EmbeddedSchemaField)
                        {
                            TCM.Fields.EmbeddedSchemaField innerField = (TCM.Fields.EmbeddedSchemaField)newItemField[i];
                            if (innerField.Values.Count == 0)
                            {
                                Dynamic.FieldSet fieldsinner = new FieldSet();
                                Dynamic.Field fin = new Dynamic.Field();
                                fe.EmbeddedSchema = manager.BuildSchema(((EmbeddedSchemaFieldDefinition)innerField.Definition).EmbeddedSchema);
                                EmbeddedSchemaFieldDefinition inlinksFieldDefinition = innerField.Definition as EmbeddedSchemaFieldDefinition;
                                ItemFields newinItemField = new ItemFields(inlinksFieldDefinition.EmbeddedSchema);
                                for (int n = 0; n < newinItemField.Count; n++)
                                {
                                    fin.Name = newItemField[n].Name;
                                    fieldsinner.Add(newinItemField[n].Name, fin);
                                }
                                fe.EmbeddedValues.Add(fieldsinner);
                                fe.Values.Add("");
                            }
                            fe.Name = newItemField[i].Name;
                            fields.Add(newItemField[i].Name, fe);

                        }
                        else
                        {
                            Dynamic.Field fein = manager.BuildField(newItemField[i], linkLevels, resolveWidthAndHeight, publishEmptyFields);
                            fein.Values.Clear();
                            fields.Add(newItemField[i].Name, fein);
                           
                        }                       
                                                
                    }
                    f.EmbeddedValues.Add(fields);                    
                    f.Values.Add("");
                }
                else if (sField.Values.Count == 0 && !publishEmptyFields)
                {
                    throw new FieldHasNoValueException();
                }
                else
                {

                    // we will wrap each linked component in a ContentModel component
                    f.EmbeddedValues = new List<Dynamic.FieldSet>();
                    f.EmbeddedSchema = manager.BuildSchema(((EmbeddedSchemaFieldDefinition)sField.Definition).EmbeddedSchema);
                    
                    foreach (TCM.Fields.ItemFields embeddedFields in sField.Values)
                    {
                        f.EmbeddedValues.Add(manager.BuildFields(embeddedFields, linkLevels, resolveWidthAndHeight, publishEmptyFields));
                    }
                }
                
                return f;
            }

            throw new FieldTypeNotDefinedException();
        }

        private static bool MustFollowField(TCM.Fields.ComponentLinkField field, BuildManager manager)
        {
            // TODO: check for setting 'followLinksPerField'
            if (!manager.BuildProperties.FollowLinksPerField)
                return true;
            XmlNamespaceManager nsMan = new XmlNamespaceManager(new NameTable());
            nsMan.AddNamespace("dd4t", "http://www.sdltridion.com/2011/DD4TField");
            XmlElement followLink = (XmlElement)field.Definition.ExtensionXml.SelectSingleNode("//dd4t:configuration/dd4t:followlink", nsMan);
            return (followLink != null && followLink.InnerText == "true");
        }        
    }
}
