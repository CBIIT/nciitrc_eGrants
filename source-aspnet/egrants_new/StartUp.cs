using Microsoft.Owin;
using Owin;

using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security.Cookies;

[assembly: OwinStartup(typeof(egrants_new.Startup))]

namespace egrants_new
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
                                            {
                                                CookieSameSite = SameSiteMode.None,
                                                CookieHttpOnly = true,
                                                CookieSecure = CookieSecureOption.Always,
                                                CookieManager = new SameSiteCookieManager(new SystemWebCookieManager())
                                            });
        }
    }
}