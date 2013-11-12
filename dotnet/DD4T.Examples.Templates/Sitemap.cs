using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager;
using DD4T.Templates.Base.Utils;
using DD4T.Examples.Templates.ExtensionMethods;
using System.Web;

namespace DD4T.Examples.Templating
{
    [TcmTemplateTitle("Sitemap")]
    public class Sitemap : ExtendibleTemplate
    {
        readonly Regex _reRemoveDoubleSlashes = new Regex("/+");
        readonly Regex _reIsNavigationItem = new Regex("^[0-9]+ ");
        XmlDocument _sitemap = null;

        public override void Transform(Tridion.ContentManager.Templating.Engine engine, Tridion.ContentManager.Templating.Package package)
        {
            this.Initialize(engine, package);

            if (!this.IsPageTemplate())
            {
                this.Logger.Warning("Calling Sitemap from a Component Template instead of a Page Template!");
                return;
            }

            Publication publication = this.GetPublication();
            _sitemap = new XmlDocument();
            _sitemap.LoadXml("<siteMap xmlns=\"http://schemas.microsoft.com/AspNet/SiteMap-File-1.0\" />");
            
            string baseUrl = publication.PublicationUrl.Substring(1);

            XmlElement navigation = CreateElement(_sitemap.DocumentElement, "navigation", "", "Navigation", "");

            foreach (Category c in publication.GetCategories())
            {
                if (c.Title == "Navigation") // TODO: make configurable in appSettings
                {
                    foreach (Keyword kw in c.GetKeywords(new KeywordsFilter(Engine.GetSession()) { IsRoot = true }))
                        AddNodes(navigation, kw, c.Title, baseUrl);

                    break;
                }
            }

            XmlElement structure = CreateElement(_sitemap.DocumentElement, "structure", "", "Structure", "");
            AddNodes(structure, publication.RootStructureGroup, baseUrl);

            this.CreateStringItem("Output", _sitemap.OuterXml, ContentType.Xml);
        }

        private void AddNodes(XmlElement context, StructureGroup sg, string baseUrl)
        {

            XmlElement sgElmt = CreateElement(context, sg.Id, CreateUrl(baseUrl, sg.Directory), StripPrefix(sg.Title), sg.Title);
            Logger.Debug("started AddNodes for sg " + sg.Title + ", with calculated url " + CreateUrl(baseUrl, sg.Directory));

            foreach (RepositoryLocalObject item in sg.GetItems())
            {
                string url;

                if (item is Page)
                {
                    Page p = (Page)item;
                    url = CreateUrl(baseUrl, sg.Directory, p.FileName + "." + p.PageTemplate.FileExtension);

                    if (p.FileName.Equals("index"))
                    {
                        // this is the index page of the current structure group
                        sgElmt.SetAttribute("url", url);
                        sgElmt.SetAttribute("uri", p.Id);
                    }
                    else
                    {
                        XmlElement pageElmt = CreateElement(sgElmt, p.Id, url, p.Title, p.Title);
                        pageElmt.SetAttribute("id", p.Id);
                    }
                }
                else
                {
                    if (item is StructureGroup)
                    {
                        Logger.Debug("found child with title " + item.Title);
                        if (!_reIsNavigationItem.IsMatch(item.Title)) // only include structure groups with a title starting with a number
                            continue;
                        AddNodes(sgElmt, (StructureGroup)item, CreateUrl(baseUrl, sg.Directory));
                    }
                }
            }
        }

        private Category GetTaxonomyByName(string taxonomyName)
        {
            Publication p = this.GetPublication();
            XmlElement list = p.GetListCategories();
            foreach (XmlElement item in list.SelectNodes("/*/*"))
            {
                if (item.GetAttribute("Title").Equals(taxonomyName))
                {
                    return (Category) Engine.GetObject(item.GetAttribute("ID"));
                }
            }
            throw new InvalidDataException(string.Format("Category {0} does not exist", taxonomyName));
        }

