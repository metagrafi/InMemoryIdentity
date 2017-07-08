using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NoDbIdentity.Startup))]
namespace NoDbIdentity
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
