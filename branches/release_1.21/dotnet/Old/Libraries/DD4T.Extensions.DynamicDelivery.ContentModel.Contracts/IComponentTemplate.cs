namespace DD4T.Extensions.DynamicDelivery.ContentModel
{
    using System.Collections.Generic;

    public interface IComponentTemplate : IRepositoryLocal
    {
        IOrganizationalItem Folder { get; }
        IDictionary<string, IField> MetadataFields { get; }
        string OutputFormat { get; }
    }
}
