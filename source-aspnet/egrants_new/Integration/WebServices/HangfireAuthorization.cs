using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;
using Microsoft.Owin;

namespace egrants_new.Integration.WebServices
{
    public class HangfireAuthorization : IDashboardAuthorizationFilter
    {

        public bool Authorize(DashboardContext context)
        {

            // In case you need an OWIN context, use the next line, `OwinContext` class
            // is the part of the `Microsoft.Owin` package.
            var owinContext = new OwinContext(context.GetOwinEnvironment());
            //var userid = owinContext.Request.User.Identity.Name;

            //var userid = HttpApplication.Context.Request.ServerVariables["HEADER_SM_USER"];
            if (Users.Split(',').Contains(userid))
            {
                return true;
            }
            return false;


        }

        public string Users { get; set; }

    }
}