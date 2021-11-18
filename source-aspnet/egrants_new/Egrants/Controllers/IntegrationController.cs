
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using egrants_new.Egrants.Builders;
using egrants_new.Egrants.Models;
using egrants_new.Integration.EmailRulesEngine;
using egrants_new.Integration.WebServices;
using Microsoft.Owin.Security.Cookies;
//using egrants_new.Integration.WebServices.SessionTokenStorage;


namespace egrants_new.Egrants.Controllers
{
    public class IntegrationController : Controller
    {
        // GET: Integration
        public ActionResult Index()
        {
            var page = new MailIntegrationPage();
            page.Result = "Start";
            
            return View("~/Egrants/Views/MailIntegrationMain.cshtml", page);
        }


        public ActionResult InvokeGraphData()
        {
            var repo = new IntegrationRepository();
            var builder = new MailIntegrationBuilder();
            var jobs = repo.GetEgrantWebServiceDueToFire();

            foreach (var job in jobs)
            {
                var histories = job.GetData();
                foreach (var history in histories)
                {
                    repo.SaveData(history);
                }

            }

            var page = new MailIntegrationPage();
            page.Result = "Process Graph Data Invoked";

            return View("~/Egrants/Views/MailIntegrationMain.cshtml", page);
        }


        public ActionResult InvokeRules()
        {

            var engine = new EmailRulesEngine();

            engine.ProcessMail();


            var page = new MailIntegrationPage();
            page.Result = "Rules Invoked";

            return View("~/Egrants/Views/MailIntegrationMain.cshtml", page);
        }


        public ActionResult InvokeActions()
        {

            var engine = new EmailRulesEngine();

            engine.ProcessPendingActions();


            var page = new MailIntegrationPage();
            page.Result = "Actions Invoked";

            return View("~/Egrants/Views/MailIntegrationMain.cshtml", page);
        }



        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                 // var owinContext = new OwinContext(context.GetOwinEnvironment());
//                IPublicClientApplication app = PublicClientApplicationBuilder.Create(clientId)
//                    .Build();

                // Signal OWIN to send an authorization request to Azure
                Request.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }


        public ActionResult SignOut()
        {
            if (Request.IsAuthenticated)
            {
                var tokenStore = new SessionTokenStorage(null,
                    System.Web.HttpContext.Current, ClaimsPrincipal.Current);

                tokenStore.Clear();

                Request.GetOwinContext().Authentication.SignOut(
                    CookieAuthenticationDefaults.AuthenticationType);
            }

            return RedirectToAction("Index", "Integration");
        }

        //public async Task<ActionResult> ReadMail()
        //{
        //    IConfidentialClientApplication app = MsalAppBuilder.BuildConfidentialClientApplication();
        //    AuthenticationResult result = null;
        //    var account = await app.GetAccountAsync(ClaimsPrincipal.Current.GetMsalAccountId());
        //    string[] scopes = { "Mail.Read" };

        //    try
        //    {
        //        // try to get token silently
        //        result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync().ConfigureAwait(false);
        //    }
        //    catch (MsalUiRequiredException)
        //    {
        //        ViewBag.Relogin = "true";
        //        return View();
        //    }
        //    catch (Exception eee)
        //    {
        //        ViewBag.Error = "An error has occurred. Details: " + eee.Message;
        //        return View();
        //    }

        //    if (result != null)
        //    {
        //        // Use the token to read email
        //        HttpClient hc = new HttpClient();
        //        hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);
        //        HttpResponseMessage hrm = await hc.GetAsync("https://graph.microsoft.com/v1.0/me/messages");

        //        string rez = await hrm.Content.ReadAsStringAsync();
        //        ViewBag.Message = rez;
        //    }

        //    return View();
        //}

    }
}