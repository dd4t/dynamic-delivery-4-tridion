///   
/// Copyright 2011 Capgemini & SDL
///
///   Licensed under the Apache License, Version 2.0 (the "License");
///   you may not use this file except in compliance with the License.
///   You may obtain a copy of the License at
///
///       http://www.apache.org/licenses/LICENSE-2.0
///
///   Unless required by applicable law or agreed to in writing, software
///   distributed under the License is distributed on an "AS IS" BASIS,
///   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
///   See the License for the specific language governing permissions and
///   limitations under the License.

using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Publishing.Rendering;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.Logging;
using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Text;
using System.Text.RegularExpressions;
using Dynamic = Tridion.Extensions.DynamicDelivery.ContentModel;
using System.Collections.Generic;
using System.Xml.Serialization;
using Tridion.Extensions.DynamicDelivery.Templates.Utils;

namespace Tridion.Extensions.DynamicDelivery.Templates
{

    public class BinaryPublisher
    {

        protected new TemplatingLogger log = TemplatingLogger.GetLogger(typeof(BinaryPublisher));
        TcmUri targetStructureGroup = null;
        Package package;
        Engine engine;
        Template currentTemplate;

        public BinaryPublisher(Package package, Engine engine)
        {

            this.package = package;
            this.engine = engine;

            currentTemplate = engine.PublishingContext.ResolvedItem.Template;

            // Determine (optional) structure group parameter
            String targetStructureGroupParam = package.GetValue("PublishBinariesTargetStructureGroup");
            if (targetStructureGroupParam != null)
            {
                if (!TcmUri.IsValid(targetStructureGroupParam))
                {
                    log.Error(String.Format("TargetStructureGroup '{0}' is not a valid TCMURI. Exiting template.", targetStructureGroupParam));
                    return;
                }

                Publication publication = TridionUtils.GetPublicationFromContext(package, engine);
                TcmUri localTargetStructureGroupTcmUri = TridionUtils.GetLocalUri(new TcmUri(publication.Id), new TcmUri(targetStructureGroupParam));
                targetStructureGroup = new TcmUri(localTargetStructureGroupTcmUri);
            }
        }


        #region Protected Members

        public string PublishBinariesInRichTextField(string xhtml)
        {

            // rich text field is well-formed, except that it does not always have a root element
            // to be sure it can be parsed, we will add a root and remove it afterwards

            TridionXml xml = new TridionXml();
            xml.LoadXml("<tmproot>" + xhtml + "</tmproot>");

            /*
                     foreach (XmlElement img in xml.SelectNodes("//xhtml:img[@xlink:href[starts-with(string(.),'tcm:')]]", xml.NamespaceManager))
                     {
                        log.Debug("found img node " + img.OuterXml);
                        XmlAttribute link = (XmlAttribute) img.SelectSingleNode("@xlink:href", xml.NamespaceManager);
                        log.Debug("about to publish binary with uri " + link.Value);
                        string path = PublishMultimediaComponent(link.Value);
                        log.Debug("binary will be published to path " + path);
                        img.SetAttribute("src", path);

                     }
            // if text links to some multimedia component
                     foreach (XmlElement img in xml.SelectNodes("//xhtml:a[@xlink:href[starts-with(string(.),'tcm:')]]", xml.NamespaceManager))
                     {
            
                         XmlAttribute link = (XmlAttribute)img.SelectSingleNode("@xlink:href", xml.NamespaceManager);

                         Component mmComp = (Component)engine.GetObject(link.Value);
                         if (mmComp.ComponentType == ComponentType.Multimedia)
                         {
                             log.Debug("found anchor node with multimedia link " + img.OuterXml);
                             log.Debug("about to publish binary with uri " + link.Value);
                             string path = PublishMultimediaComponent(link.Value);
                             log.Debug("binary will be published to path " + path);
                             img.SetAttribute("src", path);

                         }
             


                     }
            */
            string rx = @"tcm:(\d)+-(\d)+(-16){0,1}";
            Regex regex = new Regex(rx);
            foreach (Match match in regex.Matches(xml.InnerXml))
            {
                Component mmComp = (Component)engine.GetObject(match.Value);
                if (mmComp != null && mmComp.ComponentType == ComponentType.Multimedia)
                {
                    log.Debug("found multimedia  " + match.Value);
                    log.Debug("about to publish binary with uri " + match.Value);
                    string path = PublishMultimediaComponent(match.Value);
                    log.Debug("binary will be published to path " + path);


                }
            }


            return xml.DocumentElement.InnerXml;
        }

