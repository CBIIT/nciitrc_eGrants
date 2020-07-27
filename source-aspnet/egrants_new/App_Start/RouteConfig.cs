using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace egrants_new
{
    public class RouteConfig
    {     
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.RouteExistingFiles = true;

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Egrants",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Egrants", action = "Index", id = UrlParameter.Optional }
            //);          

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Egrants", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Integration",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Integration", action = "Trigger", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Docman",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Docman", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
