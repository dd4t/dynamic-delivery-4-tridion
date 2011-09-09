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
using TCM = Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;

namespace Tridion.Extensions.DynamicDelivery.Templates.Builder
{
   public class FieldBuilder
   {
       public static Dynamic.Field BuildField(TCM.Fields.ItemField tcmItemField, int linkLevels, bool resolveWidthAndHeight, BuildManager manager)
      {
         Dynamic.Field f = new Dynamic.Field();

         if (tcmItemField == null)
         {
            throw new Dynamic.Exceptions.FieldHasNoValueException();
         }
         f.Name = tcmItemField.Name;
         if (tcmItemField is TCM.Fields.XhtmlField)
         {
            TCM.Fields.XhtmlField sField = (TCM.Fields.XhtmlField)tcmItemField;
            if (sField.Values.Count == 0)
               throw new Dynamic.Exceptions.FieldHasNoValueException();
            foreach (string v in sField.Values)
            {
               f.Values.Add(v);
            }
            f.FieldType = Dynamic.FieldType.Xhtml;
            return f;
         }
         if (tcmItemField is TCM.Fields.MultiLineTextField)
         {
            TCM.Fields.TextField sField = (TCM.Fields.MultiLineTextField)tcmItemField;
            if (sField.Values.Count == 0)
               throw new Dynamic.Exceptions.FieldHasNoValueException();
            foreach (string v in sField.Values)
            {
               f.Values.Add(v);
            }
            f.FieldType = Dynamic.FieldType.MultiLineText;
            return f;
         }
         if (tcmItemField is TCM.Fields.TextField)
         {
            TCM.Fields.TextField sField = (TCM.Fields.TextField)tcmItemField;
            if (sField.Values.Count == 0)
               throw new Dynamic.Exceptions.FieldHasNoValueException();
            foreach (string v in sField.Values)
            {
               f.Values.Add(v);
            }
            f.FieldType = Dynamic.FieldType.Text;
            return f;
         }
         if (tcmItemField is TCM.Fields.KeywordField)
         {
            TCM.Fields.KeywordField sField = (TCM.Fields.KeywordField)tcmItemField;
            if (sField.Values.Count == 0)
               throw new Dynamic.Exceptions.FieldHasNoValueException();
            // we will wrap each linked component in a ContentModel component
            f.Values = new List<string>();
            foreach (TCM.Keyword kw in sField.Values)
            {
               // todo: add binary to package, and add BinaryUrl property to the component
               f.Values.Add(kw.Title);
            }
            f.FieldType = Dynamic.FieldType.Keyword;
            KeywordFieldDefinition fieldDef = (KeywordFieldDefinition) sField.Definition;
            f.CategoryId = fieldDef.Category.Id;
            f.CategoryName = fieldDef.Category.Title;
            return f;
         }
         if (tcmItemField is TCM.Fields.NumberField)
         {
            TCM.Fields.NumberField sField = (TCM.Fields.NumberField)tcmItemField;
            if (sField.Values.Count == 0)
               throw new Dynamic.Exceptions.FieldHasNoValueException();
            f.NumericValues = (List<double>)sField.Values;
            f.Values = new List<string>();
            foreach (double d in f.NumericValues)
            {
               f.Values.Add(Convert.ToString(d));
            }
            f.FieldType = Dynamic.FieldType.Number;
            return f;
         }
         if (tcmItemField is TCM.Fields.DateField)
         {
            TCM.Fields.DateField sField = (TCM.Fields.DateField)tcmItemField;
            if (sField.Values.Count == 0)
               throw new Dynamic.Exceptions.FieldHasNoValueException();
            f.DateTimeValues = (List<DateTime>)sField.Values;
            f.Values = new List<string>();
            foreach (DateTime dt in f.DateTimeValues)
            {
               f.Values.Add(Convert.ToString(dt));
            }
            f.FieldType = Dynamic.FieldType.Date;
            return f;
         }
         if (tcmItemField is TCM.Fields.MultimediaLinkField)
         {
            TCM.Fields.MultimediaLinkField sField = (TCM.Fields.MultimediaLinkField)tcmItemField;
            if (sField.Values.Count == 0)
               throw new Dynamic.Exceptions.FieldHasNoValueException();

            // we will wrap each linked component in a ContentModel component
            f.LinkedComponentValues = new List<Dynamic.Component>();
            foreach (TCM.Component comp in sField.Values)
            {
               // todo: add binary to package, and add BinaryUrl property to the component
                f.LinkedComponentValues.Add(manager.BuildComponent(comp, linkLevels - 1, resolveWidthAndHeight));
            }
            f.Values = new List<string>();
            foreach (Dynamic.Component c in f.LinkedComponentValues)
            {
               f.Values.Add(c.Id);
            }
            f.FieldType = Dynamic.FieldType.MultiMediaLink;
            return f;
         }
         if (tcmItemField is TCM.Fields.ComponentLinkField)
         {
            TCM.Fields.ComponentLinkField sField = (TCM.Fields.ComponentLinkField)tcmItemField;
            if (sField.Values.Count == 0)
               throw new Dynamic.Exceptions.FieldHasNoValueException();
            // we will wrap each linked component in a ContentModel component
            f.LinkedComponentValues = new List<Dynamic.Component>();
            foreach (TCM.Component comp in sField.Values)
            {
                f.LinkedComponentValues.Add(manager.BuildComponent(comp, linkLevels - 1, resolveWidthAndHeight));
            }
            f.Values = new List<string>();
            foreach (Dynamic.Component c in f.LinkedComponentValues)
            {
               f.Values.Add(c.Id);
            }
            f.FieldType = Dynamic.FieldType.ComponentLink;
            return f;
         }

         if (tcmItemField is TCM.Fields.EmbeddedSchemaField)
         {
            TCM.Fields.EmbeddedSchemaField sField = (TCM.Fields.EmbeddedSchemaField)tcmItemField;
            if (sField.Values.Count == 0)
               throw new Dynamic.Exceptions.FieldHasNoValueException();
            f.EmbeddedValues = new List<Dynamic.FieldSet>();
            f.FieldType = Dynamic.FieldType.Embedded;

            foreach (TCM.Fields.ItemFields embeddedFields in sField.Values)
            {
                Dynamic.FieldSet set = new Dynamic.FieldSet();            

                // add fields to set
                set.Fields = manager.BuildFields(embeddedFields, linkLevels, resolveWidthAndHeight);
                Dynamic.Schema schema = new Dynamic.Schema();

                if (sField.Definition is EmbeddedSchemaFieldDefinition)
                {
                    EmbeddedSchemaFieldDefinition def = (EmbeddedSchemaFieldDefinition)sField.Definition;

                    schema.RootElement = def.EmbeddedSchema.RootElementName;
                    schema.Id = def.EmbeddedSchema.Id;
                    schema.Title = def.EmbeddedSchema.Title;
                }
                else
                {
                    schema.RootElement = "unknown";
                    schema.Id = "unknown";
                    schema.Title = "unkown";
                }

                set.Schema = schema;

                f.EmbeddedValues.Add(set);
            }

            /*
           f.EmbeddedValues = new List<Dynamic.Fields>();
           foreach (TCM.Fields.ItemFields embeddedFields in sField.Values)
           {
               f.EmbeddedValues.Add(manager.BuildFields(embeddedFields, linkLevels, resolveWidthAndHeight));
           }

           f.FieldType = Dynamic.FieldType.Embedded;
           TCM.Fields.EmbeddedSchemaFieldDefinition def = sField.Definition as TCM.Fields.EmbeddedSchemaFieldDefinition;
           f.EmbeddedSchema = manager.BuildSchema(def.EmbeddedSchema);
             * */
            return f;
         }

         throw new Dynamic.Exceptions.FieldTypeNotDefinedException();
      }
   }
}
