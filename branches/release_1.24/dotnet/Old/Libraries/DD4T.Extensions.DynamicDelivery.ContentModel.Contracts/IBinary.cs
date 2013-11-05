using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DD4T.Extensions.DynamicDelivery.ContentModel
{
    public interface IBinary : IComponent
    {
        DateTime LastPublishedDate { get; }
        byte[] BinaryData { get; }
        string VariantId { get; }
        //IMultimedia Multimedia { get; set; }
        string Url { get; }
    }
}
