namespace DD4T.ContentModel
{
	using System;

    public interface IComponentTemplate : IRepositoryLocal
    {
        IOrganizationalItem Folder { get; }
        IFieldSet MetadataFields { get; }
        string OutputFormat { get; }
        DateTime RevisionDate { get; }

    }
}
