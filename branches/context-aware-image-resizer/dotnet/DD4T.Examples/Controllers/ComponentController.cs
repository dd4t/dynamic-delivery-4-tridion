using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using DD4T.Mvc.Controllers;
using DD4T.ContentModel.Factories;
using DD4T.ContentModel.Exceptions;
using DD4T.ContentModel;
using System.Xml;
using DD4T.Examples.Models;

namespace DD4T.Examples.Controllers
{

    public class ComponentController : TridionControllerBase
    {
        public ComponentController()
        {
        }

        protected override ViewResult GetView(IComponentPresentation componentPresentation)
        {
            if (!componentPresentation.ComponentTemplate.MetadataFields.ContainsKey("view"))
            {
                throw new ConfigurationException("no view configured for component template " + componentPresentation.ComponentTemplate.Id);
            }
            string viewName = componentPresentation.ComponentTemplate.MetadataFields["view"].Value;           
            return View(viewName, componentPresentation.Component);
        }

        public ActionResult ShowWeather()
        {
            IComponentPresentation cp = this.GetComponentPresentation();
            Weather weather = new Weather(cp);
            string viewName = cp.ComponentTemplate.MetadataFields["view"].Value;
            return View(viewName, weather);
        }

        //public ActionResult Query()
        //{
        //    List<IComponent> components = new List<IComponent>();
        //    IComponentPresentation cp = this.GetComponentPresentation();

        //    ExtendedQueryParameters eqp = new ExtendedQueryParameters();
        //    if (cp.Component.Fields.ContainsKey("Schema"))
        //    {
        //        string schemaName = cp.Component.Fields["Schema"].Value;
        //        eqp.QuerySchemas = new string[] { schemaName };
        //    }
        //    // todo: add 'last XXX days' field
        //    eqp.LastPublishedDate = DateTime.Now.AddMonths(-3); // search for everything in the last 3 months

        //    // run the query
        //    ViewBag.Results = ComponentFactory.FindComponents(eqp);
        //    return View(cp.Component);
        //}

    }
}
