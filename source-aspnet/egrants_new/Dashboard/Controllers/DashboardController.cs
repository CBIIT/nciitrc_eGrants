using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;
using egrants_new.Egrants.Models;
using egrants_new.Dashboard.Models;

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
                ViewBag.TotalWidgets = Dashboard.Models.Dashboard.GetTotalWidgets();

                //load default org
                ViewBag.Widgets = Dashboard.Models.Dashboard.LoadWidgets(act, idstr, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

                //load user selected Widgets
                ViewBag.SelectedWidgets = Dashboard.Models.Dashboard.LoadSeletedWidgets(Convert.ToString(Session["userid"]));

                //load link list
                ViewBag.LinkLists = Dashboard.Models.Dashboard.LoadLinkList();

                //load grants togo cc
                ViewBag.GrantsTogoCC = Dashboard.Models.Dashboard.LoadGrantsTogoCC(Convert.ToString(Session["userid"]), "cc");

                //load grants togo nc
                ViewBag.GrantsTogoNC = Dashboard.Models.Dashboard.LoadGrantsTogoNC(Convert.ToString(Session["userid"]), "nc");

                //load grants delayed
                ViewBag.GrantsExpedited = Dashboard.Models.Dashboard.LoadGrantsExpedited(Convert.ToString(Session["userid"]));

                //load late grants 
                ViewBag.GrantsDelayed = Dashboard.Models.Dashboard.LoadGrantsDelayed(Convert.ToString(Session["userid"]));

                //load new grants 
                ViewBag.GrantsNew = Dashboard.Models.Dashboard.LoadGrantsNew(Convert.ToString(Session["userid"]), "");

                //load Avgtime
                ViewBag.Avgtime = Dashboard.Models.Dashboard.LoadAvgtime(Convert.ToString(Session["userid"]));

                //load Grants Status 
                ViewBag.GrantsStatus = Dashboard.Models.Dashboard.LoadGrantsStatus();

                //load audit report
                ViewBag.AuditReport = Dashboard.Models.Dashboard.LoadAuditReport();

                return View("~/Dashboard/Views/Index.cshtml");
        }

        public ActionResult Save_Selection(string act, string idstr)
        {
            //save selection
            Dashboard.Models.Dashboard.save_selected(act, idstr, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return Index();
        }

        public ActionResult Reminder_Load()
        {
            return View("~/Dashboard/Views/Reminder.cshtml");
        }
    }
}