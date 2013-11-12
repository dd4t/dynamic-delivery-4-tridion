using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel.Factories;
using DD4T.Factories;
using System.Xml;
using System.Web.Mvc;

namespace DD4T.Mvc.Html
{
    public static class RichTextHelper
    {
        /// <summary>
        /// xhtml namespace uri
        /// </summary>
        private const string XhtmlNamespaceUri = "http://www.w3.org/1999/xhtml";

        /// <summary>
        /// xlink namespace uri
        /// </summary>
        private const string XlinkNamespaceUri = "http://www.w3.org/1999/xlink";

        private static ILinkFactory _linkFactory = null;
        private static ILinkFactory LinkFactory
        {
            get
            {
                if (_linkFactory == null)
                {
                    _linkFactory = new LinkFactory(); 
                }
                return _linkFactory;
            }
        }

        public static MvcHtmlString ResolveRichText(this string value)
        {
            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);

            nsmgr.AddNamespace("xhtml", XhtmlNamespaceUri);
            nsmgr.AddNamespace("xlink", XlinkNamespaceUri);
            doc.LoadXml(string.Format("<xhtmlroot>{0}</xhtmlroot>", value));
            // resolve links which haven't been resolved
            foreach (XmlNode link in doc.SelectNodes("//xhtml:a[@xlink:href[starts-with(string(.),'tcm:')]][@xhtml:href='' or not(@xhtml:href)]", nsmgr))
            {
                string tcmuri = link.Attributes["xlink:href"].Value;
                string linkUrl = LinkFactory.ResolveLink(tcmuri);

                if (!string.IsNullOrEmpty(linkUrl))
                {
                    // linkUrl = HttpHelper.AdjustUrlToContext(linkUrl);
                    // add href
                    XmlAttribute href = doc.CreateAttribute("xhtml:href");
                    href.Value = linkUrl;
                    link.Attributes.Append(href);

                    // remove all xlink attributes
                    foreach (XmlAttribute xlinkAttr in link.SelectNodes("//@xlink:*", nsmgr))
                        link.Attributes.Remove(xlinkAttr);
                }
                else
                {
                    // copy child nodes of link so we keep them
                    foreach (XmlNode child in link.ChildNodes)
                        link.ParentNode.InsertBefore(child.CloneNode(true), link);

                    // remove link node
                    link.ParentNode.RemoveChild(link);
                }
            }

            // remove any additional xlink attribute
            foreach (XmlNode node in doc.SelectNodes("//*[@xlink:*]", nsmgr))
            {
                foreach (XmlAttribute attr in node.SelectNodes("//@xlink:*", nsmgr))
                    node.Attributes.Remove(attr);
            }

            // add application context path to images
            foreach (XmlElement img in doc.SelectNodes("//*[@src]", nsmgr))
            {
                //if (img.GetAttributeNode("src") != null)
                //    img.Attributes["src"].Value = HttpHelper.AdjustUrlToContext(img.Attributes["src"].Value);
            }

            // fix empty anchors by placing the id value as a text node and adding a style attribute with position:absolute and visibility:hidden so the value won't show up
            foreach (XmlElement anchor in doc.SelectNodes("//xhtml:a[not(node())]", nsmgr))
            {
                XmlAttribute style = doc.CreateAttribute("style");
                style.Value = "position:absolute;visibility:hidden;";
                anchor.Attributes.Append(style);
                anchor.InnerText = anchor.Attributes["id"] != null ? anchor.Attributes["id"].Value : "empty";
            }

            return new MvcHtmlString(RemoveNamespaceReferences(doc.DocumentElement.InnerXml));
        }

        /// <summary>
        /// removes unwanted namespace references (like xhtml and xlink) from the html
        /// </summary>
        /// <param name="html">html as a string</param>
        /// <returns>html as a string without namespace references</returns>
        private static string RemoveNamespaceReferences(string html)
        {
            if (!string.IsNullOrEmpty(html))
            {
                html = html.Replace("xmlns=\"\"", "");
                html = html.Replace(string.Format("xmlns=\"{0}\"", XhtmlNamespaceUri), "");
                html = html.Replace(string.Format("xmlns:xhtml=\"{0}\"", XhtmlNamespaceUri), "");
                html = html.Replace(string.Format("xmlns:xlink=\"{0}\"", XlinkNamespaceUri), "");
            }

            return html;
        }
    }
}
