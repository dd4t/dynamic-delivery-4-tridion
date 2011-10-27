using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentDelivery.DynamicContent.Query;

namespace DD4T.Providers.SDLTridion2011
{
    public interface ITridionQueryWrapper : DD4T.ContentModel.Contracts.Providers.IQuery
    {
        Query ToTridionQuery();
    }
}
