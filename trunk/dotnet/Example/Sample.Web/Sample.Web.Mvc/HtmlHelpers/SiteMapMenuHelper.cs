using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Sample.Web.Mvc.Contracts;
using DD4T.ContentModel;
using DD4T.Mvc.Providers;
using System.Text;


namespace Sample.Web.Mvc.HtmlHelpers
{
    public static class SiteMapMenuHelper
    {

        #region extension methods
        public static HtmlString SiteMapMenu(this HtmlHelper helper, SiteMapNode rootNode, IPage model, MenuType menutype)
        {
            HtmlString htmlReturn = new HtmlString(String.Empty);
            switch (menutype)
            {
                case MenuType.TopMenu:
                    htmlReturn = CreateTopMenu(rootNode, helper.ViewContext, model);
                    break;
                case MenuType.LeftMenu:
                    htmlReturn = CreateLeftMenu(rootNode, helper.ViewContext, model);
                    break;
            }

            return htmlReturn;
        }
        #endregion

        #region menu generation methods
        private static HtmlString CreateTopMenu(SiteMapNode siteMapNode, ViewContext viewContext, IPage model)
        {
       
            // Only render when node has childnodes
            if (!siteMapNode.HasChildNodes)
            {
                return null;
            }

           // write out the Home node separately (in the sitemap, it is on a level by its own, but in the html it is a sibling of the first-level nodes below it)
            TridionSiteMapNode homeNode = (TridionSiteMapNode) siteMapNode.ChildNodes[0];
            StringBuilder result = new StringBuilder();
            AddMenuItem(result, homeNode, null);
            AddChildMenuItems(result, homeNode, 0);
            TagBuilder navUl = new TagBuilder("ul");
            navUl.Attributes.Add("class", "top-navigation");
            navUl.InnerHtml = result.ToString();
            // Return unordered list
            return new HtmlString(navUl.ToString());
        }


        private static HtmlString CreateLeftMenu(SiteMapNode siteMapNode, ViewContext viewContext, IPage model)
        {

            // Only render when node has childnodes
            if (!siteMapNode.HasChildNodes)
            {
                return null;
            }

            SiteMapNode parentNode;
            TridionSiteMapNode parentTridionNode;
            TridionSiteMapNode startingNode = null;

            string structureGroupId = model.StructureGroup.Id;
            if (SiteMapHelper.TryFindActiveNode(siteMapNode, structureGroupId, out parentNode))
            {
                parentTridionNode = (TridionSiteMapNode)parentNode;
                if (parentTridionNode.Level > 2)
                {
                    startingNode = FindMainParentNode(parentTridionNode);
                }
                else if (parentTridionNode.Level == 2)
                {
                    startingNode = parentTridionNode;
                }
                else
                {
                    return new MvcHtmlString(string.Format("<!-- error: cannot generate left menu for pages on level {0} -->", parentTridionNode.Level));
                }
            }
            else
            {
                return null;
            }

            StringBuilder result = new StringBuilder();
            AddChildMenuItems(result, startingNode, 2);
            TagBuilder navUl = new TagBuilder("ul");
            navUl.Attributes.Add("class", "left-navigation");
            navUl.InnerHtml = result.ToString();
            // Return unordered list
            return new HtmlString(navUl.ToString());
        }


        #endregion

        #region private helpers
        private static void AddChildMenuItems(StringBuilder sb, TridionSiteMapNode node, int deep)
        {
            // Iterate through childnodes
            if (!node.HasChildNodes)
                return;

            foreach (TridionSiteMapNode childNode in node.ChildNodes)
            {
                StringBuilder liNodes = new StringBuilder();
                if (deep > 0 && childNode.HasChildNodes)
                {
                    TagBuilder ul = new TagBuilder("ul");
                    AddChildMenuItems(liNodes, childNode, deep - 1);
                    ul.InnerHtml = liNodes.ToString();
                    AddMenuItem(sb, childNode, ul.ToString());
                }
                else
                {
                    AddMenuItem(sb, childNode, null);
                }
            }

        }

        private static void AddMenuItem(StringBuilder sb, TridionSiteMapNode node, string submenu)
        {
            TagBuilder li = new TagBuilder("li");
            TagBuilder a = new TagBuilder("a");
            a.Attributes.Add("href", node.ResolvedUrl);
            a.InnerHtml = node.Title;

            if (submenu == null)
            {
                li.InnerHtml = a.ToString();
            }
            else
            {
                li.InnerHtml = a.ToString() + submenu;
            }
            sb.Append(li.ToString());
        }
        private static TridionSiteMapNode FindMainParentNode(TridionSiteMapNode childnode)
        {
            if (childnode == null)
            {
                return null;
            }

            TridionSiteMapNode currentNode = childnode;
            while (currentNode.ParentNode != null && currentNode.Level > 2)
            {
                currentNode = (TridionSiteMapNode)currentNode.ParentNode;
            }

            return currentNode;
        }

        #endregion
    }
}
