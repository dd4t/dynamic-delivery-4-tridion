using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DD4T.WebApplication
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            ///Route for SDL Tridion UI 2011 (Aka !SiteEdit)
            routes.MapRoute(
                "SiteEditBlankPage",
                "se_blank.html",
                new { controller = "Empty", action = "Index" });

            //Tridion page route
            routes.MapRoute(
               "TridionPage",
               "{*PageId}",
               new { controller = "Page", action = "Page" }, // Parameter defaults
               new { pageId = @"^(.*)?$" } // Parameter constraints
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}