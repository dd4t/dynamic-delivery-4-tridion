using System.Net;
using System.Web.Mvc;
using DD4T.ContentModel;
using DD4T.ContentModel.Exceptions;
using DD4T.ContentModel.Factories;
using DD4T.Mvc.Html;
using System;
using System.Web;
using DD4T.Factories;
using System.Web.Configuration;
using System.IO;
using System.Security;

namespace DD4T.Mvc.Controllers
{
    public abstract class TridionControllerBase : Controller, IPageController, IComponentController
    {
        public IComponentPresentationRenderer componentPresentationRenderer { get; set; }

        public IPageFactory PageFactory { get; set; }
        public IComponentFactory ComponentFactory { get; set; }
        public IComponentPresentationRenderer ComponentPresentationRenderer { get; set; }

        protected IPage GetModelForPage(string PageId)
        {
            IPage page;
            if (PageFactory != null)
            {               
                if (PageFactory.TryFindPage(string.Format("/{0}",PageId), out page))
                {
                    return page;
                }
            }
            else
                throw new ConfigurationException("No PageFactory configured");

            return page;
        }

        protected virtual ViewResult GetView(IPage page)
        {
            string viewName = page.PageTemplate.MetadataFields["view"].Value; //TODO: Errod handling if no meta-data schema is selected on the page template
            return base.View(viewName, page);
        }

        protected IComponentPresentation GetComponentPresentation()
        {
            return this.RouteData.Values["ComponentPresentation"] as IComponentPresentation;
        }

        protected virtual ViewResult GetView(IComponentPresentation componentPresentation)
        {
            string viewName = null;

            // TODO: define field names in Web.config
            if (!componentPresentation.ComponentTemplate.MetadataFields.ContainsKey("view"))
            {
                throw new ConfigurationException("no view configured for component template " + componentPresentation.ComponentTemplate.Id);
            }
            viewName = componentPresentation.ComponentTemplate.MetadataFields["view"].Values[0];
            return View(viewName, componentPresentation);

        }

        [HandleError]
        public virtual ActionResult Page(string pageId)
        {
            try
            {
                IPage model = GetModelForPage(pageId);
                if (model == null) { throw new PageNotFoundException(); }
                ViewBag.Title = model.Title;
                ViewBag.Renderer = ComponentPresentationRenderer;
                return GetView(model);
            }
            catch (PageNotFoundException)
            {
                throw new HttpException(404, "Page cannot be found");
            }
        }

        /// <summary>
        /// Create IPage from XML in the request and forward to the view
        /// </summary>
        /// <example>
        /// To use, add the following code to the Global.asax.cs of your MVC web application:
        ///             routes.MapRoute(
        ///                "PreviewPage",
        ///                "{*PageId}",
        ///                new { controller = "Page", action = "PreviewPage" }, // Parameter defaults
        ///                new { httpMethod = new HttpMethodConstraint("POST") } // Parameter constraints
        ///            );
        ///            * This is assuming that you have a controller called PageController which extends TridionControllerBase
        /// </example>
        /// <returns></returns>
        [HandleError]
        [AcceptVerbs(HttpVerbs.Post)]
        public System.Web.Mvc.ActionResult PreviewPage()
        {

            try
            {
                using (StreamReader reader = new StreamReader(this.Request.InputStream))
                {
                    string pageXml = reader.ReadToEnd();
                    IPage model = this.PageFactory.GetIPageObject(pageXml);
                    if (model == null)
                    {
                        throw new ModelNotCreatedException("--unknown--");
                    }
                    ViewBag.Title = model.Title;
                    ViewBag.Renderer = ComponentPresentationRenderer;
                    ViewResult result = GetView(model);
                    return result;
                }
            }
            catch (SecurityException se)
            {
                throw new HttpException(403, se.Message);
            }

        }


        public virtual ActionResult ComponentPresentation(string componentPresentationId)
        {
            try
            {
                IComponentPresentation model = GetComponentPresentation();
                ViewBag.Renderer = ComponentPresentationRenderer;
                return GetView(model);
            }
            catch (ConfigurationException e)
            {
                return View("Configuration exception: " + e.Message);
            }
        }
        //public virtual ActionResult ComponentPresentation(IComponentPresentation cp)
        //{
        //    try
        //    {
        //        ViewBag.Renderer = ComponentPresentationRenderer;
        //        return GetView(cp);
        //    }
        //    catch (ConfigurationException e)
        //    {
        //        return View("Configuration exception: " + e.Message);
        //    }
        //}


    }
}
