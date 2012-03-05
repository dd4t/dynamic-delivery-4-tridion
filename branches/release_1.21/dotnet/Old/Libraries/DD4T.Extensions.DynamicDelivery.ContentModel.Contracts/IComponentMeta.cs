namespace DD4T.Extensions.DynamicDelivery.ContentModel
{
    using System;
    using System.Collections.Generic;

    public interface IComponentMeta
    {
        DateTime ModificationDate { get; }
        DateTime CreationDate { get; }
        DateTime LastPublishedDate { get; }
    }
}
