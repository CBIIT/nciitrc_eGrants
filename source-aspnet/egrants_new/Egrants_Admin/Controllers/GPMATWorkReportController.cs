using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;
using egrants_new.Egrants_Admin.Models;

namespace egrants_new.Controllers
{
    public class GPMATWorkReportController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load reports
            ViewBag.Reports = GPMATWorkReport.LoadReports(Convert.ToString(Session["ic"]),Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/GPMATWorkReport.cshtml");
        }
    }
}

