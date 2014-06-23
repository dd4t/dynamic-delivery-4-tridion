namespace DD4T.ContentModel
{
    #region Usings

	using System;
    #endregion Usings

    public interface IPageTemplate : IRepositoryLocal
    {
        string FileExtension { get; }
        DateTime RevisionDate { get; }
        IOrganizationalItem Folder { get; }
        IFieldSet MetadataFields { get; }
    }
}
