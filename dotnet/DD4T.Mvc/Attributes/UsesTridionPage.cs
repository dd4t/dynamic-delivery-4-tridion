using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using DD4T.ContentModel;
using DD4T.Mvc.Controllers;
using DD4T.Utils;
using DD4T.ContentModel.Exceptions;

namespace DD4T.Mvc.Attributes
{
    /// <summary>
    /// Can be used to decorate hybrid controllers, which create their own model but need to have a Tridion page as additional input. The Tridion page is stored in ViewBag.Page and can be used in the view.
    /// </summary>
    public class UsesTridionPage : ActionFilterAttribute
    {

        /// <summary>
        /// Determines what to do if the page is not found. If the view relies on the page being there, this value should be true, otherwise it can be false.
        /// Defaults to true.
        /// </summary>
        public bool ThrowExceptionIfPageNotFound { get; set; }

        /// <summary>
        ///  Url of the page to retrieve (defaults to current url)
        /// </summary>
        public string PageUrl { get; set; }

        public UsesTridionPage()
        {
            ThrowExceptionIfPageNotFound = true;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!(filterContext.Controller is IPageController))
            {
                LoggerService.Warning("UsesTridionPage filter is used on a controller which does not implement IPageController");
                return;
            }
            string url = DD4T.Mvc.Html.TridionHelper.AddWelcomeFile(filterContext.RequestContext.HttpContext.Request.Path);
            IPage page = null;
            if (((IPageController)filterContext.Controller).PageFactory.TryFindPage(url, out page))
            {
                filterContext.Controller.ViewBag.Page = page;
            }
            else
            {
                if (ThrowExceptionIfPageNotFound)
                    throw new PageNotFoundException();
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
