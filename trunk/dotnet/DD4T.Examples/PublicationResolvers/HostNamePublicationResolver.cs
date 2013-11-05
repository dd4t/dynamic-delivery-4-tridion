using System;
using System.Web;
using DD4T.ContentModel.Contracts.Resolvers;

namespace Trivident.DD4T.Examples.PublicationResolvers
{
    public class HostNamePublicationResolver : IPublicationResolver
    {
        public int ResolvePublicationId()
        {
            switch (HttpContext.Current.Request.Url.Host)
            {
                case "www.acme.de":
                    return 17;
                case "www.acme.com":
                    return 16;
            }
            throw new InvalidOperationException(string.Format("unknown hostname '{0}", HttpContext.Current.Request.Url.Host));
        }
    }
}
