using DD4T.ContentModel.Contracts.Resolvers;
using DD4T.Utils;

namespace DD4T.Factories.Resolvers
{
    public class DefaultPublicationResolver : IPublicationResolver
    {
        public int ResolvePublicationId()
        {
            return ConfigurationHelper.PublicationId;
        }
    }
}
