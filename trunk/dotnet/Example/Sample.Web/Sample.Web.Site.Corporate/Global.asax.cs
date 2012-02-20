using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;
using Sample.Web.Mvc.Unity;

namespace SampleWebsite
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static IUnityContainer _container;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("{resource}.ico/{*pathInfo}");
            routes.IgnoreRoute("Scripts/{resource}");


            routes.MapRoute(
                "Search",
                "Search/{*PageId}",
                new { controller = "Page", action = "Search" }, // Parameter defaults
                new { pageId = @"^(.*)?$", httpMethod = new HttpMethodConstraint("POST") } // Parameter constraints
            );

            routes.MapRoute(
                "PreviewPage",
                "{*PageId}",
                new { controller = "Page", action = "PreviewPage" }, // Parameter defaults
                new { httpMethod = new HttpMethodConstraint("POST") } // Parameter constraints
            );


            routes.MapRoute(
                "TridionPage",
                "{*PageId}",
                new { controller = "Page", action = "Page" }, // Parameter defaults
                new { pageId = @"^(.*)?$" } // Parameter constraints
            );

            routes.MapRoute(
                "Default",
                "{controller}/{action}");

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            IUnityContainer container = new UnityContainer();
            UnityConfigurationSection section
              = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
            section.Configure(container, "main");

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

    }
}