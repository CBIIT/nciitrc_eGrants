using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using System.Web.Services;
using egrants_new.Integration.Models;
using egrants_new.Integration.WebServices;

namespace egrants_new.Integration.Controllers
{
    public class IntegrationController : Controller
    {


        [HttpGet]
        public ActionResult Trigger(int webServiceId, string triggerAuth="")
        {
            //Just for testing this will be abstracted behind another class, nice neat.
            //The endpoint will be retrieved from the database and instantiated by a factory method off of the Import
            //Repository class
            IntegrationRepository ir = new IntegrationRepository();
            var data = "";
            
            if (TriggerAuthorized(webServiceId, triggerAuth))
            {
                CallWebService(webServiceId);

            }
            else
            {
                //Service Trigger Not Authorized
            }
            //TODO: Process the data

            return View();
        }

        private bool TriggerAuthorized(int serviceId, string triggerAuth)
        {
            //TODO:  Set database check of Authorization secret
            return true;
        }

        private void CallWebService(int webServiceId)
        {
            IntegrationRepository repo = new IntegrationRepository();
            //WebServiceHistory history = new WebServiceHistory();

            var ws = repo.GetEgrantWebService(webServiceId);
            var histories = ws.GetData();

            foreach( var history in histories)
            {
            repo.SaveData(history);
            }


        }

    }
}