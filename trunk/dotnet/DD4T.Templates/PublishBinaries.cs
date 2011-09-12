using System;
using System.IO;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Publishing.Rendering;
using Tridion.ContentManager.Templating;
using DD4T.Templates.Utils;

namespace DD4T.Templates 
{
	public class BinaryPublisher
    {
        protected TemplatingLogger log = TemplatingLogger.GetLogger(typeof(BinaryPublisher));
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
         xml.LoadXml("<tmproot>"  + xhtml + "</tmproot>");

         foreach (XmlElement img in xml.SelectNodes("//xhtml:img[@xlink:href[starts-with(string(.),'tcm:')]]", xml.NamespaceManager))
         {
            log.Debug("found img node " + img.OuterXml);
            XmlAttribute link = (XmlAttribute) img.SelectSingleNode("@xlink:href", xml.NamespaceManager);
            log.Debug("about to publish binary with uri " + link.Value);
            string path = PublishMultimediaComponent(link.Value);
            log.Debug("binary will be published to path " + path);
            img.SetAttribute("src", path);

         }
         return xml.DocumentElement.InnerXml;
      }

		public string PublishMultimediaComponent(string uri) {
			string itemName = "PublishMultimedia_" + uri;
			TcmUri tcmuri = new TcmUri(uri);
			Item mmItem = package.GetByName(itemName);
			if (mmItem == null) {
				mmItem = package.CreateMultimediaItem(tcmuri);
				package.PushItem(itemName, mmItem);
				log.Debug(string.Format("Image {0} ({1}) unique, adding to package", itemName, uri));
				if (!mmItem.Properties.ContainsKey(Item.ItemPropertyPublishedPath)) {
					log.Debug(string.Format("Publish Image {0} ({1}).", itemName, uri));
					PublishItem(mmItem, tcmuri);
				}
			} else {
				log.Debug(string.Format("Image {0} ({1}) already present in package, not adding again", itemName, tcmuri));
			}
			return mmItem.Properties[Item.ItemPropertyPublishedPath];
		}

		private void PublishItem(Item item, TcmUri itemUri) {
			Stream itemStream = null;
			// See if some template set itself as the applied template on this item
			TcmUri appliedTemplateUri = null;
			if (item.Properties.ContainsKey(Item.ItemPropertyTemplateUri)) {
				appliedTemplateUri = new TcmUri(item.Properties[Item.ItemPropertyTemplateUri]);
			}

			try {
				string publishedPath;
				if (targetStructureGroup == null) {
					log.Debug("no structure group defined, publishing binary with default settings");
					Component mmComp = (Component)engine.GetObject(item.Properties[Item.ItemPropertyTcmUri]);
					//Binary binary = engine.PublishingContext.RenderedItem.AddBinary(mmComp);
               Binary binary = engine.PublishingContext.RenderedItem.AddBinary(mmComp, currentTemplate.Id);

					publishedPath = binary.Url;
				} else {
					string fileName = TridionUtils.ConstructFileName(item);
					log.Debug("publishing binary into structure group " + targetStructureGroup.ItemId.ToString());
					itemStream = item.GetAsStream();
					if (itemStream == null) {
						// All items can be converted to a stream?
						log.Error(String.Format("Cannot get item '{0}' as stream", itemUri.ToString()));
					}
					byte[] data = new byte[itemStream.Length];
					itemStream.Read(data, 0, data.Length);
					publishedPath = engine.AddBinary(itemUri, appliedTemplateUri, targetStructureGroup, data, fileName);
				}
				log.Debug("binary published, published path = " + publishedPath);
				item.Properties[Item.ItemPropertyPublishedPath] = publishedPath;
			} finally {
				if (itemStream != null) itemStream.Close();
			}
		}
		#endregion

	}


}