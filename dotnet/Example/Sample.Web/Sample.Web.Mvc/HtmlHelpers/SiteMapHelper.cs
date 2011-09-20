using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Diagnostics;
using DD4T.ContentModel;
using DD4T.Mvc.Providers;

namespace Sample.Web.Mvc.HtmlHelpers
{
    public static class SiteMapHelper
    {
        public static List<TridionSiteMapNode> GenerateBreadCrumb(SiteMapNode rootNode, IPage model)
        {
            string uri = model.StructureGroup.Id;
            HtmlString htmlReturn = new HtmlString(string.Empty);
            SiteMapNode currentNode;
            if (TryFindActiveNode(rootNode, uri, out currentNode))
            {
                List<TridionSiteMapNode> path = new List<TridionSiteMapNode>();

                while (currentNode.ParentNode != null && !String.IsNullOrEmpty(currentNode.Key))
                {
                    path.Add((TridionSiteMapNode)currentNode);
                    currentNode = currentNode.ParentNode;
                }
                path.Reverse();
                return path;
            }
            else
                return new List<TridionSiteMapNode>();
        }

        public static bool TryFindActiveNode(SiteMapNode mainnode, string structureGroupId, out SiteMapNode result)
        {
            TridionSiteMapProvider provider = (TridionSiteMapProvider)mainnode.Provider;

            if (!provider.NodeDictionary.TryGetValue(structureGroupId, out result))
            {
                Trace.TraceWarning("SiteMapNode '{0}' not found in TryFindActiveNode.", structureGroupId);
                return false;
            }
            else
                return true;
        }
    }
}