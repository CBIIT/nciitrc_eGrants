#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  Global.asax.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-03-31
// Contributors:
//      - Briggs, Robin (NIH/NCI) [C] - briggsr2
//      -
// Copyright (c) National Institute of Health
// 
// <Description of the file>
// 
// This source is subject to the NIH Softwre License.
// See https://ncihub.org/resources/899/download/Guidelines_for_Releasing_Research_Software_04062015.pdf
// All other rights reserved.
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT ARE DISCLAIMED. IN NO EVENT SHALL THE NATIONAL
// CANCER INSTITUTE (THE PROVIDER), THE NATIONAL INSTITUTES OF HEALTH, THE
// U.S. GOVERNMENT OR THE INDIVIDUAL DEVELOPERS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
// \***************************************************************************/

#endregion

#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using egrants_new.Integration.WebServices;
using egrants_new.Models;

using Hangfire;
using Hangfire.SqlServer;

#endregion

namespace egrants_new
{
    /// <summary>
    /// The mvc application.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// The ic.
        /// </summary>
        private string ic;

        /// <summary>
        /// The userid.
        /// </summary>
        private string userid;

        /// <summary>
        ///     Gets the user id.
        /// </summary>
        protected string UserID
        {
            get
                {
                    this.userid = this.Context.Request.ServerVariables["HEADER_SM_USER"];
                    if (this.userid == null) {
                        this.userid = ""; // string.Empty
                        this.userid = "hooverrl"; // string.Empty
                    }

                    return this.userid;
                }
        }

        /// <summary>
        ///     Gets the ic.
        /// </summary>
        protected string IC
        {
            get
                {
                    this.ic = this.Context.Request.ServerVariables["HEADER_USER_SUB_ORG"];

                    if (this.ic == null)
                    {
                        this.ic = "NCI"; // nci           
                    }

                    // check exception user and who's ic not as nci
                    if (this.ic != "nci" && this.ic != "NCI")
                    {
                        var userexception = EgrantsCommon.CheckUsersException(this.userid);

                        if (userexception == 1)
                        {
                            this.ic = "nci";
                        }
                    }

                    return this.ic;
                }
        }

        /// <summary>
        ///     Gets the browser type.
        /// </summary>
        protected string BrowserType => this.Request.Browser.Browser;

        /// <summary>
        ///     This event raised when the application starts up and application
        ///     domain is created.
        /// </summary>
        protected void Application_Start()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            this.Application["UsersOnline"] = 0;

            ViewEngines.Engines.Clear();

            ViewEngines.Engines.Add(new CustomRazorViewEngine());

            AreaRegistration.RegisterAllAreas();

            RouteConfig.RegisterRoutes(RouteTable.Routes);

            HangfireAspNet.Use(this.GetHangfireServers);

            var wsCronExp = ConfigurationManager.AppSettings[@"IntegrationCheckCronExp"];
            var notifierCronExp = ConfigurationManager.AppSettings[@"NotificationCronExp"];
            var sqlNotifierTime = ConfigurationManager.AppSettings[@"SQLErrorCronExp"];