        public string PublishMultimediaComponent(string uri)
        {
            // get uri in context repository
            Publication pub = ((RepositoryLocalObject)engine.PublishingContext.ResolvedItem.Item).ContextRepository as Publication;

            if (uri.Split('-')[0] != string.Format("tcm:{0}", pub.Id.ItemId))
                uri = GetTcmIDinPublication(uri, pub);

            string itemName = "PublishMultimedia_" + uri;
            TcmUri tcmuri = new TcmUri(uri);
            Item mmItem = package.GetByName(itemName);
           
            if (mmItem == null)
            {
                mmItem = package.CreateMultimediaItem(tcmuri);
                package.PushItem(itemName, mmItem);
                log.Debug(string.Format("Image {0} ({1}) unique, adding to package", itemName, uri));
                if (!mmItem.Properties.ContainsKey(Item.ItemPropertyPublishedPath))
                {
                    log.Debug(string.Format("Publish Image {0} ({1}).", itemName, uri));
                    PublishItem(mmItem, tcmuri);
                }
            }
            else
            {
                log.Debug(string.Format("Image {0} ({1}) already present in package, not adding again", itemName, tcmuri));
            }
            return mmItem.Properties[Item.ItemPropertyPublishedPath];
        }

        private void publishBinariesInComponentMetaData(Component MMComponent)
        {
            //if (MMComponent.Metadata == null)
            //    return;
           
            TridionXml xml = new TridionXml();
            try
            {
                xml.LoadXml(MMComponent.Metadata.OuterXml);
            }
            catch (Exception ex)
            {
                return;
            }

                string rx = @"tcm:(\d)+-(\d)+(-16){0,1}";
                Regex regex = new Regex(rx);
                foreach (Match match in regex.Matches(xml.InnerXml))
                {
                    Component mmComp = (Component)engine.GetObject(match.Value);
                    if (mmComp != null && mmComp.ComponentType == ComponentType.Multimedia)
                    {
                        log.Debug("found multimedia  " + match.Value);
                        log.Debug("about to publish binary with uri " + match.Value);
                        string path = PublishMultimediaComponent(match.Value);
                        log.Debug("binary will be published to path " + path);


                    }

                }
           
        }

        private string GetTcmIDinPublication(string TCMID, Publication publication)
        {

            return string.Format("tcm:{0}-{1}", publication.Id.ItemId, TCMID.Split('-')[1]);

        }

        private void PublishItem(Item item, TcmUri itemUri)
        {
            Stream itemStream = null;
            // See if some template set itself as the applied template on this item
            TcmUri appliedTemplateUri = null;
            if (item.Properties.ContainsKey(Item.ItemPropertyTemplateUri))
            {
                appliedTemplateUri = new TcmUri(item.Properties[Item.ItemPropertyTemplateUri]);
            }

            try
            {
                string publishedPath;
                if (targetStructureGroup == null)
                {
                    log.Debug("no structure group defined, publishing binary with default settings");
                    Component mmComp = (Component)engine.GetObject(item.Properties[Item.ItemPropertyTcmUri]);
                    //Binary binary = engine.PublishingContext.RenderedItem.AddBinary(mmComp);
                    //Binary binary = engine.PublishingContext.RenderedItem.AddBinary(mmComp, currentTemplate.Id);
                    publishBinariesInComponentMetaData(mmComp);
                    Binary binary = engine.PublishingContext.RenderedItem.AddBinary(mmComp, "Full");

                    publishedPath = binary.Url;
                }
                else
                {
                    string fileName = TridionUtils.ConstructFileName(item);
                    log.Debug("publishing binary into structure group " + targetStructureGroup.ItemId.ToString());
                    itemStream = item.GetAsStream();
                    if (itemStream == null)
                    {
                        // All items can be converted to a stream?
                        log.Error(String.Format("Cannot get item '{0}' as stream", itemUri.ToString()));
                    }
                    byte[] data = new byte[itemStream.Length];
                    itemStream.Read(data, 0, data.Length);
                    publishedPath = engine.AddBinary(itemUri, appliedTemplateUri, targetStructureGroup, data, fileName);
                }
                log.Debug("binary published, published path = " + publishedPath);
                item.Properties[Item.ItemPropertyPublishedPath] = publishedPath;
            }
            finally
            {
                if (itemStream != null) itemStream.Close();
            }
        }
        #endregion

    }


}