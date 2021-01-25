using System;

using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using egrants_new.Integration.WebServices;
using Microsoft.Owin;
using Owin;
using Hangfire;
using Hangfire.SqlServer;
using egrants_new;
using Hangfire.Dashboard;

namespace egrants_new { }
//{
//    public class Startup
//    {
//        private IEnumerable<IDisposable> GetHangfireServers()
//        {
//            string conx = ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString;
//            GlobalConfiguration.Configuration
//                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
//                .UseSimpleAssemblyNameTypeSerializer()
//                .UseRecommendedSerializerSettings()
//                .UseSqlServerStorage(conx, new SqlServerStorageOptions
//                {
//                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
//                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
//                    QueuePollInterval = TimeSpan.Zero,
//                    UseRecommendedIsolationLevel = true,
//                    DisableGlobalLocks = true
//                });

//            yield return new BackgroundJobServer();
//        }



//        public void Configuration(IAppBuilder app)
//        {
//            app.UseHangfireAspNet(GetHangfireServers);
//            var options = new DashboardOptions
//            {
//                Authorization = new[]
//                {
//                    new HangfireAuthorization { Users = "shellba"}
//                }
//            };

//            app.UseHangfireDashboard("/hangfire",options);


//            string wsCronExp = ConfigurationManager.AppSettings["IntegrationCheckCronExp"];
//            string notifierCronExp = ConfigurationManager.AppSettings["NotificationCronExp"];

//            // Create the Background job
//            RecurringJob.AddOrUpdate<WsScheduleManager>(x => x.StartScheduledJobs(),wsCronExp);
//            RecurringJob.AddOrUpdate<EmailNotifier>(x => x.GenerateExceptionMessage(),notifierCronExp);

//        }


//    }

