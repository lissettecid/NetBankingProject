using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NetBanking.Startup))]
namespace NetBanking
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
