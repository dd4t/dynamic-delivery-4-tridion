using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Web.Security;
using System.Collections.Specialized;
using Microsoft.Practices.Unity;
using DD4T.ContentModel;
using DD4T.Mvc.Providers;
using DD4T.Factories;
using DD4T.ContentModel.Factories;
using DD4T.Examples.Unity;
using DD4T.Examples.Utils;
using DD4T.Utils;
using ConfigurationException = DD4T.ContentModel.Exceptions.ConfigurationException;
using System.Text.RegularExpressions;

namespace DD4T.Examples.Helpers
{
    public enum MenuType { Top, Left, Full, BreadCrumbs }
    public static class SiteMapMenuHelper
    {

        #region properties
        private static ILinkFactory _linkFactory = null;
        private static ILinkFactory LinkFactory
        {
            get
            {
                if (_linkFactory == null)
                    _linkFactory = UnityHelper.Container.Resolve<ILinkFactory>();

                return _linkFactory;
            }
        }
        private static TridionSiteMapProvider _siteMapProvider = null;
        private static SiteMapProvider CurrentSiteMapProvider
        {
            get
            {
                if (_siteMapProvider == null)
                {
                    _siteMapProvider = new TridionSiteMapProvider();
                    _siteMapProvider.PageFactory = UnityHelper.Container.Resolve<IPageFactory>();
                }
                return _siteMapProvider;
            }
        }

        private static string _rootType = null;
        private static string RootType
        {
            get
            {
                if (_rootType == null)
                    _rootType = ConfigurationManager.AppSettings["DD4T.Navigation.Root"];
                return _rootType;
            }
        }
        readonly static Regex _reIsStructureGroup = new Regex("-4$");


#endregion

        #region extension methods
        public static HtmlString SiteMapMenu(this HtmlHelper helper, IPage model, MenuType menutype)
        {
            switch (menutype)
            {
                case MenuType.Top:
                    return CreateTopMenu(FindRootNode(CurrentSiteMapProvider.RootNode), helper.ViewContext, model);
                case MenuType.Left:
                    return CreateLeftMenu(FindRootNode(CurrentSiteMapProvider.RootNode), helper.ViewContext, model);
                case MenuType.BreadCrumbs:
                    return CreateBreadCrumbs(FindRootNode(CurrentSiteMapProvider.RootNode), helper.ViewContext, model);
                case MenuType.Full:
                    return CreateFullSiteMap(FindRootNode(CurrentSiteMapProvider.RootNode), helper.ViewContext, model);
            }

            return new HtmlString(String.Empty);
        }

        #endregion

        #region menu generation methods

        /// <summary>
        /// Creates the 4 'static' links: SUPPORT, DOWNLOADS, CONTACT US, MY SDL
        /// </summary>
        private static HtmlString CreateTopMenu(SiteMapNode siteMapNode, ViewContext viewContext, IPage model)
        {
            // Only render when node has childnodes
            if (!siteMapNode.HasChildNodes)
            {
                return null;
            }

            var homeNode = (TridionSiteMapNode) siteMapNode.ChildNodes[0];

            var result = new StringBuilder();

            foreach (TridionSiteMapNode childNode in homeNode.ChildNodes)
            {
                if (_reIsStructureGroup.IsMatch(childNode.Key))
                    AddMenuItem(result, childNode, null, null);
            }

            var tbNavUl = new TagBuilder("ul");
            tbNavUl.InnerHtml = result.ToString();
            return new HtmlString(tbNavUl.ToString());
        }

        private static HtmlString CreateLeftMenu(SiteMapNode siteMapNode, ViewContext viewContext, IPage model)
        {
            // Only render when node has childnodes
            if (!siteMapNode.HasChildNodes)
                return null;

            SiteMapNode parentNode;

            if (!TryFindActiveNode(siteMapNode, model.Id, out parentNode))
                return new MvcHtmlString(string.Format("<!-- error: cannot generate left menu for pages with URI {0} -->", model.Id));

            var result = new StringBuilder();
            AddChildMenuItems(result, ((TridionSiteMapNode) parentNode), 2, new List<int> {4, 64});


            var navUl = new TagBuilder("ul");
            navUl.InnerHtml = result.ToString();

            var navUlTop = new TagBuilder("ul");
            var navLi = new TagBuilder("li");
            navLi.InnerHtml = parentNode.Title + navUl.ToString();
            navUlTop.InnerHtml = navLi.ToString();

            // Return unordered list
            return new HtmlString(navUlTop.ToString());
        }

