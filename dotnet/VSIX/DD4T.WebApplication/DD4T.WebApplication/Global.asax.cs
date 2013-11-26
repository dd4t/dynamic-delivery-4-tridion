using log4net.Config;
using Ninject;
using Ninject.Web.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace DD4T.WebApplication
{
    /// <summary>
    /// Application class, note we are using NinjectHttpApplication so you can easily inject dependencies into controllers
    /// </summary>
    public class MvcApplication : NinjectHttpApplication
    {
        protected override void OnApplicationStarted()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Set up log4net
            XmlConfigurator.Configure();
        }

        protected override IKernel CreateKernel()
        {
            return DependencyInjectionConfig.Configure();
        }
    }
}