namespace DD4T.ContentModel
{
    using System.Collections.Generic;

    public interface IComponent : IRepositoryLocal, IViewable
    {
        IList<ICategory> Categories { get; }
        ComponentType ComponentType { get; }
        IDictionary<string,IField> Fields { get; }
        IOrganizationalItem Folder { get; }
        IDictionary<string,IField> MetadataFields { get; }
        IMultimedia Multimedia { get; }
        ISchema Schema { get; }
        //string ResolvedUrl { get; set; }
        int Version { get; }
    }
}
