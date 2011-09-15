using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.Extensions.DynamicDelivery.ContentModel;
using TComm = Tridion.ContentManager.CommunicationManagement;
using TCM = Tridion.ContentManager.ContentManagement;
using DD4T.Extensions.DynamicDelivery.Templates.Utils;

namespace DD4T.Extensions.DynamicDelivery.Templates.Builder
{
   public class PublicationBuilder
   {
		public static Dynamic.Publication BuildPublication(TCM.Repository tcmPublication) {
         GeneralUtils.TimedLog("start BuildPublication");
			Dynamic.Publication pub = new Dynamic.Publication();
         GeneralUtils.TimedLog("get title from pub");
         pub.Title = tcmPublication.Title;
         GeneralUtils.TimedLog("found title");
         GeneralUtils.TimedLog("title=" + pub.Title);
         pub.Id = tcmPublication.Id.ToString();
         GeneralUtils.TimedLog("finished BuildPublication");
         return pub;
		}
   }
}
