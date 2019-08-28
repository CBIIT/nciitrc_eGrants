using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Controllers.Management
{
    public class DocTransactionReportController : Controller
    {       
        public ActionResult Index()
        {
            //load egrants specialist list
            ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(Session["ic"]));

            return View("~/Management/Views/DocTransactionReport.cshtml");
        }

        //search by click button
        public ActionResult To_Show_Report(string transaction_type, int person_id, string date_range)
        {
            //load egrants specialist list
            ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(Session["ic"]));

            ViewBag.PersonID = person_id;
            ViewBag.TransactionType = transaction_type;
            ViewBag.DateRange = date_range;

            var start_date = "";
            var end_date = "";
           
            //load docs Transaction history
            ViewBag.EgrantsDocs = Egrants.Models.EgrantsDoc.LoadDocTransactionHistory(transaction_type, person_id, start_date, end_date, date_range, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"])).AsEnumerable();

            return View("~/Management/Views/DocTransactionReport.cshtml");
        }

        public ActionResult To_Create_Report(string transaction_type, int person_id, string start_date, string end_date)
        {
            //load egrants specialist list
            ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(Session["ic"]));

            ViewBag.PersonID = person_id;
            ViewBag.TransactionType = transaction_type;
            ViewBag.StartDate = start_date;
            ViewBag.EndDate = end_date;

            //load docs Transaction history
            ViewBag.EgrantsDocs = Egrants.Models.EgrantsDoc.LoadDocTransactionHistory(transaction_type, person_id, start_date, end_date, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"])).AsEnumerable();

            return View("~/Management/Views/DocTransactionReport.cshtml");
        }    
    }
}