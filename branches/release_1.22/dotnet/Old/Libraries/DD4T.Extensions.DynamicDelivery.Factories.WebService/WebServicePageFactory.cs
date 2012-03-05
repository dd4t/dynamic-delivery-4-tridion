using System;
using System.ComponentModel.Composition;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Tridion.Extensions.DynamicDelivery.ContentModel;
using Tridion.Extensions.DynamicDelivery.ContentModel.Exceptions;
using Tridion.Extensions.DynamicDelivery.ContentModel.Factories;
using Tridion.Extensions.DynamicDelivery.Factories;
using Tridion.Extensions.DynamicDelivery.Factories.WebService.Tridion.Broker;

namespace Tridion.Extensions.DynamicDelivery.Factories.WebService
{
    [Export(typeof(IPageFactory))]
    /// <summary>
    /// This is a development PageFactory. Intended to be used SOLELY on local development machine.
    /// </summary>
    public class WebServicePageFactory : TridionFactoryBase, IPageFactory
    {
        public bool TryFindPage(string Url, out IPage page)
        {
            var serializer = new XmlSerializer(typeof(Page));

            page = null;
            //Create WCF Client
            using (var client = new TridionBrokerServiceClient())
            {
                //Create XML Document to hold Xml returned from WCF Client
                var pageContent = new XmlDocument();
                pageContent.LoadXml(client.FindPageByUrl(PublicationId, Url));
                
                //Load XML into Reader for deserialization
                using (var reader = new XmlNodeReader(pageContent.DocumentElement))
                {
                    try
                    {
                        page = (IPage)serializer.Deserialize(reader);
                        LoadComponentModelsFromComponentFactory(page);
                        return true;
                    }
                    catch (Exception)
                    {
                        //return false;
                        //throw new FieldHasNoValueException();
                    }
                }
            }

            return false;
        }

        private static void LoadComponentModelsFromComponentFactory(IPage page)
        {
            WebServiceComponentFactory factory = new WebServiceComponentFactory();
            foreach (ComponentPresentation cp in page.ComponentPresentations)
            {
                cp.Component = (Component)factory.GetComponent(cp.Component.Id);
            }
        }

        public IPage FindPage(string Url)
        {
            IPage page;
            if (!TryFindPage(Url, out page))
            {
                throw new PageNotFoundException();
            }

            return page;
        }

        public bool TryFindPageContent(string Url, out string pageContent)
        {
            pageContent = string.Empty;
            using (var client = new TridionBrokerServiceClient())
            {
                pageContent = client.FindPageByUrl(PublicationId, Url);
                if (pageContent != string.Empty)
                {
                    return true;
                }
            }
            return false;
        }
        public string FindPageContent(string Url)
        {
            string pageContent = string.Empty;
            if (!TryFindPageContent(Url, out pageContent))
            {
                throw new PageNotFoundException();
            }

            return pageContent;
        }

        public bool TryGetPage(string TcmUri, out IPage page)
        {
            throw new NotImplementedException();
        }
        public IPage GetPage(string TcmUri)
        {
            throw new NotImplementedException();
        }

        public bool TryGetPageContent(string TcmUri, out string pageContent)
        {
            throw new NotImplementedException();
        }
        public string GetPageContent(string TcmUri)
        {
            throw new NotImplementedException();
        }

        public bool HasPageChanged(string Url)
        {
            return true;
        }


        public DateTime GetLastPublishedDate(string url)
        {
            using (var client = new TridionBrokerServiceClient())
            {
                string pageMetaXml = client.FindPageMetaByUrl(PublicationId, url);
                if (!String.IsNullOrEmpty(pageMetaXml))
                {
                    XElement pageMeta = XElement.Parse(pageMetaXml);
                    DateTime lastPublishedDate = DateTime.Parse(pageMeta.Element("LastPublishDate").Value);
                    return lastPublishedDate;
                }
            }
            return DateTime.Now;
        }

        public string[] GetAllPublishedPageUrls(string[] includeExtensions, string[] pathStarts)
        {
            using (var client = new TridionBrokerServiceClient())
            {
                return client.GetAllPublishedPageUrls(includeExtensions, pathStarts, PublicationId);
            }
        }


        public DateTime GetLastPublishedDateByUrl(string url)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastPublishedDateByUri(string url)
        {
            throw new NotImplementedException();
        }
    }
}
