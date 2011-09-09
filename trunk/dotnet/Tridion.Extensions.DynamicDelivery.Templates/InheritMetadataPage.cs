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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.CommunicationManagement;
using Dynamic = Tridion.Extensions.DynamicDelivery.ContentModel;
using TCM = Tridion.ContentManager.ContentManagement;

namespace Tridion.Extensions.DynamicDelivery.Templates
{
   [TcmTemplateTitle("Add inherited metadata to page")]
   public partial class InheritMetadataPage : BasePageTemplate
   {
      protected Dynamic.MergeAction defaultMergeAction = Dynamic.MergeAction.Skip;
     
      protected override void TransformPage(Dynamic.Page page)
      {
         Page tcmPage = this.GetTcmPage();
         StructureGroup tcmSG = (StructureGroup)tcmPage.OrganizationalItem;
         
         String mergeActionStr = package.GetValue("MergeAction");
         Dynamic.MergeAction mergeAction;
         if (string.IsNullOrEmpty(mergeActionStr))
         {
            mergeAction = defaultMergeAction;
         }
         else
         {
            mergeAction = (Dynamic.MergeAction)Enum.Parse(typeof(Dynamic.MergeAction), mergeActionStr);
         }

         while (tcmSG.OrganizationalItem != null)
         {
            if (tcmSG.MetadataSchema != null)
            {
               TCM.Fields.ItemFields tcmFields = new TCM.Fields.ItemFields(tcmSG.Metadata, tcmSG.MetadataSchema);
               // change
               Builder.FieldsBuilder.AddFields(page.Metadata, tcmFields, 1, false, mergeAction, manager);
            }
            tcmSG = (StructureGroup)tcmSG.OrganizationalItem;
         }

      }
   }
}
