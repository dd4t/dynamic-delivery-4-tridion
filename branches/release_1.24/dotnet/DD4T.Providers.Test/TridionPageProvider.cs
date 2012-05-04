using System;
using System.Xml;
using System.Xml.Serialization;
using DD4T.ContentModel.Contracts.Providers;
using System.IO;
using System.Collections.Generic;
using DD4T.ContentModel;
using System.Text;

namespace DD4T.Providers.Test
{
    /// <summary>
    /// 
    /// </summary>
    public class TridionPageProvider : BaseProvider, IPageProvider
    {


        #region IPageProvider Members

        /// <summary>
        /// Get all urls of published pages
        /// </summary>
        /// <param name="includeExtensions"></param>
        /// <param name="pathStarts"></param>
        /// <param name="publicationID"></param>
        /// <returns></returns>
        public string[] GetAllPublishedPageUrls(string[] includeExtensions, string[] pathStarts)
        {
            List<string> testUrls = new List<string>() { "/test/one.html", "/test/two.html" };
            return testUrls.ToArray();
        }


        /// <summary>
        /// Gets the raw string (xml) from the broker db by URL
        /// </summary>
        /// <param name="Url">URL of the page</param>
        /// <returns>String with page xml or empty string if no page was found</returns>
        public string GetContentByUrl(string Url)
        {
            Page page = new Page();
            page.Title = Randomizer.AnyString(15);
            page.Id = Randomizer.AnyUri(64);
            page.Filename = Randomizer.AnySafeString(8) + ".html";

            PageTemplate pt = new PageTemplate();
            pt.Title = Randomizer.AnyString(20);
            Field ptfieldView = new Field();
            ptfieldView.Name = "view";
            ptfieldView.Values.Add("Standard");
            pt.MetadataFields = new FieldSet();
            pt.MetadataFields.Add(ptfieldView.Name, ptfieldView);

            page.PageTemplate = pt;

            Schema schema = new Schema();
            schema.Title = Randomizer.AnyString(10);

            Component component = new Component();
            component.Title = Randomizer.AnyString(30);
            component.Id = Randomizer.AnyUri(16);
            component.Schema = schema;

            Field field1 = Randomizer.AnyTextField(6, 120, true);
            Field field2 = Randomizer.AnyTextField(8, 40, false);

            FieldSet fieldSet = new FieldSet();
            fieldSet.Add(field1.Name, field1);
            fieldSet.Add(field2.Name, field2);
            component.Fields = fieldSet;

            ComponentTemplate ct = new ComponentTemplate();
            ct.Title = Randomizer.AnyString(20);
            Field fieldView = new Field();
            fieldView.Name = "view";
            fieldView.Values.Add("DefaultComponentView");
            ct.MetadataFields = new FieldSet();
            ct.MetadataFields.Add(fieldView.Name, fieldView);

            ComponentPresentation cp = new ComponentPresentation();
            cp.Component = component;
            cp.ComponentTemplate = ct;

            page.ComponentPresentations = new List<ComponentPresentation>();
            page.ComponentPresentations.Add(cp);

            FieldSet metadataFields = new FieldSet();
            page.MetadataFields = metadataFields;

            var serializer = new XmlSerializer(typeof(Page));
            StringBuilder builder = new StringBuilder();
            StringWriter writer = new StringWriter(builder);
            //XmlTextWriter writer = new XmlTextWriter(page.Filename, Encoding.UTF8);
            //serializer.Serialize(writer, page);
            serializer.Serialize(writer, page);
            string pageAsString = builder.ToString();
            return pageAsString;
        }


        /// <summary>
        /// Gets the raw string (xml) from the broker db by URI
        /// </summary>
        /// <param name="Url">TCM URI of the page</param>
        /// <returns>String with page xml or empty string if no page was found</returns>
        public string GetContentByUri(string TcmUri)
        {
            throw new NotImplementedException();
        }


        public DateTime GetLastPublishedDateByUrl(string url)
        {
            return DateTime.Now;
        }

		public DateTime GetLastPublishedDateByUri(string uri) {
            return DateTime.Now;
        }
        #endregion

        public void Dispose()
        {
        }
    }
}
