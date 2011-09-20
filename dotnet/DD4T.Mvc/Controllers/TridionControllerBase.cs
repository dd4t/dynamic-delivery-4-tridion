using System.Net;
using System.Web.Mvc;
using DD4T.ContentModel;
using DD4T.ContentModel.Exceptions;
using DD4T.ContentModel.Factories;
using DD4T.Mvc.Html;
using System;
using System.Web;
using DD4T.Factories;

namespace DD4T.Mvc.Controllers
{
    public abstract class TridionControllerBase : Controller
    {
        public IComponentPresentationRenderer componentPresentationRenderer { get; set; }
        public TridionControllerBase()
        {
            PageFactory = new PageFactory();
            LinkFactory = new LinkFactory();
            BinaryFactory = new BinaryFactory();
            ComponentFactory = new ComponentFactory();
        }

        protected IPageFactory PageFactory { get; private set; }
        protected ILinkFactory LinkFactory { get; private set; }
        protected IBinaryFactory BinaryFactory { get; private set; }
        protected IComponentFactory ComponentFactory { get; private set; }
        protected IComponentPresentationRenderer ComponentPresentationRenderer { get; private set; }

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

        protected ViewResult GetView(IPage page)
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
                throw new HttpException(404, "De pagina wordt niet gevonden.");
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
