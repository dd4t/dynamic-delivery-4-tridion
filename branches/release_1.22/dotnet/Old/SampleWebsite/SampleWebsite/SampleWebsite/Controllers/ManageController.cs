using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using DD4T.Extensions.DynamicDelivery.Mvc.Controllers;
using System.Security;
using DD4T.Extensions.DynamicDelivery.Mvc.Html;
using System.ComponentModel.Composition;

namespace SampleWebsite.Controllers
{
    public class ManageController : TridionControllerBase
    {
        [ImportingConstructor]
        public ManageController(IComponentPresentationRenderer renderer)
            : base(renderer)
        {
        }

#if (!DEBUG)
        [OutputCache(CacheProfile = "ControllerCache")]
#endif
        [HandleError]
        public override System.Web.Mvc.ActionResult Page(string pageId)
        {
            try
            {
                return base.Page(pageId);
            }
            catch (SecurityException se)
            {
                throw new HttpException(403, se.Message);
            }
        }
    }
}
