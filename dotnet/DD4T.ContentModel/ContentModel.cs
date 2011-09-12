using System.Linq;

namespace DD4T.ContentModel
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Xml.Serialization;
using DD4T.ContentModel.Factories;
    using System.Runtime.Serialization;
    #endregion Usings

    public class ComponentMeta : IComponentMeta
    {
        public string ID { get; set; }
        public DateTime ModificationDate { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastPublishedDate { get; set; }

        DateTime IComponentMeta.ModificationDate
        {
            get { return ModificationDate; }        
        }

        DateTime IComponentMeta.CreationDate
        {
            get { return CreationDate; }
        }

        DateTime IComponentMeta.LastPublishedDate
        {
            get { return LastPublishedDate; }
        }
    }

    public class Page : RepositoryLocalItem, IPage
    {
        public string Filename { get; set; }

        public PageTemplate PageTemplate { get; set; }
        [XmlIgnore]
        IPageTemplate IPage.PageTemplate
        {
            get { return PageTemplate; }
        }

        public Schema Schema { get; set; }
        [XmlIgnore]
        ISchema IPage.Schema
        {
            get { return Schema; }
        }
        public SerializableDictionary<string, Field> Metadata { get; set; }
        [XmlIgnore]
        IDictionary<string, IField> IPage.Metadata
        {
            get { return (Metadata != null ? (Metadata as Dictionary<string, Field>).Values.ToDictionary<IField, string>(f => f.Name) : null); }
        }
        public List<ComponentPresentation> ComponentPresentations { get; set; }
        [XmlIgnore]
        IList<IComponentPresentation> IPage.ComponentPresentations
        {
            get { return ComponentPresentations.ToList<IComponentPresentation>(); }
        }

        public OrganizationalItem StructureGroup { get; set; }
        [XmlIgnore]
        IOrganizationalItem IPage.StructureGroup
        {
            get { return StructureGroup; }
        }
        public List<Category> Categories { get; set; }
        [XmlIgnore]
        IList<ICategory> IPage.Categories
        {
            get { return Categories.ToList<ICategory>(); }
        }

        public int Version { get; set; }
    }

    public class Keyword : TridionItem, IKeyword
    {
        [XmlAttribute]
        public string TaxonomyId { get; set; }
        [XmlAttribute]
        public string Path { get; set; }   
        private List<IKeyword> parentKeywords = new List<IKeyword>();
        [XmlIgnore]
        public IList<IKeyword> ParentKeywords { get { return parentKeywords; } }    

    }

    public class Category : TridionItem, ICategory
    {
        public List<Keyword> Keywords { get; set; }
        [XmlIgnore]
        IList<IKeyword> ICategory.Keywords
        { get { return Keywords as IList<IKeyword>; } }
    }

    public class ComponentPresentation : IComponentPresentation
    {
        [XmlIgnore]
        public IPage Page { get; set; }
        public Component Component { get; set; }
        [XmlIgnore]
        IComponent IComponentPresentation.Component
        {
            get { return Component as IComponent; }
        }
        public ComponentTemplate ComponentTemplate { get; set; }
        [XmlIgnore]
        IComponentTemplate IComponentPresentation.ComponentTemplate
        {
            get { return ComponentTemplate as IComponentTemplate; }
        }
        public string RenderedContent { get; set; }
        public bool IsDynamic { get; set; }


    }

    public class PageTemplate : RepositoryLocalItem, IPageTemplate
    {
        public string FileExtension { get; set; }
        public SerializableDictionary<string, Field> MetadataFields { get; set; }
        [XmlIgnore]
        IDictionary<string, IField> IPageTemplate.MetadataFields
        {
            get { return (MetadataFields != null ? (MetadataFields as Dictionary<string, Field>).Values.ToDictionary<IField, string>(f => f.Name) : null); }
        }
        public OrganizationalItem Folder { get; set; }
        [XmlIgnore]
        IOrganizationalItem IPageTemplate.Folder
        {
            get { return Folder as IOrganizationalItem; }
        }
    }

    public class ComponentTemplate : RepositoryLocalItem, IComponentTemplate
    {
        public string OutputFormat { get; set; }
        public SerializableDictionary<string, Field> MetadataFields { get; set; }
        [XmlIgnore]
        IDictionary<string, IField> IComponentTemplate.MetadataFields
        {
            get { return (MetadataFields != null ? (MetadataFields as Dictionary<string, Field>).Values.ToDictionary<IField, string>(f => f.Name) : null); }
        }
        public OrganizationalItem Folder { get; set; }
        [XmlIgnore]
        IOrganizationalItem IComponentTemplate.Folder
        {
            get { return Folder as IOrganizationalItem; }
        }
    }

    public class Component : RepositoryLocalItem, IComponent
    {

        #region Properties
        public Schema Schema { get; set; }
        [XmlIgnore]
        ISchema IComponent.Schema
        {
            get { return Schema; }
        }

        public SerializableDictionary<string, Field> Fields { get; set; }
        [XmlIgnore]
        IDictionary<string, IField> IComponent.Fields
        {
            get { return (Fields != null ? (Fields as Dictionary<string, Field>).Values.ToDictionary<IField, string>(f => f.Name) : null); }
        }
        public SerializableDictionary<string, Field> MetadataFields { get; set; }
        [XmlIgnore]
        IDictionary<string, IField> IComponent.MetadataFields
        {
            get { return (MetadataFields != null ? (MetadataFields as Dictionary<string, Field>).Values.ToDictionary<IField, string>(f => f.Name) : null); }
        }
        public ComponentType ComponentType { get; set; }
        public Multimedia Multimedia { get; set; }
        [XmlIgnore]
        IMultimedia IComponent.Multimedia
        {
            get { return Multimedia as IMultimedia; }
        }
        public OrganizationalItem Folder { get; set; }
        [XmlIgnore]
        IOrganizationalItem IComponent.Folder
        {
            get { return Folder as IOrganizationalItem; }
        }
        public List<Category> Categories { get; set; }
        [XmlIgnore]
        IList<ICategory> IComponent.Categories
        {
            get { return Categories as IList<ICategory>; }
        }
        //[XmlIgnore]
        //public string ResolvedUrl
        //{
        //    get
        //    {
        //        //TODO: check if the LinkFactory has been set
        //        return LinkFactory.ResolveLink(this.Id);
        //    }
        //}

        //public ILinkFactory LinkFactory { get; set; }

        public int Version { get; set; }
        #endregion Properties

        #region constructors
        public Component()
        {
            this.Schema = new Schema();
            this.Fields = new SerializableDictionary<string, Field>();
            this.MetadataFields = new SerializableDictionary<string, Field>();
        }
        #endregion constructors
    }
    public class Schema : RepositoryLocalItem, ISchema
    {
        public OrganizationalItem Folder { get; set; }
        [XmlIgnore]
        IOrganizationalItem ISchema.Folder
        {
            get { return Folder as IOrganizationalItem; }
        }
    }
    public enum MergeAction { Replace, Merge, Skip }

    [Serializable]
    public class Fields : SerializableDictionary<string, Field>
    {
        public Fields()
            : base()
        {
        }

        protected Fields(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class Field : IField
    {
        #region Properties
        public string Name
        {
            get;
            set;
        }
        public string Value
        {
            get{
                if (this.Values == null || this.Values.Count == 0)
                    return string.Empty;
                return this.Values[0];
            }                
        }
        public List<string> Values
        {
            get;
            set;
        }
        [XmlIgnore]
        IList<string> IField.Values
        {
            get { return Values as IList<string>; }
        }
        public List<double> NumericValues
        {
            get;
            set;
        }
        [XmlIgnore]
        IList<double> IField.NumericValues
        {
            get { return NumericValues as IList<double>; }
        }
        public List<DateTime> DateTimeValues
        {
            get;
            set;
        }
        [XmlIgnore]
        IList<DateTime> IField.DateTimeValues
        {
            get { return DateTimeValues as IList<DateTime>; }
        }
        public List<Component> LinkedComponentValues
        {
            get;
            set;
        }
        [XmlIgnore]
        IList<IComponent> IField.LinkedComponentValues
        {
            get { return LinkedComponentValues.ToList<IComponent>(); }
        }
        public List<Fields> EmbeddedValues
        {
            get;
            set;
        }
        [XmlIgnore]
        IList<IDictionary<string, IField>> IField.EmbeddedValues
        {
            get { return (EmbeddedValues.Select<SerializableDictionary<string, Field>,IDictionary<string, IField>>(e => e.Values.ToDictionary<IField, string>(f => f.Name)).ToList<IDictionary<string, IField>>()); }
        }

        [XmlAttribute]
        public FieldType FieldType
        {
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
        #endregion Properties
        #region Constructors
        public Field()
        {
            this.Values = new List<string>();
            this.NumericValues = new List<double>();
            this.DateTimeValues = new List<DateTime>();
            this.LinkedComponentValues = new List<Component>();
        }
        #endregion Constructors
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

    public class OrganizationalItem : RepositoryLocalItem, IOrganizationalItem
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

    public class TcmUri
    {
        public int ItemId { get; set; }
        public int PublicationId { get; set; }
        public int ItemTypeId { get; set; }
        public int Version { get; set; }

        public TcmUri(string Uri)
        {
            Regex re = new Regex(@"tcm:(\d+)-(\d+)-?(\d*)-?v?(\d*)");
            Match m = re.Match(Uri);
            if (m.Success)
            {
                PublicationId = Convert.ToInt32(m.Groups[1].Value);
                ItemId = Convert.ToInt32(m.Groups[2].Value);
                if (m.Groups.Count > 3 && !string.IsNullOrEmpty(m.Groups[3].Value))
                {
                    ItemTypeId = Convert.ToInt32(m.Groups[3].Value);
                }
                else
                {
                    ItemTypeId = 16;
                }
                if (m.Groups.Count > 4 && !string.IsNullOrEmpty(m.Groups[4].Value))
                {
                    Version = Convert.ToInt32(m.Groups[4].Value);
                }
                else
                {
                    Version = 0;
                }
            }
        }
        public TcmUri(int PublicationId, int ItemId, int ItemTypeId, int Version)
        {
            this.PublicationId = PublicationId;
            this.ItemId = ItemId;
            this.ItemTypeId = ItemTypeId;
            this.Version = Version;
        }
        public override string ToString()
        {
            if (this.ItemTypeId == 16)
            {
                return string.Format("tcm:{0}-{1}", this.PublicationId, this.ItemId);
            }
            return string.Format("tcm:{0}-{1}-{2}", this.PublicationId, this.ItemId, this.ItemTypeId);
        }
    }

    public class Multimedia : IMultimedia
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


    public class Binary : Component, IBinary
    {

        public Binary(IBinaryFactory factory)
        {
            this.Factory = factory;
        }

        public DateTime LastPublishedDate { get; set; }

        public byte[] BinaryData 
        {
            get
            {
                if (this.binaryData == null)
                {                    
                    this.binaryData = this.Factory.FindBinaryContent(Url);
                }
                return this.binaryData;
            }
        }

        public string VariantId { get; set; }
        public string Url { get; set; }
        //public IMultimedia Multimedia { get; set; }

        private byte[] binaryData = null;
        private IBinaryFactory Factory { get; set; }
    }
}
