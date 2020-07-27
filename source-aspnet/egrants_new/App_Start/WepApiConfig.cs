using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace egrants_new.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            //config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{action}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            // configure additional webapi settings here..
        }
    }
}