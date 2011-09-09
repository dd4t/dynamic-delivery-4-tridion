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
using System.Configuration;
using System.Collections.Generic;
using Tridion.Extensions.DynamicDelivery.ContentModel.Exceptions;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Tridion.Extensions.DynamicDelivery.ContentModel
{


   public class Page : RepositoryLocalItem
   {
      public string Filename { get; set; }
      public PageTemplate PageTemplate { get; set; }
      public Schema Schema { get; set; }
      public Fields Metadata { get; set; }
      public List<ComponentPresentation> ComponentPresentations { get; set; }
      public OrganizationalItem StructureGroup { get; set; }
      public List<Category> Categories { get; set; }
      public int Version { get; set; }
   }

   public class Keyword : TridionItem
   {
      [XmlAttribute]
      public string TaxonomyId { get; set;  }
      [XmlAttribute]
      public string Path { get; set; }
   }

   public class Category : TridionItem
   {
      public List<Keyword> Keywords { get; set; }
   }

   public class ComponentPresentation
   {
      public Component Component { get; set; }
      public ComponentTemplate ComponentTemplate { get; set; }
      public string RenderedContent { get; set; }
   }

   public class PageTemplate : RepositoryLocalItem
   {
	   public string FileExtension { get; set; }
      public Fields MetadataFields { get; set; }
      public OrganizationalItem Folder { get; set; }
   }

   public class ComponentTemplate : RepositoryLocalItem
   {
      public string OutputFormat { get; set; }
      public Fields MetadataFields { get; set; }
      public OrganizationalItem Folder { get; set; }
   }

   public class FieldSet : RepositoryLocalItem
   {
       public Schema Schema { get; set; }
       public Fields Fields { get; set; }

       public FieldSet()
      {
         this.Schema = new Schema();
         this.Fields = new Fields();
      }
   }

   public class Component : FieldSet
   {
      #region public properties

      public Fields MetadataFields { get; set; }
	  public ComponentType ComponentType { get; set; }
     public Multimedia Multimedia { get; set; }
     public OrganizationalItem Folder { get; set; }
     public List<Category> Categories { get; set; }
     public int Version { get; set; }

	  #endregion

      #region constructors
      public Component() : base()
      {
         this.MetadataFields = new Fields();
      }
      #endregion
   }
   public class Schema : RepositoryLocalItem
   {
      public OrganizationalItem Folder { get; set; }
      public String RootElement { get; set; }
   }
   public enum MergeAction { Replace, Merge, Skip }
   public enum ComponentType { Multimedia, Normal }

   public class Fields : SerializableDictionary<string, Field> {
	   public Fields()
		   : base() {
	   }
   }

   public enum FieldType { Text, MultiLineText, Xhtml, Keyword, Embedded, MultiMediaLink, ComponentLink, ExternalLink, Number, Date }

   public class Field {
	   public string Name {
		   get;
		   set;
	   }
	   public List<string> Values {
		   get;
		   set;
	   }
	   public List<double> NumericValues {
		   get;
		   set;
	   }
	   public List<DateTime> DateTimeValues {
		   get;
		   set;
	   }
	   public List<Component> LinkedComponentValues {
		   get;
		   set;
	   }
       public List<FieldSet> EmbeddedValues
      {
         get;
         set;
      }

      [XmlAttribute]
	   public FieldType FieldType {
		   get;
		   set;
	   }

      [XmlAttribute]
      public string CategoryName
      {
         get;
         set;
      }

      [XmlAttribute]
      public string CategoryId
      {
         get;
         set;
      }

	   public Field() {
		   this.Values = new List<string>();
		   this.NumericValues = new List<double>();
		   this.DateTimeValues = new List<DateTime>();
		   this.LinkedComponentValues = new List<Component>();
           this.EmbeddedValues = new List<FieldSet>();
	   }

   }

   public abstract class TridionItem
   {
      public string Id { get; set; }
      public string Title { get; set; }
   }
   public abstract class RepositoryLocalItem : TridionItem
   {
      public string PublicationId { get; set; }
      public Publication Publication { get; set; }
   }

   public class OrganizationalItem : RepositoryLocalItem
   {
   }

   public class Publication : TridionItem
   {
      public string PublicationId
      {
         get
         {
            return null;
         }
      }
   }

   public class TcmUri {
	   public int ItemId { get; set; }
	   public int PublicationId { get; set; }
	   public int ItemTypeId { get; set; }
	   public int Version { get; set; }

	   public TcmUri(string Uri) {
		   Regex re = new Regex(@"tcm:(\d+)-(\d+)-?(\d*)-?v?(\d*)");
		   Match m = re.Match(Uri);
		   if (m.Success) {
			   PublicationId = Convert.ToInt32(m.Groups[1].Value);
			   ItemId = Convert.ToInt32(m.Groups[2].Value);
			   if (m.Groups.Count > 3 && ! string.IsNullOrEmpty(m.Groups[3].Value)) {
				   ItemTypeId = Convert.ToInt32(m.Groups[3].Value);
			   } else {
				   ItemTypeId = 16;
			   }
			   if (m.Groups.Count > 4 && !string.IsNullOrEmpty(m.Groups[4].Value)) {
				   Version = Convert.ToInt32(m.Groups[4].Value);
			   } else {
				   Version = 0;
			   }
		   }
	   }
	   public TcmUri(int PublicationId, int ItemId, int ItemTypeId, int Version) {
		   this.PublicationId = PublicationId;
		   this.ItemId = ItemId;
		   this.ItemTypeId = ItemTypeId;
		   this.Version = Version;
	   }
	   public override string ToString() {
		   if (this.ItemTypeId == 16) {
			   return string.Format("tcm:{0}-{1}", this.PublicationId, this.ItemId);
		   }
		   return string.Format("tcm:{0}-{1}-{2}", this.PublicationId, this.ItemId, this.ItemTypeId);
	   }
   }

   public class Multimedia
   {
      public string Url
      {
         get;
         set;
      }
      public string MimeType
      {
         get;
         set;
      }
      public string AltText
      {
         get;
         set;
      }
      public string FileName
      {
         get;
         set;
      }
      public string FileExtension
      {
         get;
         set;
      }
      public int Size
      {
         get;
         set;
      }
      public int Width
      {
         get;
         set;
      }
      public int Height
      {
         get;
         set;
      }
   }
}
