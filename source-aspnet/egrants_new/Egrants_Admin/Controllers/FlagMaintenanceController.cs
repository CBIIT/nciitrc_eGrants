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
    public class FlagMaintenanceController : Controller
    {
        // GET: FlagMaintenance
        public ActionResult Index()
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load flagtypes
            ViewBag.Flagtypes = FlagMaintenance.LoadFlagTypes();

            //load admin codes
            ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            //load flags
            var act = "show_flags";
            ViewBag.Flags = FlagMaintenance.LoadFlags(act, "", "", 0, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/FlagMaintenanceIndex.cshtml");
        }

        public ActionResult To_Search(string act, string flag_type, string admin_code, int serial_num)
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load flagtypes
            ViewBag.FlagTypes = FlagMaintenance.LoadFlagTypes();

            //load admin codes
            ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            //set default value
            ViewBag.SerialNumber = Convert.ToSingle(serial_num);
            if (flag_type == "")
            {
                ViewBag.FlagType = null;
            }
            else
            {
                ViewBag.FlagType = flag_type;
            }

            //load flags
            ViewBag.Flags = FlagMaintenance.LoadFlags(act, flag_type, admin_code, serial_num, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/FlagMaintenanceIndex.cshtml");
        }

        public ActionResult Show_Flags(string act, string flag_type)
        {
            //load searching data
            ViewBag.FlagType = flag_type;

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load flagtypes
            ViewBag.FlagTypes = FlagMaintenance.LoadFlagTypes();

            //load admin codes
            ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            //load flags
            ViewBag.Flags = FlagMaintenance.LoadFlags(act, flag_type, "", 0, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/FlagMaintenanceIndex.cshtml");
        }

        public ActionResult Show_Flag(string act, string flag_type)
        {
            //load searching data
            ViewBag.FlagType = flag_type;

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load flagtypes
            ViewBag.FlagTypes = FlagMaintenance.LoadFlagTypes();

            //load admin codes
            ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            //load flags
            ViewBag.Flags = FlagMaintenance.LoadFlags(act, flag_type, "", 0, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/FlagMaintenanceIndex.cshtml");
        }

        public ActionResult To_Setup(string flag_type)
        {
            //load act
            ViewBag.Act = "";
            if (flag_type == "")
            {
                ViewBag.FlagType = null;
            }
            else
            {
                ViewBag.FlagType = flag_type;
            }

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load flagtypes
            ViewBag.FlagTypes = FlagMaintenance.LoadFlagTypes();

            //load admin codes
            ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            return View("~/Egrants_Admin/Views/FlagMaintenanceSetup.cshtml");
        }

        public ActionResult Show_Appls(string act, int serial_number, string admin_code, string flag_type)
        {
            //load searching data
            ViewBag.Act = act;
            ViewBag.FlagType = flag_type;
            ViewBag.SerialNumber = Convert.ToString(serial_number);

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load flagtypes
            ViewBag.FlagTypes = FlagMaintenance.LoadFlagTypes();

            //load admin codes
            ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            //load appls
            ViewBag.Appls = FlagMaintenance.LoadFlags(act, flag_type, admin_code, serial_number, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/FlagMaintenanceSetup.cshtml");
        }

        public ActionResult Remove_Flags(string act, string id_string, string flag_type)
        {
            //remove flags
            FlagMaintenance.run_db(act, flag_type, "", 0, id_string, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return Show_Flags("show_flags", flag_type);
        }

        public ActionResult Setup_Flag(string act, string flag_type, string admin_code, int serial_num)
        {
            //remove flags
            FlagMaintenance.run_db(act, flag_type, admin_code, serial_num, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return Show_Flags("show_flags", flag_type);
        }

        public ActionResult Setup_Flags(string act, string flag_type, string id_string)
        {
            //remove flags
            FlagMaintenance.run_db(act, flag_type, "", 0, id_string, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return Show_Flags("show_flags", flag_type);
        }

        public ActionResult Show_Grant_Destructed()
        {
            var act = "show_grant_destructed";

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //remove grant destructed
            ViewBag.Appls = FlagMaintenance.LoadAppls(act, "", "", 0, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/GrantDestructed.cshtml");
        }

        public ActionResult Search_Grant_Destructed(string search_str)
        {
            var act = "search_grant_destructed";
            ViewBag.SearchStr = search_str;

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //remove grant destructed
            ViewBag.Appls = FlagMaintenance.LoadAppls(act, "", "", 0, search_str, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/GrantDestructed.cshtml");
        }
    }
}