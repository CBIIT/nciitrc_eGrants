using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;
using egrants_new.Dashboard.Functions;


namespace egrants_new.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {       
                Session["dashboard"] = 1;
                var act = "get_assignment";
                var idstr = "";
                //ViewBag.ICList = Egrants.Models.Egrants.GetICList();
                ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

                //get GetTotalWidget
                ViewBag.TotalWidgets = DashboardFunctions.GetTotalWidgets();

                //load default org
                ViewBag.Widgets = DashboardFunctions.LoadWidgets(act, idstr, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

                //load user selected Widgets
                ViewBag.SelectedWidgets = DashboardFunctions.LoadSeletedWidgets(Convert.ToString(Session["userid"]));

                //load link list
                ViewBag.LinkLists = DashboardFunctions.LoadLinkList();

                //load grants togo cc
                ViewBag.GrantsTogoCC =DashboardFunctions.LoadGrantsTogoCC(Convert.ToString(Session["userid"]), "cc");

                //load grants togo nc
                ViewBag.GrantsTogoNC =DashboardFunctions.LoadGrantsTogoNC(Convert.ToString(Session["userid"]), "nc");

                //load grants delayed
                ViewBag.GrantsExpedited =DashboardFunctions.LoadGrantsExpedited(Convert.ToString(Session["userid"]));

                //load late grants 
                ViewBag.GrantsDelayed =DashboardFunctions.LoadGrantsDelayed(Convert.ToString(Session["userid"]));

                //load new grants 
                ViewBag.GrantsNew =DashboardFunctions.LoadGrantsNew(Convert.ToString(Session["userid"]), "");

                //load Avgtime
                ViewBag.Avgtime =DashboardFunctions.LoadAvgtime(Convert.ToString(Session["userid"]));

                //load Grants Status 
                ViewBag.GrantsStatus =DashboardFunctions.LoadGrantsStatus();

                //load audit report
                ViewBag.AuditReport =DashboardFunctions.LoadAuditReport();

                return View("~/Views/Dashboard/Index.cshtml");
        }

        public ActionResult Save_Selection(string act, string idstr)
        {
            //save selection
           DashboardFunctions.save_selected(act, idstr, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return Index();
        }

        public ActionResult Reminder_Load()
        {
            return View("~/Views/Dashboard/Reminder.cshtml");
        }
    }
}