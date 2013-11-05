namespace DD4T.Extensions.DynamicDelivery.ContentModel
{
    public interface ISchema : IRepositoryLocal
    {
        IOrganizationalItem Folder { get; }
    }
}
