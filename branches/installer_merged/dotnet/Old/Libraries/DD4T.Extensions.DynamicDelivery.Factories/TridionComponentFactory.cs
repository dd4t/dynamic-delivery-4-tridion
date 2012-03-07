using Tridion.Extensions.DynamicDelivery.ContentModel;
using Tridion.Extensions.DynamicDelivery.ContentModel.Exceptions;
using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;

namespace DD4T.Extensions.DynamicDelivery.Factories
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;

    [Export(typeof(IComponentFactory))]
    public class TridionComponentFactory : TridionComponentFactoryBase    
    {
    }
}

