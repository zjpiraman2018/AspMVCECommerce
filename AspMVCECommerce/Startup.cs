using AspMVCECommerce.Controllers;
using AspMVCECommerce.Models;
using AspMVCECommerce.Utility;
using Hangfire;
using Hangfire.Common;
using Hangfire.SqlServer;
using Hangfire.Storage;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

[assembly: OwinStartupAttribute(typeof(AspMVCECommerce.Startup))]
namespace AspMVCECommerce
{
    public partial class Startup
    {

        private IEnumerable<IDisposable> GetHangfireServers()
        {
            string conn = ConfigurationManager.ConnectionStrings[1].ConnectionString;

            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(conn, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });

            yield return new BackgroundJobServer();
        }

        //public void Configuration(IAppBuilder app)
        //{
        //    app.UseHangfireAspNet(GetHangfireServers);
        //    app.UseHangfireDashboard();

        //    // Let's also create a sample background job
        //    BackgroundJob.Enqueue(() => Debug.WriteLine("Hello world from Hangfire!"));

        //    // ...other configuration logic
        //}


        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");
            app.UseHangfireDashboard();
            //
            HomeController homeController = new HomeController();

            //RecurringJob.AddOrUpdate(() => homeController.HangFireSendEmail(""), Cron.Minutely);
            //RecurringJob.AddOrUpdate(() => homeController.HangFireSendEmail(""), "*/15 * * * *");

            // SEND EMAIL EVERY 5 MINUTEs

            //RecurringJob.AddOrUpdate(() => homeController.HangFireSendEmail(""), "*/5 * * * *");


            app.UseHangfireAspNet(GetHangfireServers);
            app.UseHangfireDashboard();


            // Let's also create a sample background job
            //BackgroundJob.Enqueue(() => Debug.WriteLine("Hello world from Hangfire!"));



            //RecurringJob.AddOrUpdate(() => homeController.HangFireSendEmail(""), "* * * * *");

            //RecurringJob.AddOrUpdate(
            //                        "myrecurringjob",
            //                        () => homeController.HangFireSendEmail(""),
            //                        Cron.MinuteInterval(1));


            //CLEAR HANG FIRE RECURRINGJOBS
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }


            //CLEAR HANGFIRE DATABASE
            using (var context = new ApplicationDbContext())
            {
                HangfireUtility.ClearDatabase(context);
            }



            var manager = new RecurringJobManager();

 

            manager.AddOrUpdate(DateTime.Now.ToLongTimeString(), Job.FromExpression(() => homeController.HangFireSendEmail("")), "*/5 * * * *");

            app.UseHangfireServer();
        }
    }
}
