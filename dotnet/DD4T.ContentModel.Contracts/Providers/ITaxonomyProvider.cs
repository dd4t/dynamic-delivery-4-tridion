namespace DD4T.ContentModel.Contracts.Providers
{
    public interface ITaxonomyProvider : IProvider
    {
        IKeyword GetKeyword(string categoryUriToLookIn, string keywordName);
    }
}
