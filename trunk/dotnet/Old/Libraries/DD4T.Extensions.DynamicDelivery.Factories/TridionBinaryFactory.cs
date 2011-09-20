using System;
using System.ComponentModel.Composition;
using System.Web;
using DD4T.Extensions.DynamicDelivery.ContentModel;
using DD4T.Extensions.DynamicDelivery.ContentModel.Exceptions;
using DD4T.Extensions.DynamicDelivery.ContentModel.Factories;

namespace DD4T.Extensions.DynamicDelivery.Factories
{
    [Export(typeof(IBinaryFactory))]
    /// <summary>
    /// This is a temporary PageFactory, to be replaced by the DynamicDelivery.PageFactory
    /// </summary>
    public class TridionBinaryFactory : TridionBinaryFactoryBase
    {
        
        
    }
}