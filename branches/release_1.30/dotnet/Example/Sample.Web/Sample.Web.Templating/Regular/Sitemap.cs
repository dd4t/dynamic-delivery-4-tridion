using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.ContentManagement;
using Sample.Web.Templating.ExtensionMethods;
using Sample.Web.Templating.Base.Regular;

namespace Sample.Web.Templating.Regular
{
    public class Sitemap : ExtendibleTemplate
    {
        readonly Regex _re = new Regex("/+");
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
            AddNodes(_sitemap.DocumentElement, publication.RootStructureGroup, "");

            this.CreateStringItem("Output", _sitemap.OuterXml, ContentType.Xml);

        }

        private void AddNodes(XmlElement context, StructureGroup sg, string baseUrl)
        {
            XmlElement sgElmt = CreateElement(context, sg.Id, CreateUrl(baseUrl,sg.Directory), sg.Title, sg.Title);
            Logger.Debug("started AddNodes for sg " + sg.Title + ", with calculated url " + CreateUrl(baseUrl,sg.Directory));
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
                        sgElmt.SetAttribute("pageId", p.Id);
                    }
                    else
                    {
                        XmlElement pageElmt = CreateElement(sgElmt, p.Id, url, p.Title, p.Title);
                        pageElmt.SetAttribute("pageId", p.Id);
                    }
                }
                else if (item is StructureGroup)
                {
                    AddNodes(sgElmt, (StructureGroup)item, CreateUrl(baseUrl, sg.Directory));
                }
            }
        }

        private string CreateUrl(params string[] segments)
        {

            StringBuilder sb = new StringBuilder();
            foreach (string s in segments)
            {
                sb.Append("/" + s);
            }
            return _re.Replace(sb.ToString(), "/");
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
}
