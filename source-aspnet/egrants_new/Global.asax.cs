using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;
using System.Net;
using System.Web.Http;
using Hangfire;
using Hangfire.SqlServer;
using egrants_new.Integration.WebServices;
//using egrants_new.App_Start;


namespace egrants_new
{
    public class MvcApplication : System.Web.HttpApplication
    {
        string userid;
        string ic;

        //This event raised when the application starts up and application domain is created.
        protected void Application_Start()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Application["UsersOnline"] = 0;
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new CustomRazorViewEngine());
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            HangfireAspNet.Use(GetHangfireServers);
            var wsCronExp = ConfigurationManager.AppSettings["IntegrationCheckCronExp"];
            var notifierCronExp = ConfigurationManager.AppSettings["NotificationCronExp"];
            var sqlNotifierTime = ConfigurationManager.AppSettings["SQLErrorCronExp"];

            //       /// Create the Background job
            RecurringJob.AddOrUpdate<WsScheduleManager>(x => x.StartScheduledJobs(), wsCronExp);
            //RecurringJob.AddOrUpdate<EmailNotifier>(x => x.GenerateExceptionMessage(), notifierCronExp);
            //RecurringJob.AddOrUpdate<EmailNotifier>(x => x.GenerateSqlJobErrorMessage(), sqlNotifierTime);
            var wsMgr = new WsScheduleManager();

