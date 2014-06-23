namespace DD4T.ContentModel
{
    using System;

	public interface IComponentMeta
    {
        DateTime ModificationDate { get; }
        DateTime CreationDate { get; }
        DateTime LastPublishedDate { get; }
    }
}
