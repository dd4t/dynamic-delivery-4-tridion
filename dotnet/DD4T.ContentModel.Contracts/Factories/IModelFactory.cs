namespace DD4T.ContentModel.Factories
{
    public interface IModelFactory
    {
        object CreateModel(string viewName, IComponentPresentation componentPresentation);
        bool TryCreateModel(string viewName, IComponentPresentation componentPresentation, out object model);
        I Create<I>(IPage page);
        string MapComponentViewName(string viewName);
    }
}
