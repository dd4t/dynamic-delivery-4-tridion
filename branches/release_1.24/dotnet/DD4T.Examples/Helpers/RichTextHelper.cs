using System;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Microsoft.Practices.Unity;
using DD4T.ContentModel;
using DD4T.Factories;
using DD4T.ContentModel.Contracts.Providers;
using DD4T.ContentModel.Factories;
using DD4T.Examples.Unity;
using DD4T.Examples.Utils;

namespace DD4T.Examples.Helpers
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
                    _linkFactory = UnityHelper.Container.Resolve<ILinkFactory>();
                }

                return _linkFactory;
            }
        }

        /// <summary>
        /// Extension method on String to resolve rich text. 
        /// It will strip a full width table from the rtf so it can be placed separate in the view using Model.Field["key"].Value.FullWidthTable()
        /// Use as: Model.Field["key"].Value.ResolveRichText()
        /// </summary>
        /// <param name="value">the rich text string</param>
        /// <param name="displayFullWidthTable">display full width table, default value is false</param>
        /// <returns>MvcHtmlString (resolved rich text)</returns>
        public static MvcHtmlString ResolveRichText(this string value)
        {
            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);

            nsmgr.AddNamespace("xhtml", XhtmlNamespaceUri);
            nsmgr.AddNamespace("xlink", XlinkNamespaceUri);
            doc.LoadXml(string.Format("<xhtmlroot>{0}</xhtmlroot>", value));

            // remove full width table
            foreach (XmlNode table in doc.SelectNodes("//xhtml:table[@class='full-width']", nsmgr))
            {
                table.ParentNode.RemoveChild(table);
            }
            // resolve links which haven't been resolved
            foreach (XmlNode link in doc.SelectNodes("//xhtml:a[@xlink:href[starts-with(string(.),'tcm:')]][@xhtml:href='' or not(@xhtml:href)]", nsmgr))
            {
                string tcmuri = link.Attributes["xlink:href"].Value;
                string linkUrl = LinkFactory.ResolveLink(tcmuri);

                if (!string.IsNullOrEmpty(linkUrl))
                {
                    linkUrl = General.AdjustUrlToContext(linkUrl);

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
                if (img.GetAttributeNode("src") != null)
                    img.Attributes["src"].Value = General.AdjustUrlToContext(img.Attributes["src"].Value);
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
        /// Extension method on Model.Field to resolve rich text for a specified index.
        /// </summary>
        /// <param name="field">the field</param>
        /// <param name="index">specified index of the field</param>
        /// <returns>MvcHtmlString (resolved rich text)</returns>
        public static MvcHtmlString ResolveRichText(this IField field, int index)
        {
            if (field.FieldType == FieldType.Xhtml)
                return ResolveRichText(field.Values[index]);

            // return message to view developer that this is incorrectly considered a rich text field
            return new MvcHtmlString(".ResolveRichText() only works on rich text fields...");
        }

        /// <summary>
        /// Extension method on Model.Field to resolve rich text for the first value of the field.
        /// </summary>
        /// <param name="field">the field</param>
        /// <returns>MvcHtmlString (resolved rich text)</returns>
        public static MvcHtmlString ResolveRichText(this IField field)
        {
            return ResolveRichText(field, 0);
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
