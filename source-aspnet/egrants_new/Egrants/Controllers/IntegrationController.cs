using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using egrants_new.Egrants.Builders;
using egrants_new.Egrants.Models;
using egrants_new.Integration.EmailRulesEngine;
using egrants_new.Integration.Models;
using egrants_new.Integration.WebServices;


namespace egrants_new.Egrants.Controllers
{
    public class IntegrationController : Controller
    {
        // GET: Integration
        public ActionResult Index()
        {
            var repo = new IntegrationRepository();
            var builder = new MailIntegrationBuilder();

            var jobs = repo.GetEgrantWebServiceDueToFire();

            foreach (var job in jobs)
            {
                WebServiceHistory history;

                history = job.GetData();
                history.ResultStatusCode = HttpStatusCode.OK;
                history.Result = builder.GetGoodData(28995);
                repo.SaveData(history);
            }

            var page = new MailIntegrationPage();
            page.Result = "Temporary";
            
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

    }
}