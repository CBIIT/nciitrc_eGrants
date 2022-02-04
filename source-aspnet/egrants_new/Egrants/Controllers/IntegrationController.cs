
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using egrants_new.Egrants.Builders;
using egrants_new.Egrants.Models;
using egrants_new.Integration.EmailRulesEngine;
using egrants_new.Integration.EmailRulesEngine.Models;
using egrants_new.Integration.WebServices;

//using egrants_new.Integration.WebServices.SessionTokenStorage;


namespace egrants_new.Egrants.Controllers
{
    public class IntegrationController : Controller
    {
        // GET: Integration
        public ActionResult Index()
        {
            var repo = new EmailIntegrationRepository();
            var page = new MailIntegrationPage();
            page.Result = "Start";

            var rule = repo.GetEmailRules().Where(r => r.Id == 4).FirstOrDefault();
            var msgs = repo.GetEmailMessages(rule);
            page.Messages = msgs;

            if (msgs.Count > 0)
            {
               int numMsgs = msgs.Count > 400 ? 400 : msgs.Count;
                page.Messages = msgs.GetRange(0,numMsgs);
            }

            
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
            page.Messages = new List<EmailMsg>();

            return RedirectToAction("Index"); //View("~/Egrants/Views/MailIntegrationMain.cshtml", page);
        }


        public ActionResult InvokeRules()
        {

            var engine = new EmailRulesEngine();

            engine.ProcessMail();


            var page = new MailIntegrationPage();
            page.Result = "Rules Invoked";
            page.Messages = new List<EmailMsg>();

            return RedirectToAction("Index");//return View("~/Egrants/Views/MailIntegrationMain.cshtml", page);
        }


        public ActionResult InvokeActions()
        {

            var engine = new EmailRulesEngine();

            engine.ProcessPendingActions();


            var page = new MailIntegrationPage();
            page.Result = "Actions Invoked";
            page.Messages = new List<EmailMsg>();

            return  RedirectToAction("Index");   //View("~/Egrants/Views/MailIntegrationMain.cshtml", page);
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
            page.Messages = new List<EmailMsg>();

            return RedirectToAction("Index"); //View("~/Egrants/Views/MailIntegrationMain.cshtml", page);
        }

        public ActionResult ProcessEmailMessage(int messageId, int ruleId = 0)
        {

            var engine = new EmailRulesEngine();

            engine.ProcessMessage(messageId,ruleId);

            var repo = new EmailIntegrationRepository();


            var page = new MailIntegrationPage();
            page.Result = $"Processed MessageId {messageId}";
            page.Messages = new List<EmailMsg>();

            return RedirectToAction("Index");//return View("~/Egrants/Views/MailIntegrationMain.cshtml", page);
            


        }

    }
}