        private void AddNodes(XmlElement context, Keyword kw, string categoryName, string baseUrl)
        {
            //exclude keyword from site map
            XmlNodeList nd = null;
            if (kw.MetadataSchema != null && kw.Metadata != null)
            {
                nd = kw.Metadata.SelectNodes("*");

                foreach (XmlNode n in nd)
                {
                    if (n.Name == "ExcludeFromNavigation" && n.InnerText == "Yes")
                        return;
                }
            }

            XmlElement kwElmt = CreateElement(context, kw, categoryName); // TODO: special CreateElement for keywords, with image, summary, etc
            Logger.Debug("started AddNodes for keyword " + kw.Title);

            if (nd != null && nd.Count > 0)
            {
                string followTaxonomy = null;
                ItemFields kwFields = new ItemFields(kw.Metadata, kw.MetadataSchema);

                foreach (XmlNode n in nd)
                {
                        if (n.Name == "FollowTaxonomy")
                            followTaxonomy = ConvertUriToPublication(new TcmUri(n.InnerText), kw.Id);
                        else
                        {
                            if (kwFields[n.Name] is MultimediaLinkField) 
                                kwElmt.SetAttribute(n.Name, new BinaryPublisher(Package, Engine).PublishMultimediaComponent(((MultimediaLinkField)kwFields[n.Name]).Value.Id));
                            else if (kwFields[n.Name] is ComponentLinkField) 
                                kwElmt.SetAttribute(n.Name, (((ComponentLinkField)kwFields[n.Name]).Value.Id));
                            else
                                kwElmt.SetAttribute(n.Name, HttpUtility.HtmlEncode(n.InnerText));
                        }
                }

                if (!string.IsNullOrEmpty(followTaxonomy))
                {
                    Category ft = Engine.GetObject(followTaxonomy) as Category;
                    if (ft != null)
                    {
                        foreach (Keyword k in ft.GetKeywords(new KeywordsFilter(Engine.GetSession()) { IsRoot = true }))
                            AddNodes(kwElmt, k, ft.Title, "");
                    }
                }
                else
                {
                    foreach (Keyword k in kw.GetChildKeywords())
                        AddNodes(kwElmt, k, categoryName, "");
                }
            }
            else
            {
                foreach (Keyword k in kw.GetChildKeywords())
                    AddNodes(kwElmt, k, categoryName, "");
            }
        }

        private string StripPrefix(string title)
        {
            return _reIsNavigationItem.Replace(title, "");
        }

        private string CreateUrl(params string[] segments)
        {
            StringBuilder sb = new StringBuilder();
            
            foreach (string s in segments)
                sb.Append("/" + s);

            return _reRemoveDoubleSlashes.Replace(sb.ToString(), "/");
        }

        private XmlElement CreateElement(XmlElement context, Keyword kw, string categoryName)
        {
            XmlElement result = context.OwnerDocument.CreateElement("siteMapNode");

            result.SetAttribute("id", string.Format("{0}::{1}", categoryName, kw.Title));
            result.SetAttribute("url", kw.Id.ToString());
            result.SetAttribute("title", StripPrefix(kw.Title));
            result.SetAttribute("description", kw.Description);
            result.SetAttribute("keywordUri", kw.Id);

            Component c = FindComponent(kw);

            if (c != null)
                result.SetAttribute("uri", ConvertUriToPublication(c.Id, kw.Id));
           
            context.AppendChild(result);
            
            return result;
        }

        private Component FindComponent(Keyword kw)
        {
            XmlElement usingItems = kw.GetListUsingItems(new UsingItemsFilter(Engine.GetSession()) { ItemTypes = new List<ItemType>() { ItemType.Component }, IncludedVersions = VersionCondition.OnlyLatestVersions });

            Component component = null;

            List<Component> matchingComponents = new List<Component>();
            List<Component> otherComponents = new List<Component>();

            if (kw.Title.Contains("Web Content Management"))
                Logger.Debug(string.Format("looking for component for keyword {0}", kw.Title));
            foreach (XmlElement item in usingItems.SelectNodes("/*/*"))
            {
                Component c = (Component)Engine.GetObject(item.GetAttribute("ID"));
                if (kw.Title.Contains("Web Content Management"))
                    Logger.Debug(string.Format("round 1: component {0}, schema title {2}", c.Title, c.Id, c.Schema.Title));

                //The component which is based on the header schema related to the current taxonomy takes precedence
                if (c.Schema.Title.ToLower().Contains(kw.OrganizationalItem.Title.ToLower())) // e.g. if the keyword is in the category 'Product', a component with schema 'Header For Product' matches!
                    matchingComponents.Add(c);
                else
                    //The first one that is returned by the system takes precedence
                    otherComponents.Add(c);
            }

            if (matchingComponents.Count == 0 && otherComponents.Count == 0)
            {
                if (kw.Title.Contains("Web Content Management"))
                    Logger.Debug("no components found, returning null");
                return null;
            }

            if (TryFindComponentWithinMatchingComponents(matchingComponents, kw, out component))
                return component;

            matchingComponents.Clear();

            foreach (Component c in otherComponents)
            {
                if (kw.Title.Contains("Web Content Management"))
                    Logger.Debug(string.Format("round 2: component {0}, schema {1}", c.Title, c.Schema.Title)); 

                //The component which is based on any other header schema takes precedence
                if (c.Schema.Title.StartsWith("Header")) // TODO: make configurable
                    matchingComponents.Add(c);

            }

            if (TryFindComponentWithinMatchingComponents(matchingComponents, kw, out component))
                return component;

            if (kw.Title.Contains("Web Content Management"))
                Logger.Debug(string.Format("no rules matched, returning first component in list ({0})", otherComponents[0].Title));

            if (TryFindComponentWithinMatchingComponents(otherComponents, kw, out component))
                return component;




            return null;
        }

