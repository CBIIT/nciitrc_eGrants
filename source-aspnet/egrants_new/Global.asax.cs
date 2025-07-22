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
using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using egrants_new.Integration.WebServices;
using egrants_new.Models;

using Octokit;
using Hangfire;
using Hangfire.SqlServer;

//using Microsoft.ApplicationInsights.Extensibility;

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
                if (this.userid == null)
                {
                    this.userid = "";
#if DEBUG

                    this.userid = "hooverrl"; // should correspond to person table, column: active
 #endif
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
                    var usersException = EgrantsCommon.CheckUsersException(this.userid);

                    if (usersException == 1)
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

            //TelemetryConfiguration.Active.DisableTelemetry = true;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            this.Application["UsersOnline"] = 0;

            ViewEngines.Engines.Clear();

            ViewEngines.Engines.Add(new CustomRazorViewEngine());

            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //  HangfireAspNet.Use(this.GetHangfireServers);

            // var wsCronExp = ConfigurationManager.AppSettings[@"IntegrationCheckCronExp"];
            // var notifierCronExp = ConfigurationManager.AppSettings[@"NotificationCronExp"];
            // var sqlNotifierTime = ConfigurationManager.AppSettings[@"SQLErrorCronExp"];

            // create the Background job
            // RecurringJob.AddOrUpdate<WsScheduleManager>(x => x.StartScheduledJobs(), wsCronExp);
            // RecurringJob.AddOrUpdate<EmailNotifier>(x => x.GenerateExceptionMessage(), notifierCronExp);
            //RecurringJob.AddOrUpdate<EmailNotifier>(x => x.GenerateSQLJobErrorMessage(), sqlNotifierTime);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            SameSiteCookieRewriter.FilterSameSiteNoneForIncompatibleUserAgents(sender);
        }

        /// <summary>
        ///     The get hangfire servers.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.IEnumerable`1" /> .
        /// </returns>
        private IEnumerable<IDisposable> GetHangfireServers()
        {
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
            this.Response.Headers.Remove("Server: Microsoft-IIS/8.5");
            this.Response.Headers.Remove("X-AspNetMvc-Version: 5.2");
            this.Response.Headers.Remove("X-AspNet-Version: 4.0.30319");
            this.Response.Headers.Remove("X-UA-Compatible: IE=Edge");

            this.userid = this.UserID;
            this.ic = this.IC;

            // Response.Write(userid + ", " + ic);
            var userValidation = EgrantsCommon.CheckUserValidation(this.ic, this.userid);

            if (userValidation == 0)
            {
                this.Response.Redirect("~/Shared/Views/egrants_default.htm");
            }
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
            //this.Session.Timeout = 5;
            // Code that runs when a new session is started---added 11_21_2018
            var sessionId = this.Session.SessionID;
            this.Session["GitHubToken"] = ConfigurationManager.ConnectionStrings["GitHubToken"].ConnectionString;
            string token = this.Session["GitHubToken"].ToString();
            var latestReleaseFull = GetLatestReleaseTagAsync("CBIIT", "nciitrc_eGrants", token);
            var latestRelease = latestReleaseFull.Split(' ')[0];
            this.Session.Add("Release", latestRelease);
        
            this.Session["userid"] = this.UserID;
            this.Session["ic"] = this.IC;
            this.Session["browser"] = this.BrowserType;
            this.Session["CurrentView"] = "standardForm";
        
            var usertype = EgrantsCommon.UserType(Convert.ToString(this.Session["ic"]), Convert.ToString(this.Session["userid"]));
        
        
            if (string.IsNullOrEmpty(usertype) || usertype == "NULL")
            {
                this.Response.Redirect("~/Shared/Views/egrants_default.htm");
        
                return;
            }
        
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
        
                return;
            }
        
            // egrants-file-dev.nci.nih.gov
            this.Session["WebGrantUrl"] = ConfigurationManager.ConnectionStrings["WebGrantUrl"].ConnectionString;
        
            // egrants/funded2/nci/main/
            this.Session["WebGrantRelativePath"] = ConfigurationManager.ConnectionStrings["WebGrantRelativePath"].ConnectionString;
        
            // https://egrants-web-dev.nci.nih.gov/
            // this.Session["EgrantsUrl"] = ConfigurationManager.ConnectionStrings["EgrantsUrl"].ConnectionString;
        
            // https://egrants-web-dev.nci.nih.gov/
            this.Session["ImageServerUrl"] = ConfigurationManager.ConnectionStrings["ImageServerUrl"].ConnectionString;
        
            // for egrants
            this.Session["dashboard"] = 0;
        
            // data/funded2/nci/main/
            this.Session["EgrantsDocNewRelativePath"] = ConfigurationManager.ConnectionStrings["EgrantsDocNewRelativePath"].ConnectionString;
        
            // data/funded/nci/modify/
            this.Session["EgrantsDocModifyRelativePath"] = ConfigurationManager.ConnectionStrings["EgrantsDocModifyRelativePath"].ConnectionString;
        
            // funded/nci/funding/upload/
            this.Session["EgrantsFundingRelativePath"] = ConfigurationManager.ConnectionStrings["EgrantsFundingRelativePath"].ConnectionString;
        
            // funded/nci/institutional/
            this.Session["EgrantsInstRelativePath"] = ConfigurationManager.ConnectionStrings["EgrantsInstRelativePath"].ConnectionString;
        
            // NCIeGrantsDev@mail.nih.gov
            this.Session["EgrantsDocEmail"] = ConfigurationManager.ConnectionStrings["EgrantsDocEmail"].ConnectionString;
        
            this.Session["closeoutAcceptance"] = ConfigurationManager.ConnectionStrings["closeoutAcceptance"].ConnectionString;
            this.Session["frpprAcceptance"] = ConfigurationManager.ConnectionStrings["frpprAcceptance"].ConnectionString;
            this.Session["irpprAcceptance"] = ConfigurationManager.ConnectionStrings["irpprAcceptance"].ConnectionString;
        
            // session is started so update the last login date
            EgrantsCommon.UpdateUsersLastLoginDate(this.userid);
        }
        
        public string GetLatestReleaseTagAsync(string owner, string repoName, string token)
        {
            try
            {
                var client = new GitHubClient(new ProductHeaderValue("eGrants"));
                var tokenAuth = new Credentials(token);
                client.Credentials = tokenAuth;
                var latestRelease = client.Repository.Release.GetLatest(owner, repoName).Result;
                return latestRelease.Name;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving release information: {ex.Message}");
                return "Unknown Release"; // Or handle as appropriate    }
            }
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
            this.Session.RemoveAll();
            this.Application.UnLock();

            Console.WriteLine("Session Ended!");

            // response object is not available at this point
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
            foreach (var cookieName in this.Response.Cookies.AllKeys)
            {
                Response.Cookies[cookieName].Secure = true;
                Response.Cookies[cookieName].SameSite = SameSiteMode.Lax;
                Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
            }
        }
    }
}
