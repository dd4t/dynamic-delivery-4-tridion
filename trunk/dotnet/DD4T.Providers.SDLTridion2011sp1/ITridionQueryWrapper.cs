using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentDelivery.DynamicContent.Query;
using DD4T.ContentModel.Querying;

namespace DD4T.Providers.SDLTridion2011sp1
{
    public interface ITridionQueryWrapper : IQuery
    {
        Query ToTridionQuery();
    }
}
