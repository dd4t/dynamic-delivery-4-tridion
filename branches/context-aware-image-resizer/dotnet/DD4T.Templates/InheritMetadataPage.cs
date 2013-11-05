using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.CommunicationManagement;
using TCM = Tridion.ContentManager.ContentManagement;
using Dynamic = DD4T.ContentModel;
using DD4T.Templates.Base;
using DD4T.Templates.Base.Builder;
using DD4T.Templates.Base.Utils;

namespace DD4T.Templates
{
   [TcmTemplateTitle("Add inherited metadata to page")]
    [TcmTemplateParameterSchema("resource:DD4T.Templates.Resources.Schemas.Dynamic Delivery Parameters.xsd")]
    public partial class InheritMetadataPage : BasePageTemplate
   {
      protected Dynamic.MergeAction defaultMergeAction = Dynamic.MergeAction.Skip;



      private int LinkLevels
      {
          get
          {
              if (HasPackageValue(Package, "LinkLevels"))
              {
                  return Convert.ToInt32(Package.GetValue("LinkLevels"));
              }
              else
              {
                  GeneralUtils.TimedLog("no link levels configured, using default level " + this.DefaultLinkLevels);
                  return this.DefaultLinkLevels;
              }
          }
      }

      protected override void TransformPage(Dynamic.Page page)
      {
          GeneralUtils.TimedLog("start TransformPage with id " + page.Id);

         Page tcmPage = this.GetTcmPage();
         StructureGroup tcmSG = (StructureGroup)tcmPage.OrganizationalItem;
         
         String mergeActionStr = Package.GetValue("MergeAction");
         Dynamic.MergeAction mergeAction;
         if (! string.IsNullOrEmpty(mergeActionStr))
         {
             try
             {
                 mergeAction = (Dynamic.MergeAction)Enum.Parse(typeof(Dynamic.MergeAction), mergeActionStr);
             }
             catch
             {
                 GeneralUtils.TimedLog("unexpected merge action " + mergeActionStr + ", using default");
                 mergeAction = defaultMergeAction;
             }
         }
         else
         {
             GeneralUtils.TimedLog("no merge action specified, using default");
             mergeAction = defaultMergeAction;
         }
         GeneralUtils.TimedLog("using merge action " + mergeAction.ToString());

         while (tcmSG != null)
         {
             GeneralUtils.TimedLog("found structure group with id " + tcmSG.Id);

            if (tcmSG.MetadataSchema != null)
            {
               TCM.Fields.ItemFields tcmFields = new TCM.Fields.ItemFields(tcmSG.Metadata, tcmSG.MetadataSchema);
               GeneralUtils.TimedLog(string.Format("about to merge {0} fields on structure group with {1} fields on page ", tcmFields.Count, page.MetadataFields.Count));

               // change
               FieldsBuilder.AddFields(page.MetadataFields, tcmFields, LinkLevels, false, mergeAction, Manager);
               GeneralUtils.TimedLog(string.Format("finished merging, we now have {0} fields on structure group and {1} fields on page ", tcmFields.Count, page.MetadataFields.Count));
            }
            tcmSG = tcmSG.OrganizationalItem as StructureGroup;
         }

      }
   }
}
