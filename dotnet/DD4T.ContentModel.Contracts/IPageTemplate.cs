namespace DD4T.ContentModel
{
    #region Usings
    using System.Collections.Generic;
    #endregion Usings

    public interface IPageTemplate : IRepositoryLocal
    {
        string FileExtension { get; }
        IOrganizationalItem Folder { get; }
        IFieldSet MetadataFields { get; }
    }
}
