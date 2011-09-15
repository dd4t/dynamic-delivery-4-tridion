using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;

namespace DD4T.Extensions.DynamicDelivery.Factories
{
    using System.ComponentModel.Composition;

    [Export(typeof(ILinkFactory))]
    public class TridionLinkFactory : TridionLinkFactoryBase
    {
        
    }
}

