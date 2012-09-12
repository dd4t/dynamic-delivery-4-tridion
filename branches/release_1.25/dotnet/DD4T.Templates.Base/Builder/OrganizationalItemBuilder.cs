using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.ContentModel;
using TComm = Tridion.ContentManager.CommunicationManagement;
using TCM = Tridion.ContentManager.ContentManagement;
using DD4T.Templates.Base.Utils;

namespace DD4T.Templates.Base.Builder {
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
