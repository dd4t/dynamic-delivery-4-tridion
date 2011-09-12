namespace DD4T.ContentModel
{
    using System.Collections.Generic;

    public interface IPage : IRepositoryLocal, IViewable
    {
        IList<ICategory> Categories { get; }
        IList<IComponentPresentation> ComponentPresentations { get; }
        string Filename { get; }
        IDictionary<string, IField> Metadata { get; }
        IPageTemplate PageTemplate { get; }
        ISchema Schema { get; }
        IOrganizationalItem StructureGroup { get; }
        int Version { get; }
    }
}