            wsMgr.StartScheduledJobs();
        }

        protected string UserID
        {
            get
            {
                userid = Context.Request.ServerVariables["HEADER_SM_USER"];
                if (userid == null)
                {
                    userid = "shellba";    //qians
                }
                return userid;
            }
        }
        private IEnumerable<IDisposable> GetHangfireServers()
        {
            string conx = ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString;

            Hangfire.GlobalConfiguration.Configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(conx, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            });

            yield return new BackgroundJobServer();
        }

        protected string IC
        {
            get
            {
                ic = Context.Request.ServerVariables["HEADER_USER_SUB_ORG"];
                if (ic == null)
                {
                    ic = "NCI";        //nci
                }

                //check exception user and who's ic not as nci
                if (ic == "nci" || ic == "NCI") return ic;
                int userexception = EgrantsCommon.CheckUsersException(userid);
                if (userexception == 1)
                {
                    ic = "nci";
                }
                return ic;
            }
        }

        protected string BrowserType => Request.Browser.Browser.ToString();

        //This event is used to determine user permissions and give authorization rights to user. 
        protected void Application_AuthorizeRequest(Object sender, EventArgs e)
        {
            userid = UserID;
            ic = IC;
            //Response.Write(userid + ", " + ic);
            int uservalidation = EgrantsCommon.CheckUserValidation(ic, userid);
            if (uservalidation == 0)
            {
                Response.Redirect("~/Shared/Views/egrants_default.htm");
            }
        }

        //This event raised for each time a new session begins, This is a good place to put code that is session-specific.
        protected void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started---added 11_21_2018
            string sessionId = Session.SessionID;

            Session["userid"] = UserID;
            Session["ic"] = IC;
            Session["browser"] = BrowserType;

            check_user_type();
        }

        protected void check_user_type()
        {
            var usertype = Models.EgrantsCommon.UserType(Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
            //Session.Add("UserType", usertype);
            if (string.IsNullOrEmpty(usertype) || usertype == "NULL")
            {
                Response.Redirect("~/Shared/Views/egrants_default.htm");
            }
            else
            {
                check_user_validation(usertype);
            }
        }

        protected void check_user_validation(string usertype)
        {
            //set all profiles for user
            IEnumerable<Models.EgrantsCommon.User> users =
                Models.EgrantsCommon.uservar(Convert.ToString(Session["userid"]), Convert.ToString(Session["ic"]),
                    usertype);
            foreach (var usr in users)
            {
                Session.Add("Validation", usr.Validation);
                Session.Add("userid", usr.UserId);
                Session.Add("ic", usr.ic);
                Session.Add("Personid", usr.personID);
                Session.Add("position_id", usr.positionID);
                Session.Add("UserName", usr.PersonName);
                Session.Add("UserEmail", usr.PersonEmail);
                Session.Add("Menus", usr.menulist);
            }

            //check user validation
            if (Session["Validation"].ToString() != "OK")
            {
                Response.Redirect("~/Shared/Views/egrants_default.htm");
            }
            else
            {
                check_user_profile(usertype);
            }

        }

        protected void check_user_profile(string usertype)
        {
            //get link and server from web.config file
            //Session["server"] = ConfigurationManager.ConnectionStrings["Server"].ConnectionString;
            Session["webgrant"] = ConfigurationManager.ConnectionStrings["webgrant"].ConnectionString;
            Session["ImageServer"] = ConfigurationManager.ConnectionStrings["ImageServer"].ConnectionString;

            //for egrants
            Session["dashboard"] = 0;
            Session["egrantsDocNew"] = ConfigurationManager.ConnectionStrings["egrantsDocNew"].ConnectionString;
            Session["egrantsDocModify"] = ConfigurationManager.ConnectionStrings["egrantsDocModify"].ConnectionString;
            Session["egrantsFunding"] = ConfigurationManager.ConnectionStrings["egrantsFunding"].ConnectionString;
            Session["egrantsInst"] = ConfigurationManager.ConnectionStrings["egrantsInst"].ConnectionString;
            Session["egrantsDocEmail"] = ConfigurationManager.ConnectionStrings["egrantsDocEmail"].ConnectionString;

            Session["closeoutAcceptance"] = ConfigurationManager.ConnectionStrings["closeoutAcceptance"].ConnectionString;
            Session["frpprAcceptance"] = ConfigurationManager.ConnectionStrings["frpprAcceptance"].ConnectionString;
            Session["irpprAcceptance"] = ConfigurationManager.ConnectionStrings["irpprAcceptance"].ConnectionString;

        }

        //This event raised whenever an unhandled exception occurs in the application. This provides an opportunity to implement generic application-wide error handling.
        protected void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

            // Get the exception object.
            Exception exc = Server.GetLastError();

            // Handle HTTP errors
            if (exc.GetType() == typeof(HttpException))
            {
                // The Complete Error Handling Example generates
                // some errors using URLs with "NoCatch" in them;
                // ignore these here to simulate what would happen
                // if a global.asax handler were not implemented.
                if (exc.Message.Contains("NoCatch") || exc.Message.Contains("maxUrlLength"))
                    return;

                //Redirect HTTP errors to HttpError page
                Server.Transfer("HttpErrorPage.aspx");
            }

            // For other kinds of errors give the user some information
            // but stay on the default page
            Response.Write("<h2>Global Page Error</h2>\n");
            Response.Write("<p>" + exc.Message + "</p>\n");
            Response.Write("Return to the <a href='Default.aspx'>" + "Default Page</a>\n");

            // Log the exception and notify system operators
            ExceptionUtility.LogException(exc, "DefaultPage");
            ExceptionUtility.NotifySystemOps(exc);

            // Clear the error from the server
            Server.ClearError();

            //commented out by leon 6/18/2018
            //Exception objErr = Server.GetLastError().GetBaseException();
            //string err = "Error in: " + Request.Url.ToString() + ". Error Message:" + objErr.Message.ToString();
        }

        //This event called when session of user ends.
        protected void Session_End(object sender, EventArgs e)
        {
            Application.Lock();
            Application["UsersOnline"] = (int)Application["UsersOnline"] - 1;
            Application.UnLock();
        }

        // Create our own utility for exceptions 
        public sealed class ExceptionUtility
        {
            // All methods are static, so this can be private 
            private ExceptionUtility()
            { }

            // Log an Exception 
            public static void LogException(Exception exc, string source)
            {
                // Include enterprise logic for logging exceptions 
                // Get the absolute path to the log file 
                string logFile = "App_Data/ErrorLog.txt";
                logFile = HttpContext.Current.Server.MapPath(logFile);

                //if (!File.Exists(logFile))
                //{
                //    byte[] file = new byte[0];
                //    File.Create(logFile);
                //}

                // Open the log file for append and write the log
                StreamWriter sw = new StreamWriter(logFile, true);
                sw.WriteLine("********** {0} **********", DateTime.Now);
                if (exc.InnerException != null)
                {
                    sw.Write("Inner Exception Type: ");
                    sw.WriteLine(exc.InnerException.GetType().ToString());
                    sw.Write("Inner Exception: ");
                    sw.WriteLine(exc.InnerException.Message);
                    sw.Write("Inner Source: ");
                    sw.WriteLine(exc.InnerException.Source);
                    if (exc.InnerException.StackTrace != null)
                    {
                        sw.WriteLine("Inner Stack Trace: ");
                        sw.WriteLine(exc.InnerException.StackTrace);
                    }
                }
                sw.Write("Exception Type: ");
                sw.WriteLine(exc.GetType().ToString());
                sw.WriteLine("Exception: " + exc.Message);
                sw.WriteLine("Source: " + source);
                sw.WriteLine("Stack Trace: ");
                if (exc.StackTrace != null)
                {
                    sw.WriteLine(exc.StackTrace);
                    sw.WriteLine();
                }
                sw.Close();
            }

            // Notify System Operators about an exception 
            public static void NotifySystemOps(Exception exc)
            {
                // Include code for notifying IT system operators
            }
        }
    }
}