        private bool TryFindComponentWithinMatchingComponents(List<Component> matchingComponents, Keyword kw, out Component component)
        {
            component = null;
            if (matchingComponents.Count == 0)
                return false;

            if (matchingComponents.Count == 1)
            {
                component = matchingComponents[0];
                return true;
            }


            // final rule: find the component whose title resembles the keyword title most closely!
            // this uses an algorythm that calculates the Levenshtein distance
            // see http://en.wikipedia.org/wiki/Levenshtein_distance

            int lowestLD = Int32.MaxValue;
            Component closestComponent = null;
            Logger.Debug(string.Format("found > 1 component with the same schema affinity, comparing component titles to keyword title {0}", kw.Title));
            foreach (Component c in matchingComponents)
            {
                int ld = Distance.LD(c.Title, kw.Title);
                Logger.Debug(string.Format("component title {0} has Levenshtein distance {1}", c.Title, ld));
                if (ld < lowestLD)
                {
                    lowestLD = ld;
                    closestComponent = c;
                }
            }
            if (closestComponent == null)
            {
                Logger.Warning("unexpected problem: we could not find a matching component based on title");
                return false;
            }

            component = closestComponent;
            Logger.Debug(string.Format("found component with title {0} and id {1}", component.Title, component.Id));
            return true;
        }

        private bool TitlesAreSimilar(string a, string b)
        {
            // strip prefixes
            a = _reIsNavigationItem.Replace(a, "");
            b = _reIsNavigationItem.Replace(b, "");
            a = new Regex(" ").Replace(a, "");
            b = new Regex(" ").Replace(b, "");
            a = a.ToLower();
            b = b.ToLower();
            return a.Equals(b);
        }

        private string ConvertUriToPublication(TcmUri itemUri, TcmUri scopeUri)
        {
            if (scopeUri.ItemType == ItemType.Publication)
                return new TcmUri(itemUri.ItemId, itemUri.ItemType, scopeUri.ItemId);
            
            return new TcmUri(itemUri.ItemId, itemUri.ItemType, scopeUri.PublicationId);
        }

        private static XmlElement CreateElement(XmlElement context, string id, string url, string title, string description)
        {
            XmlElement result = context.OwnerDocument.CreateElement("siteMapNode");
            
            result.SetAttribute("id", id);
            result.SetAttribute("url", url);
            result.SetAttribute("title", title);
            result.SetAttribute("description", description);
            context.AppendChild(result);
            
            return result;
        }
    }

    public static class Distance
    {

        /// <summary>
        /// Compute Levenshtein distance
        /// </summary>
        /// <param name="s">String 1</param>
        /// <param name="t">String 2</param>
        /// <returns>Distance between the two strings.
        /// The larger the number, the bigger the difference.
        /// </returns>

        public static int LD(string s, string t)
        {
            int n = s.Length; //length of s
            int m = t.Length; //length of t

            int[,] d = new int[n + 1, m + 1]; // matrix

            int cost; // cost

            // Step 1

            if (n == 0) return m;
            if (m == 0) return n;

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++);
            for (int j = 0; j <= m; d[0, j] = j++);

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    cost = (t.Substring(j - 1, 1) == s.Substring(i - 1, 1) ? 0 : 1);
                    // Step 6
                    d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                              d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }

}
