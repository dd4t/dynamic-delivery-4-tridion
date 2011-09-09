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
using System.Text;
using Dynamic = Tridion.Extensions.DynamicDelivery.ContentModel;
using TComm = Tridion.ContentManager.CommunicationManagement;
using TCM = Tridion.ContentManager.ContentManagement;
using Tridion.Extensions.DynamicDelivery.Templates.Utils;

namespace Tridion.Extensions.DynamicDelivery.Templates.Builder {
	public class OrganizationalItemBuilder {
		public static Dynamic.OrganizationalItem BuildOrganizationalItem(TComm.StructureGroup tcmStructureGroup) {
         GeneralUtils.TimedLog("start BuildOrganizationalItem");
         Dynamic.OrganizationalItem oi = new Dynamic.OrganizationalItem();
			oi.Title = tcmStructureGroup.Title;
         oi.Id = tcmStructureGroup.Id.ToString();
         oi.PublicationId = tcmStructureGroup.ContextRepository.Id.ToString();
         GeneralUtils.TimedLog("finished BuildOrganizationalItem");
         return oi;
		}
      public static Dynamic.OrganizationalItem BuildOrganizationalItem(TCM.Folder tcmFolder)
      {
         GeneralUtils.TimedLog("start BuildOrganizationalItem");
         Dynamic.OrganizationalItem oi = new Dynamic.OrganizationalItem();
         oi.Title = tcmFolder.Title;
         oi.Id = tcmFolder.Id.ToString();
         oi.PublicationId = tcmFolder.ContextRepository.Id.ToString();
         GeneralUtils.TimedLog("finished BuildOrganizationalItem");
         return oi;
      }
   }
}
