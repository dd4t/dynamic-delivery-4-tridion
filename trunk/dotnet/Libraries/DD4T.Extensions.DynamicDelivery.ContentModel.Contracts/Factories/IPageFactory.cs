using System;
namespace DD4T.Extensions.DynamicDelivery.ContentModel.Factories
{
    public interface IPageFactory
    {
        bool TryFindPage(string url, out IPage page);
        IPage FindPage(string url);
        bool TryGetPage(string tcmUri, out IPage page);
        IPage GetPage(string tcmUri);
        bool TryFindPageContent(string url, out string pageContent);
        string FindPageContent(string url);
        bool TryGetPageContent(string tcmUri, out string pageContent);
        string GetPageContent(string tcmUri);
        bool HasPageChanged(string url);
        DateTime GetLastPublishedDateByUrl(string url);
		DateTime GetLastPublishedDateByUri(string url);
        string[] GetAllPublishedPageUrls(string[] includeExtensions, string[] pathStarts);
    }
}
