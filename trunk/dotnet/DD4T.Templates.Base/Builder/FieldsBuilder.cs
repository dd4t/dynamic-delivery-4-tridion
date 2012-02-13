using System;
using DD4T.ContentModel;
using DD4T.ContentModel.Exceptions;
using Dynamic = DD4T.ContentModel;
using TCM = Tridion.ContentManager.ContentManagement;
using DD4T.Templates.Base.Utils;

namespace DD4T.Templates.Base.Builder
{
   public class FieldsBuilder
   {
       public static Dynamic.FieldSet BuildFields(TCM.Fields.ItemFields tcmItemFields, int linkLevels, bool resolveWidthAndHeight, BuildManager manager)
      {
         Dynamic.FieldSet fields = new FieldSet();
         AddFields(fields, tcmItemFields, linkLevels, resolveWidthAndHeight, Dynamic.MergeAction.Replace, manager);
         return fields;
      }
       public static void AddFields(Dynamic.FieldSet fields, TCM.Fields.ItemFields tcmItemFields, int linkLevels, bool resolveWidthAndHeight, Dynamic.MergeAction mergeAction, BuildManager manager)
       {
           GeneralUtils.TimedLog(string.Format("add fields: found {0} fields",tcmItemFields.Count));
           

           foreach (TCM.Fields.ItemField tcmItemField in tcmItemFields)
           {
               GeneralUtils.TimedLog("add fields: found " + tcmItemField.Name);
               try
               {
                   if (fields.ContainsKey(tcmItemField.Name))
                   {
                       GeneralUtils.TimedLog("field exists already, with " + fields[tcmItemField.Name].Values.Count + " values");
                       if (mergeAction.Equals(Dynamic.MergeAction.Skip) || (mergeAction.Equals(Dynamic.MergeAction.MergeMultiValueSkipSingleValue) && tcmItemField.Definition.MaxOccurs == 1))
                       {
                           GeneralUtils.TimedLog(string.Format("skipping field (merge action {0}, maxoccurs {1}", mergeAction.ToString(), tcmItemField.Definition.MaxOccurs));
                           continue;
                       }
                       Dynamic.Field f = manager.BuildField(tcmItemField, linkLevels, resolveWidthAndHeight);
                       if (mergeAction.Equals(Dynamic.MergeAction.Replace) || (mergeAction.Equals(Dynamic.MergeAction.MergeMultiValueReplaceSingleValue) && tcmItemField.Definition.MaxOccurs == 1))
                       {
                           GeneralUtils.TimedLog(string.Format("replacing field (merge action {0}, maxoccurs {1}", mergeAction.ToString(), tcmItemField.Definition.MaxOccurs));
                           fields.Remove(f.Name);
                           fields.Add(f.Name, f);
                       }
                       else
                       {
                           IField existingField = fields[f.Name];
                           switch (existingField.FieldType)
                           {

                               case FieldType.ComponentLink:
                               case FieldType.MultiMediaLink:
                                   foreach (Component linkedComponent in f.LinkedComponentValues)
                                   {
                                       bool valueExists = false;
                                       foreach (Component existingLinkedComponent in existingField.LinkedComponentValues)
                                       {
                                           if (linkedComponent.Id.Equals(existingLinkedComponent.Id))
                                           {
                                               // this value already exists
                                               valueExists = true;
                                               break;
                                           }
                                       }
                                       if (!valueExists)
                                       {
                                           existingField.LinkedComponentValues.Add(linkedComponent);
                                       }
                                   }
                                   break;
                               case FieldType.Date:
                                   foreach (DateTime dateTime in f.DateTimeValues)
                                   {
                                       bool valueExists = false;
                                       foreach (DateTime existingDateTime in existingField.DateTimeValues)
                                       {
                                           if (dateTime.Equals(existingDateTime))
                                           {
                                               // this value already exists
                                               valueExists = true;
                                               break;
                                           }
                                       }
                                       if (!valueExists)
                                       {
                                           existingField.DateTimeValues.Add(dateTime);
                                       }
                                   }
                                   break;
                               case FieldType.Number:
                                   foreach (int nr in f.NumericValues)
                                   {
                                       bool valueExists = false;
                                       foreach (int existingNr in existingField.NumericValues)
                                       {
                                           if (nr == existingNr)
                                           {
                                               // this value already exists
                                               valueExists = true;
                                               break;
                                           }
                                       }
                                       if (!valueExists)
                                       {
                                           existingField.NumericValues.Add(nr);
                                       }
                                   }
                                   break;
                               default:
                                   foreach (string val in f.Values)
                                   {
                                       bool valueExists = false;
                                       foreach (string existingVal in existingField.Values)
                                       {
                                           if (val.Equals(existingVal))
                                           {
                                               // this value already exists
                                               valueExists = true;
                                               break;
                                           }
                                       }
                                       GeneralUtils.TimedLog(string.Format("found value {0}, valueExists: {1}", val, valueExists));
                                       if (!valueExists)
                                       {
                                           existingField.Values.Add(val);
                                       }
                                   }
                                   break;
                           }
                       }
                   }
                   else
                   {
                       Dynamic.Field f = manager.BuildField(tcmItemField, linkLevels, resolveWidthAndHeight);
                       fields.Add(f.Name, f);
                   }
               }
               catch (FieldHasNoValueException)
               {
                   // fail silently, field is not added to the list
               }
               catch (FieldTypeNotDefinedException)
               {
                   // fail silently, field is not added to the list
               }
           }
       }

       public static void AddXpathToFields(Dynamic.FieldSet fieldSet, string baseXpath)
       {
          // add XPath properties to all fields

           if (fieldSet == null)
           {
               GeneralUtils.TimedLog("Error: fieldSet = null");
           }
           if (fieldSet.Values == null)
           {
               GeneralUtils.TimedLog("Error: fieldSet.Values = null");
           }
           try
           {
               foreach (Field f in fieldSet.Values)
               {
                   if (f == null)
                   {
                       GeneralUtils.TimedLog("Error: field = null");
                   }
                   f.XPath = string.Format("{0}/custom:{1}", baseXpath, f.Name);
                   int i = 1;
                   if (f.EmbeddedValues != null)
                   {
                       foreach (FieldSet subFields in f.EmbeddedValues)
                       {
                           AddXpathToFields(subFields, string.Format("{0}/custom:{1}[{2}]", baseXpath, f.Name, i++));
                       }
                   }
               }
           }
           catch (Exception e)
           {
               GeneralUtils.TimedLog("Caught exception: " + e.Message);
           }
       }
   }
}
