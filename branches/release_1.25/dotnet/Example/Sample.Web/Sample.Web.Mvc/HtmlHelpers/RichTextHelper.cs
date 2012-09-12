using System;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using DD4T.ContentModel;
using DD4T.Factories;
using System.Collections.Generic;

namespace Sample.Web.Mvc.HtmlHelpers
{
    public static class RichTextHelper
    {
        /// <summary>
        /// Extension method on String to resolve rich text. 
        /// Use as: Model.Field["key"].Value.ResolveRichText()
        /// </summary>
        /// <param name="value"></param>
        /// <returns>MvcHtmlString (resolved rich text)</returns>
        public static MvcHtmlString ResolveRichText(this String value)
        {
            LinkFactory linkFactory = new LinkFactory();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(string.Format("<xhtml>{0}</xhtml>", value));
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
            nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");

            // resolve links which haven't been resolved
            foreach (XmlNode link in doc.SelectNodes("//xhtml:a[@xlink:href[starts-with(string(.),'tcm:')]][@xhtml:href='' or not(@xhtml:href)]", nsmgr))
            {
                string tcmuri = link.Attributes["xlink:href"].Value;
                string linkUrl = linkFactory.ResolveLink(tcmuri);
                if (!string.IsNullOrEmpty(linkUrl))
                {
                    // add href
                    XmlAttribute href = doc.CreateAttribute("xhtml:href");
                    href.Value = linkUrl;
                    link.Attributes.Append(href);

                    // remove all xlink attributes
                    foreach (XmlAttribute xlinkAttr in link.SelectNodes("//@xlink:*", nsmgr))
                    {
                        link.Attributes.Remove(xlinkAttr);
                    }
                }
                else
                {
                    // copy child nodes of link so we keep them
                    foreach (XmlNode child in link.ChildNodes)
                    {
                        link.ParentNode.InsertBefore(child.CloneNode(true), link);
                    }
                    // remove link node
                    link.ParentNode.RemoveChild(link);
                }
            }

            return new MvcHtmlString(doc.DocumentElement.InnerXml);
        }

        /// <summary>
        /// Extension method on Model.Field to resolve rich text for a specified index.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="index"></param>
        /// <returns>MvcHtmlString (resolved rich text)</returns>
        public static MvcHtmlString ResolveRichText(this IField field, int index)
        {
            if (field.FieldType == FieldType.Xhtml)
            {
                return ResolveRichText(field.Values[index]);
            }

            return new MvcHtmlString(".ResolveRichText() only works on rich text fields...");
        }

        /// <summary>
        /// Extension method on Model.Field to resolve rich text for the first value of the field.
        /// </summary>
        /// <param name="field"></param>
        /// <returns>MvcHtmlString (resolved rich text)</returns>
        public static MvcHtmlString ResolveRichText(this IField field)
        {
            return ResolveRichText(field, 0);
        }
    }
}
