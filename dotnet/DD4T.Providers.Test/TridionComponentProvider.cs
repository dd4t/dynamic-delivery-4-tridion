using System;
using System.Collections.Generic;
using DD4T.ContentModel.Querying;
using DD4T.ContentModel.Contracts.Providers;

namespace DD4T.Providers.Test
{
    /// <summary>
    /// 
    /// </summary>
    public class TridionComponentProvider : BaseProvider, IComponentProvider
    {

        public string GetContent(string uri, string templateUri = "")
        {            
            return string.Empty;
        }

        /// <summary>
        /// Returns the Component contents which could be found. Components that couldn't be found don't appear in the list. 
        /// </summary>
        /// <param name="componentUris"></param>
        /// <returns></returns>
        public List<string> GetContentMultiple(string[] componentUris)
        {
            throw new NotImplementedException();
        }

        public IList<string> FindComponents(IQuery query)
        {
            throw new NotImplementedException();
        }
        public DateTime GetLastPublishedDate(string uri)
        {
            throw new NotImplementedException();
        }
    }
}
