using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(SBM.Web.Startup))]

namespace SBM.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}