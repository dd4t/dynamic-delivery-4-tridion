﻿using System;
using System.IO;
using System.Drawing;
using Tridion.ContentManager.Templating;
using TCM = Tridion.ContentManager.ContentManagement;
using DD4T.ContentModel;
using DD4T.Templates.Base.Utils;
using Dynamic = DD4T.ContentModel;

namespace DD4T.Templates.Base.Builder
{
   public class ComponentBuilder
   {
      protected static TemplatingLogger log = TemplatingLogger.GetLogger(typeof(ComponentBuilder));
      public static Dynamic.Component BuildComponent(TCM.Component tcmComponent, BuildManager manager)
      {
          return BuildComponent(tcmComponent, 1, false,false, manager);
		}
      public static Dynamic.Component BuildComponent(TCM.Component tcmComponent, int linkLevels, bool resolveWidthAndHeight, bool publishEmptyFields, BuildManager manager)
      {
          GeneralUtils.TimedLog("start BuildComponent");
          Dynamic.Component c = new Dynamic.Component();
          c.Title = tcmComponent.Title;
          c.Id = tcmComponent.Id.ToString();
          c.RevisionDate = tcmComponent.RevisionDate;
          GeneralUtils.TimedLog("component title = " + c.Title);

          c.Version = tcmComponent.Version;
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
              string extension = System.IO.Path.GetExtension(multimedia.FileName);
              if (string.IsNullOrEmpty(extension))
                  multimedia.FileExtension = "";
              else
                  multimedia.FileExtension = extension.Substring(1);
 

              if (resolveWidthAndHeight)
              {
                  try
                  {
                      MemoryStream memstream = new MemoryStream();
                      tcmComponent.BinaryContent.WriteToStream(memstream);
                      Image image = Image.FromStream(memstream);
                      memstream.Close();

                      multimedia.Width = image.Size.Width;
                      multimedia.Height = image.Size.Height;
                  }
                  catch (Exception e)
                  {
                      log.Warning("error retrieving width and height of image: " + e.Message);
                      multimedia.Width = 0;
                      multimedia.Height = 0;
                  }
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
          c.Fields = new Dynamic.FieldSet();
          c.MetadataFields = new Dynamic.FieldSet();
          if (linkLevels > 0)
          {
              if (tcmComponent.Content != null)
              {
                  GeneralUtils.TimedLog("start retrieving tcm fields");
                  TCM.Fields.ItemFields tcmFields = new TCM.Fields.ItemFields(tcmComponent.Content, tcmComponent.Schema);
                  log.Debug("TCM fields" +tcmFields.ToXml());
                  GeneralUtils.TimedLog("finished retrieving tcm fields");
                  GeneralUtils.TimedLog("start building fields");
                  c.Fields = manager.BuildFields(tcmFields, linkLevels, resolveWidthAndHeight,publishEmptyFields);
                  GeneralUtils.TimedLog("finished building fields");
              }
              
              if (tcmComponent.Metadata != null)
              {
                  GeneralUtils.TimedLog("start retrieving tcm metadata fields");
                  TCM.Fields.ItemFields tcmMetadataFields = new TCM.Fields.ItemFields(tcmComponent.Metadata, tcmComponent.MetadataSchema);
                  GeneralUtils.TimedLog("finished retrieving tcm metadata fields");
                  GeneralUtils.TimedLog("start building metadata fields");
                  c.MetadataFields = manager.BuildFields(tcmMetadataFields, linkLevels, resolveWidthAndHeight,publishEmptyFields);
                  GeneralUtils.TimedLog("finished building metadata fields");
              }
          }
          if (! manager.BuildProperties.OmitContextPublications)
          c.Publication = manager.BuildPublication(tcmComponent.ContextRepository);

          if (!manager.BuildProperties.OmitOwningPublications)
              c.OwningPublication = manager.BuildPublication(tcmComponent.OwningRepository);

          if (!manager.BuildProperties.OmitFolders)
          {
              TCM.Folder folder = (TCM.Folder)tcmComponent.OrganizationalItem;
              c.Folder = manager.BuildOrganizationalItem(folder);
          }
          c.Categories = manager.BuildCategories(tcmComponent);

          manager.AddXpathToFields(c.Fields, "tcm:Content/custom:" + tcmComponent.Schema.RootElementName); 
          manager.AddXpathToFields(c.MetadataFields, "tcm:Metadata/custom:Metadata");
          return c;
      }
	}
}
