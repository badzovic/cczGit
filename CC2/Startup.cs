using Microsoft.Owin;
using Owin;
using System.Web.Services.Description;
using NLog;



[assembly: OwinStartupAttribute(typeof(CC2.Startup))]
namespace CC2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
           
        }

    
    }
}
