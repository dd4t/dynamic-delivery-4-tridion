using System;
namespace DD4T.ContentModel.Factories
{
    public interface ILinkFactory
    {
        string ResolveLink(string componentUri);
        string ResolveLink(string sourcePageUri, string componentUri, string excludeComponentTemplateUri);
    }
}
