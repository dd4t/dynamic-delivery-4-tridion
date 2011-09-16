using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DD4T.ContentModel
{
    public interface IBinary : IComponent
    {
        DateTime LastPublishedDate { get; }
        byte[] BinaryData { get; set; }
        string VariantId { get; }
        //IMultimedia Multimedia { get; set; }
        string Url { get; set;  }
    }
}