        private static HtmlString CreateBreadCrumbs(SiteMapNode siteMapNode, ViewContext viewContext, IPage model)
        {
            // Only render when node has childnodes
            if (!siteMapNode.HasChildNodes)
                return null;

            List<TridionSiteMapNode> trail = GenerateBreadCrumb(siteMapNode, model);

            if (trail.Count == 0)
                return new MvcHtmlString(string.Format("<!-- error: cannot generate bread crumbs for page {0} -->", model.Id));

            TagBuilder navUl = new TagBuilder("ul");
            StringBuilder listItems = new StringBuilder();
            navUl.Attributes.Add("class", "breadcrumbs");

            foreach (TridionSiteMapNode node in trail)
            {
                TagBuilder navLi = new TagBuilder("li");

                if (!string.IsNullOrEmpty(node.ResolvedUrl) && node.ResolvedUrl.EndsWith("html"))
                {
                    TagBuilder navA = new TagBuilder("a");
                    navA.Attributes.Add("href", node.ResolvedUrl.AdjustUrl());
                    navA.SetInnerText(GetTitleOrDescription(node.Title, node.Description));
                    navLi.InnerHtml = navA.ToString();
                }
                else
                    navLi.InnerHtml = GetTitleOrDescription(node.Title, node.Description);
            
                listItems.Append(navLi.ToString());
            }

            navUl.InnerHtml = listItems.ToString();
            

            // Return unordered list
            return new HtmlString(navUl.ToString());
        }

        private static HtmlString CreateFullSiteMap(SiteMapNode siteMapNode, ViewContext viewContext, IPage model)
        {
            StringBuilder result = new StringBuilder();
            AddChildMenuItems(result, ((TridionSiteMapNode) siteMapNode), 100, new List<int> {4, 64, 1024});

            TagBuilder navUl = new TagBuilder("ul");
            navUl.Attributes.Add("class", "sitemap");
            navUl.InnerHtml = result.ToString();

            // Return unordered list
            return new HtmlString(navUl.ToString());
        }

        private static List<TridionSiteMapNode> GenerateBreadCrumb(SiteMapNode rootNode, IPage model)
        {
            return GenerateBreadCrumb(rootNode, model.Id);
        }

        private static List<TridionSiteMapNode> GenerateBreadCrumb(SiteMapNode rootNode, string uri)
        {
            SiteMapNode currentNode;

            if (TryFindActiveNode(rootNode, uri, out currentNode))
            {
                List<TridionSiteMapNode> path = new List<TridionSiteMapNode>();

                while (currentNode.Title != "Navigation" && !string.IsNullOrEmpty(currentNode.Key))
                {
                    path.Add((TridionSiteMapNode)currentNode);                        
                    currentNode = currentNode.ParentNode;
                }

                path.Reverse();
                
                return path;
            }

            return new List<TridionSiteMapNode>();
        }

        private static SiteMapNode FindRootNode(SiteMapNode topLevelNode)
        {
            if (string.IsNullOrEmpty(RootType))
            {
                return topLevelNode;
            }
            foreach (SiteMapNode child in topLevelNode.ChildNodes)
            {
                if (child.Title.Equals(RootType))
                    return child;
            }
            return topLevelNode;
        }

        #endregion

        #region private helpers
        
