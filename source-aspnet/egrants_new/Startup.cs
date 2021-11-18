using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(egrants_new.Startup))]

namespace egrants_new
{
    public partial class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

    }
}