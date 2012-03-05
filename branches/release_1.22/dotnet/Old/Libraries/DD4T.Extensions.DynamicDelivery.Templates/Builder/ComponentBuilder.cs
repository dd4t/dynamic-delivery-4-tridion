using System;
using System.Drawing;
using System.IO;
using Tridion.ContentManager.Templating;
using DD4T.Extensions.DynamicDelivery.ContentModel;
using DD4T.Extensions.DynamicDelivery.Templates.Utils;
using Dynamic = DD4T.Extensions.DynamicDelivery.ContentModel;
using TCM = Tridion.ContentManager.ContentManagement;

namespace DD4T.Extensions.DynamicDelivery.Templates.Builder
{
   // TODO: add Publication element inside component (+ page, etc)
   // TODO: add OrganizationalItem element inside component (+ page, etc)
   public class ComponentBuilder
   {
      protected TemplatingLogger log = TemplatingLogger.GetLogger(typeof(ComponentBuilder));
      public static Dynamic.Component BuildComponent(TCM.Component tcmComponent, BuildManager manager)
      {
          return BuildComponent(tcmComponent, 1, false, manager);
		}
      public static Dynamic.Component BuildComponent(TCM.Component tcmComponent, int linkLevels, bool resolveWidthAndHeight, BuildManager manager)
      {
         GeneralUtils.TimedLog("start BuildComponent");
			Dynamic.Component c = new Dynamic.Component();
			c.Title = tcmComponent.Title;
			c.Id = tcmComponent.Id.ToString();
         GeneralUtils.TimedLog("component title = " + c.Title);

         GeneralUtils.TimedLog("start building schema");
         c.Schema = manager.BuildSchema(tcmComponent.Schema);
         GeneralUtils.TimedLog("finished building schema");
         c.ComponentType = (ComponentType)Enum.Parse(typeof(ComponentType), tcmComponent.ComponentType.ToString());

         if (tcmComponent.ComponentType.Equals(TCM.ComponentType.Multimedia))
         {
            GeneralUtils.TimedLog("start building multimedia");
            Multimedia multimedia = new Multimedia();
            multimedia.MimeType = tcmComponent.BinaryContent.MultimediaType.MimeType;
            multimedia.Size = tcmComponent.BinaryContent.FileSize;
            multimedia.FileName = tcmComponent.BinaryContent.Filename;
            // remove leading dot from extension because microsoft returns this as ".gif"
            multimedia.FileExtension = System.IO.Path.GetExtension(multimedia.FileName).Substring(1);

            if (resolveWidthAndHeight)
            {
               MemoryStream memstream = new MemoryStream();
               tcmComponent.BinaryContent.WriteToStream(memstream);
               Image image = Image.FromStream(memstream);
               memstream.Close();

               multimedia.Width = image.Size.Width;
               multimedia.Height = image.Size.Height;
            }
            else
            {
               multimedia.Width = 0;
               multimedia.Height = 0;
            }
            c.Multimedia = multimedia;
            GeneralUtils.TimedLog("finished building multimedia");
         }
         else
         {
            c.Multimedia = null;
         }
         c.Fields = new Dynamic.SerializableDictionary<string,Field>();
			c.MetadataFields = new Dynamic.SerializableDictionary<string, Field>();
			if (linkLevels > 0) {
            if (tcmComponent.Content != null)
            {
               GeneralUtils.TimedLog("start retrieving tcm fields");
               TCM.Fields.ItemFields tcmFields = new TCM.Fields.ItemFields(tcmComponent.Content, tcmComponent.Schema);
               GeneralUtils.TimedLog("finished retrieving tcm fields");
               GeneralUtils.TimedLog("start building fields");
               c.Fields = manager.BuildFields(tcmFields, linkLevels, resolveWidthAndHeight);
               GeneralUtils.TimedLog("finished building fields");
            }
				if (tcmComponent.Metadata != null) {
               GeneralUtils.TimedLog("start retrieving tcm metadata fields");
               TCM.Fields.ItemFields tcmMetadataFields = new TCM.Fields.ItemFields(tcmComponent.Metadata, tcmComponent.MetadataSchema);
               GeneralUtils.TimedLog("finished retrieving tcm metadata fields");
               GeneralUtils.TimedLog("start building metadata fields");
               c.MetadataFields = manager.BuildFields(tcmMetadataFields, linkLevels, resolveWidthAndHeight);
               GeneralUtils.TimedLog("finished building metadata fields");
            }
			}



         GeneralUtils.TimedLog("start retrieving tcm publication");
         TCM.Repository pub = tcmComponent.ContextRepository;
         GeneralUtils.TimedLog("finished retrieving tcm publication");
         GeneralUtils.TimedLog("start building publication");
         c.Publication = manager.BuildPublication(pub);
         GeneralUtils.TimedLog("finished building publication");

         GeneralUtils.TimedLog("start retrieving tcm folder");
         TCM.Folder folder = (TCM.Folder) tcmComponent.OrganizationalItem;
         GeneralUtils.TimedLog("finished retrieving tcm folder");
         GeneralUtils.TimedLog("start building folder");
         c.Folder = manager.BuildOrganizationalItem(folder);
         GeneralUtils.TimedLog("finished building folder");
         GeneralUtils.TimedLog("start building categories");
         c.Categories = manager.BuildCategories(tcmComponent);
         GeneralUtils.TimedLog("finished building categories");

         GeneralUtils.TimedLog("finished BuildComponent " + c.Title);

         return c;
		}
	}
}
