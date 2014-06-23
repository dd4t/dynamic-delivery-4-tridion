namespace DD4T.ContentModel.Contracts.Providers
{
    public interface ILinkProvider : IProvider
    {
        string ResolveLink(string componentUri);
        string ResolveLink(string sourcePageUri, string componentUri, string excludeComponentTemplateUri);
    }
}
