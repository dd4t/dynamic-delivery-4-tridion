namespace DD4T.ContentModel
{
    using System.Collections.Generic;

    public interface IComponentTemplate : IRepositoryLocal
    {
        IOrganizationalItem Folder { get; }
        IDictionary<string, IField> MetadataFields { get; }
        string OutputFormat { get; }
    }
}
