using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.Extensions.DynamicDelivery.ContentModel;
using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;
using System.ComponentModel.Composition;

namespace Tridion.Extensions.DynamicDelivery.Factories.LocalDatabase
{
    [Export(typeof(IPageFactory))]
    public class PageFactory : IPageFactory
    {
        #region IPageFactory Members

        public bool TryFindPage(string url, out IPage page)
        {
            throw new NotImplementedException();
        }

        public IPage FindPage(string url)
        {
            throw new NotImplementedException();
        }

        public bool TryGetPage(string tcmUri, out IPage page)
        {
            throw new NotImplementedException();
        }

        public IPage GetPage(string tcmUri)
        {
            throw new NotImplementedException();
        }

        public bool TryFindPageContent(string url, out string pageContent)
        {
            throw new NotImplementedException();
        }

        public string FindPageContent(string url)
        {
            throw new NotImplementedException();
        }

        public bool TryGetPageContent(string tcmUri, out string pageContent)
        {
            throw new NotImplementedException();
        }

        public string GetPageContent(string tcmUri)
        {
            throw new NotImplementedException();
        }

        public bool HasPageChanged(string url)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastPublishedDate(string url)
        {
            throw new NotImplementedException();
        }

        public string[] GetAllPublishedPageUrls(string[] includeExtensions, string[] pathStarts)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
