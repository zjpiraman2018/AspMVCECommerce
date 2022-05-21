using AspMVCECommerce.Controllers;
using AspMVCECommerce.Models;
using Hangfire;
using Microsoft.Owin;
using Owin;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

[assembly: OwinStartupAttribute(typeof(AspMVCECommerce.Startup))]
namespace AspMVCECommerce
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");
            app.UseHangfireDashboard();
            //
            HomeController homeController = new HomeController();
            RecurringJob.AddOrUpdate(() => homeController.HangFireSendEmail(), Cron.Minutely);

            //ApplicationDbContext db = new ApplicationDbContext();

            //HomeController homeController = new HomeController();

            //string controllerName = "Home";
            //var context = new HttpContextWrapper(System.Web.HttpContext.Current);
            //var routeData = System.Web.Routing.RouteTable.Routes.GetRouteData(context);

            //var myRequestContext = new System.Web.Routing.RequestContext(context, routeData);

            //var controllerBuilder = ControllerBuilder.Current;
            //IControllerFactory factory = controllerBuilder.GetControllerFactory();
            //IController controller = factory.CreateController(myRequestContext, controllerName);

            //var controller2 = DependencyResolver.Current.GetService<HomeController>();
            //controller2.ControllerContext = new ControllerContext(myRequestContext, controller2);


            //try
            //{

            //    //controller.Execute(myRequestContext);

            //    RecurringJob.AddOrUpdate(() => controller2.TestEmail(), Cron.Minutely);
            //}
            //finally
            //{
            //    factory.ReleaseController(controller);
            //}


            app.UseHangfireServer();
        }
    }
}
