using DD4T.ContentModel.Contracts.Providers;

namespace DD4T.Providers.Test
{
    public class BaseProvider : IProvider
    {
        public int PublicationId { get; set; }
    }
}
