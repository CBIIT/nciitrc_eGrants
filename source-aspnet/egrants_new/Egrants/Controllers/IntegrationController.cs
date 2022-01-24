
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using egrants_new.Egrants.Builders;
using egrants_new.Egrants.Models;
using egrants_new.Integration.EmailRulesEngine;
using egrants_new.Integration.WebServices;

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

        public ActionResult InvokeGetAttachment(string messageId)
        {
            string outmeeage = "";
            string dir = "C:\\testing\\attachments";
            var repo = new EmailIntegrationRepository();
            
            var attach = repo.GetEmailAttachments(messageId);

            if (attach.Count > 0)
            {
                
                foreach (var att in attach)
                {
                    //Save them to disk
                    att.SaveToDisk(dir,att.Name,"file");

                }
            }
            var page = new MailIntegrationPage();
            page.Result = "Attachments Saved";

            return View("~/Egrants/Views/MailIntegrationMain.cshtml", page);
        }

    }
}