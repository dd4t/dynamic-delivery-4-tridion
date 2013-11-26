using DD4T.Mvc.Controllers;
using DD4T.WebApplication.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DD4T.WebApplication.Controllers
{
    public class PageController : TridionControllerBase
    {
        public override ActionResult Page(string pageId)
        {
            pageId = UriHelper.ParseUrl(pageId);
            return base.Page(pageId);
        }
    }
}
