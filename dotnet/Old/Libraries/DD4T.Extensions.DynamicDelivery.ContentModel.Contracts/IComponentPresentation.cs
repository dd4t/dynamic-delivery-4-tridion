namespace DD4T.Extensions.DynamicDelivery.ContentModel
{
    public interface IComponentPresentation 
    {
        IComponent Component { get; }
        IComponentTemplate ComponentTemplate { get; }
        IPage Page { get; set; }
        string RenderedContent { get; }
    }
}