        private static void AddChildMenuItems(StringBuilder sb, TridionSiteMapNode node, int deep, List<int> itemTypes)
        {
            // Iterate through childnodes
            if (!node.HasChildNodes)
                return;

            foreach (TridionSiteMapNode childNode in node.ChildNodes)
            {
                if (itemTypes.Contains(new TcmUri(childNode.Key).ItemTypeId))
                {
                    if (childNode.Url.Contains(ConfigurationManager.AppSettings["DD4T.DefaultPage"]))
                        continue;

                    AddMenuItem(sb, childNode, null, null);
                    if (deep > 0 && childNode.HasChildNodes)
                    {
                        var liNodes = new StringBuilder();
                        AddChildMenuItems(liNodes, childNode, deep - 1, itemTypes);
                        if (liNodes.Length > 0)
                        {
                            var ul = new TagBuilder("ul");
                            ul.InnerHtml = liNodes.ToString();
                            sb.Append(ul.ToString());
                        }
                    }
                }
            }
        }

        private static void AddMenuItem(StringBuilder sb, TridionSiteMapNode node, string submenu)
        {
            AddMenuItem(sb, node, submenu, null);
        }

        private static void AddMenuItem(StringBuilder sb, TridionSiteMapNode node, string submenu, string cssClass)
        {
            var li = new TagBuilder("li");
            var a = new TagBuilder("a");

            var span = new TagBuilder("span");
            span.Attributes.Add("class", cssClass);

            var span2 = new TagBuilder("span");
            span2.Attributes.Add("class", "arrow icon");

            span.InnerHtml = node.Title;

            if (cssClass != null)
                a.InnerHtml = span + span2.ToString();

            string href = null;
            if (node.Attributes["RedirectUrl"] != null)
            {
                href = node.Attributes["RedirectUrl"];
            }
            else
            {
                href = RootType.ToLower().Equals("structure") ? General.AdjustUrlToContext(node.Url) : General.AdjustUrlToContext(node.ResolvedUrl);
            }

            a.Attributes.Add("href", href);

            if (cssClass == null)
                a.InnerHtml = node.Title;

            if (submenu == null)
            {
                li.InnerHtml = a.ToString();
            }
            else
            {
                li.InnerHtml = a + submenu;
            }

            sb.AppendLine(li.ToString());
        }

        private static TridionSiteMapNode FindMainParentNode(TridionSiteMapNode childnode)
        {
            if (childnode == null)
                return null;

            TridionSiteMapNode currentNode = childnode;
            while (currentNode.ParentNode != null && currentNode.Level > 2)
            {
                currentNode = (TridionSiteMapNode)currentNode.ParentNode;
            }

            return currentNode;
        }

        private static bool TryFindActiveNode(SiteMapNode mainnode, string key, out SiteMapNode result)
        {
            TridionSiteMapProvider provider = (TridionSiteMapProvider)mainnode.Provider;
            SiteMapNode current = null;
            if (string.IsNullOrEmpty(key) || !provider.NodeDictionary.TryGetValue(key, out current))
            {
                LoggerService.Warning("SiteMapNode '{0}' not found in TryFindActiveNode.", key);
                result = null;
                return false;
            }

            TridionSiteMapNode currentT = (TridionSiteMapNode)current;
            if (currentT.Level < 3) // we are in the Home sg, no way to find out what the left menu should be
            {
                result = null;
                return false;
            }

            while (currentT.Level > 3)
                currentT = currentT.ParentNode as TridionSiteMapNode;
            result = currentT;
            return true;
        }

        private static string GetSitemapNodeKey(IPage model)
        {
            if (model != null && model.ComponentPresentations != null && model.ComponentPresentations.Count > 0)
            {
                for (int i = 0; i < model.ComponentPresentations.Count; i++)
                {
                    if (model.ComponentPresentations[i].Component.Schema.Title.StartsWith("Header"))
                    {
                        IFieldSet meta = model.ComponentPresentations[i].Component.MetadataFields;
                        foreach (var j in meta)
                        {
                            if (j.Value.FieldType == FieldType.Keyword)
                                return string.Format("{0}::{1}", j.Value.CategoryName, j.Value.Value);
                        }

                        break;
                    }
                }
            }

            return null;
        }

        private static string GetTitleOrDescription(string title, string description)
        {
            if (!string.IsNullOrEmpty(description))
                return description;

            if (!string.IsNullOrEmpty(title))
                return title;

            return "";
        }

        #endregion
    }
}
