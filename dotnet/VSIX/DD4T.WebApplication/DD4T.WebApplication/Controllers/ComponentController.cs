using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DD4T.Mvc.Controllers;
using System.Web.Mvc;
using DD4T.ContentModel;
using DD4T.ContentModel.Exceptions;
using System.Configuration;

namespace DD4T.WebApplication.Controllers
{
    public class ComponentController : TridionControllerBase
    {
        protected override ViewResult GetView(IComponentPresentation componentPresentation)
        {
            if (!componentPresentation.ComponentTemplate.MetadataFields.ContainsKey("view"))
            {
                throw new ConfigurationErrorsException("no view configured for component template " + componentPresentation.ComponentTemplate.Id);
            }

            string viewName = componentPresentation.ComponentTemplate.MetadataFields["view"].Value;
           
            return View(viewName, componentPresentation);
        }
    }
}
