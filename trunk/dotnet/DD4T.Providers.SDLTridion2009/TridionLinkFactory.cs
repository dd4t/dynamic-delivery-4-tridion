using DD4T.ContentModel.Factories;

namespace DD4T.Factories
{
    using System.ComponentModel.Composition;

    [Export(typeof(ILinkFactory))]
    public class TridionLinkFactory : ILinkFactory
    {

        public string ResolveLink(string componentUri)
        {
            throw new System.NotImplementedException();
        }

        public string ResolveLink(string sourcePageUri, string componentUri, string excludeComponentTemplateUri)
        {
            throw new System.NotImplementedException();
        }
    }
}

