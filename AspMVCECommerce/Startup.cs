using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AspMVCECommerce.Startup))]
namespace AspMVCECommerce
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