            // create the Background job
            RecurringJob.AddOrUpdate<WsScheduleManager>(x => x.StartScheduledJobs(), wsCronExp);
            RecurringJob.AddOrUpdate<EmailNotifier>(x => x.GenerateExceptionMessage(), notifierCronExp);
            //RecurringJob.AddOrUpdate<EmailNotifier>(x => x.GenerateSQLJobErrorMessage(), sqlNotifierTime);
        }

        /// <summary>
        ///     The get hangfire servers.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.IEnumerable`1" /> .
        /// </returns>
        private IEnumerable<IDisposable> GetHangfireServers()
        {
            // Hangfire.
            var conx = ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString;

            GlobalConfiguration.Configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170).UseSimpleAssemblyNameTypeSerializer()
                               .UseRecommendedSerializerSettings().UseSqlServerStorage(
                                    conx,
                                    new SqlServerStorageOptions
                                        {
                                            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                                            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                                            QueuePollInterval = TimeSpan.Zero,
                                            UseRecommendedIsolationLevel = true,
                                            DisableGlobalLocks = true
                                        });

            yield return new BackgroundJobServer();
        }

        /// <summary>
        /// This event is used to determine user permissions and give
        ///     authorization rights to user.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Application_AuthorizeRequest(object sender, EventArgs e)
        {
            this.userid = this.UserID;
            this.ic = this.IC;

            // Response.Write(userid + ", " + ic);
            var uservalidation = EgrantsCommon.CheckUserValidation(this.ic, this.userid);
            if (uservalidation == 0) this.Response.Redirect("~/Shared/Views/egrants_default.htm");
        }

        /// <summary>
        /// This event raised for each time a new session begins, This is a good
        ///     place to put code that is session-specific.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started---added 11_21_2018
            var sessionId = this.Session.SessionID;

            this.Session["userid"] = this.UserID;
            this.Session["ic"] = this.IC;
            this.Session["browser"] = this.BrowserType;

            this.check_user_type();

            Egrants.Models.Egrants.UpdateLastLoginDate(this.UserID);
        }

        /// <summary>
        ///     The check_user_type.
        /// </summary>
        protected void check_user_type()
        {
            var usertype = EgrantsCommon.UserType(Convert.ToString(this.Session["ic"]), Convert.ToString(this.Session["userid"]));

            if (string.IsNullOrEmpty(usertype) || usertype == "NULL")
            {
                this.Response.Redirect("~/Shared/Views/egrants_default.htm");
            }
            else
            {
                this.check_user_validation(usertype);
            }
        }

        /// <summary>
        /// The check_user_validation.
        /// </summary>
        /// <param name="usertype">
        /// The usertype.
        /// </param>
        protected void check_user_validation(string usertype)
        {
            // set all profiles for user
            var users = EgrantsCommon.uservar(Convert.ToString(this.Session["userid"]), Convert.ToString(this.Session["ic"]), usertype);

            foreach (var usr in users)
            {
                this.Session.Add("Validation", usr.Validation);
                this.Session.Add("userid", usr.UserId);
                this.Session.Add("ic", usr.ic);
                this.Session.Add("Personid", usr.personID);
                this.Session.Add("position_id", usr.positionID);
                this.Session.Add("UserName", usr.PersonName);
                this.Session.Add("UserEmail", usr.PersonEmail);
                this.Session.Add("Menus", usr.menulist);
            }

            // check user validation
            if (this.Session["Validation"].ToString() != "OK")
            {
                this.Response.Redirect("~/Shared/Views/egrants_default.htm");
            }
            else
            {
                this.check_user_profile(usertype);
            }
        }

        /// <summary>
        /// The check_user_profile.
        /// </summary>
        /// <param name="usertype">
        /// The usertype.
        /// </param>
        protected void check_user_profile(string usertype)
        {
            // get link and server from web.config file
            // Session["server"] = ConfigurationManager.ConnectionStrings["Server"].ConnectionString;
            this.Session["webgrant"] = ConfigurationManager.ConnectionStrings["webgrant"].ConnectionString;
            this.Session["ImageServer"] = ConfigurationManager.ConnectionStrings["ImageServer"].ConnectionString;

            // for egrants
            this.Session["dashboard"] = 0;
            this.Session["egrantsDocNew"] = ConfigurationManager.ConnectionStrings["egrantsDocNew"].ConnectionString;
            this.Session["egrantsDocModify"] = ConfigurationManager.ConnectionStrings["egrantsDocModify"].ConnectionString;
            this.Session["egrantsFunding"] = ConfigurationManager.ConnectionStrings["egrantsFunding"].ConnectionString;
            this.Session["egrantsInst"] = ConfigurationManager.ConnectionStrings["egrantsInst"].ConnectionString;
            this.Session["egrantsDocEmail"] = ConfigurationManager.ConnectionStrings["egrantsDocEmail"].ConnectionString;

            this.Session["closeoutAcceptance"] = ConfigurationManager.ConnectionStrings["closeoutAcceptance"].ConnectionString;
            this.Session["frpprAcceptance"] = ConfigurationManager.ConnectionStrings["frpprAcceptance"].ConnectionString;
            this.Session["irpprAcceptance"] = ConfigurationManager.ConnectionStrings["irpprAcceptance"].ConnectionString;
        }

        /// <summary>
        /// This event raised whenever an unhandled exception occurs in the
        ///     application. This provides an opportunity to implement generic
        ///     application-wide error handling.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

            // Get the exception object.
            var exc = this.Server.GetLastError();

            // Handle HTTP errors
            if (exc.GetType() == typeof(HttpException))
            {
                // The Complete Error Handling Example generates
                // some errors using URLs with "NoCatch" in them;
                // ignore these here to simulate what would happen
                // if a global.asax handler were not implemented.
                if (exc.Message.Contains("NoCatch") || exc.Message.Contains("maxUrlLength"))
                {
                    return;
                }
                    
                // Redirect HTTP errors to HttpError page
                this.Server.Transfer("~/HttpErrorPage.aspx");
            }

            // For other kinds of errors give the user some information
            // but stay on the default page
            this.Response.Write("<h2>Global Page Error</h2>\n");
            this.Response.Write("<p>" + exc.Message + "</p>\n");
            this.Response.Write("Return to the <a href='Default.aspx'>" + "Default Page</a>\n");

            // Log the exception and notify system operators
            ExceptionUtility.LogException(exc, "~/DefaultPage");
            ExceptionUtility.NotifySystemOps(exc);

            // Clear the error from the server
            this.Server.ClearError();

            // commented out by leon 6/18/2018
            // Exception objErr = Server.GetLastError().GetBaseException();
            // string err = "Error in: " + Request.Url.ToString() + ". Error Message:" + objErr.Message.ToString();
        }

        /// <summary>
        /// This event called when session of user ends.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Session_End(object sender, EventArgs e)
        {
            this.Application.Lock();
            this.Application["UsersOnline"] = (int)this.Application["UsersOnline"] - 1;
            this.Application.UnLock();
        }

        /// <summary>
        /// The <see cref="Application_EndRequest"/> is called
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            // Iterate through any cookies found in the Response object.
            foreach (var cookieName in this.Response.Cookies.AllKeys) this.Response.Cookies[cookieName].Secure = true;
        }

        // Create our own utility for exceptions 
        /// <summary>
        /// The exception utility.
        /// </summary>
        public sealed class ExceptionUtility
        {
            // All methods are static, so this can be private 
            /// <summary>
            /// Prevents a default instance of the <see cref="ExceptionUtility"/> class from being created.
            /// </summary>
            private ExceptionUtility()
            {
            }

            // Log an Exception 
            /// <summary>
            /// The log exception.
            /// </summary>
            /// <param name="exc">
            /// The exc.
            /// </param>
            /// <param name="source">
            /// The source.
            /// </param>
            public static void LogException(Exception exc, string source)
            {
                // Include enterprise logic for logging exceptions 
                // Get the absolute path to the log file 
                var logFile = "~/App_Data/ErrorLog.txt";
                logFile = HttpContext.Current.Server.MapPath(logFile);

                // if (!File.Exists(logFile))
                // {
                // byte[] file = new byte[0];
                // File.Create(logFile);
                // }

                // Open the log file for append and write the log
                var sw = new StreamWriter(logFile, true);
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

            /// <summary>
            /// Notify System Operators about an exception
            /// </summary>
            /// <param name="exc">
            /// The exc.
            /// </param>
            public static void NotifySystemOps(Exception exc)
            {
                // Include code for notifying IT system operators
            }
        }
    }
}