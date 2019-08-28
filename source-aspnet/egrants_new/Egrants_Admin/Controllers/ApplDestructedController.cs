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
    public class ApplDestructedController : Controller
    {
        public ActionResult Index()
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load appl destructed years list
            ViewBag.Years = ApplDestructed.LoadYears();

            //load descrip codes list
            ViewBag.DescripCodes = ApplDestructed.LoadDescripCodes();

            //load exception codes list
            ViewBag.ExceptionCodes = ApplDestructed.LoadExceptionCodes();

            return View("~/Egrants_Admin/Views/ApplDestructedIndex.cshtml");
        }

        public ActionResult Search(int year, string status, string exception, string str)
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load appl destructed years list
            ViewBag.Years = ApplDestructed.LoadYears();

            //load descrip codes list
            ViewBag.DescripCodes = ApplDestructed.LoadDescripCodes();

            //load exception codes list
            ViewBag.ExceptionCodes = ApplDestructed.LoadExceptionCodes();

            //get searching variable
            ViewBag.SearchYear = year;
        
            if (status != "")
            {
                ViewBag.StatusCode = status;
            }

            if (exception != "")
            {
                ViewBag.ExceptionCode = exception;
            }

            if (str != "")
            {
                ViewBag.Str = str;
            }

            //check access permission
            ViewBag.Processable = ApplDestructed.CheckPermission(year, Convert.ToString(Session["userid"]));

            //load search info
            ViewBag.SearchInfo = ApplDestructed.LoadSearchInfo(year, status, exception, str);

            //load appls
            ViewBag.Appls = ApplDestructed.LoadAppls("", year, status, exception, str, "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/ApplDestructedIndex.cshtml");
        }

        public ActionResult Modify(string act, int year, string status, string exception, string str, string id_string, string exception_type)
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load appl destructed years list
            ViewBag.Years = ApplDestructed.LoadYears();

            //get searching variable
            ViewBag.SearchYear = year;

            if (status != "")
            {
                ViewBag.StatusCode = status;
            }

            if (exception != "")
            {
                ViewBag.ExceptionCode = exception;
            }

            if (str != "")
            {
                ViewBag.Str = str;
            }
          
            //modify data and load appls
            ViewBag.Appls = ApplDestructed.LoadAppls(act, year, status, exception, str, id_string, exception_type, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load search info
            ViewBag.SearchInfo = ApplDestructed.LoadSearchInfo(year, status, exception, str);
            
            //load descrip codes list
            ViewBag.DescripCodes = ApplDestructed.LoadDescripCodes();

            //load exception codes list
            ViewBag.ExceptionCodes = ApplDestructed.LoadExceptionCodes();

            //check access permission
            ViewBag.Processable = ApplDestructed.CheckPermission(year, Convert.ToString(Session["userid"]));
   
            return View("~/Egrants_Admin/Views/ApplDestructedIndex.cshtml");
        }

        public ActionResult Show_Exception_Code()
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load exception codes list
            ViewBag.ExceptionCodes = ApplDestructed.LoadExceptionCodes();

            return View("~/Egrants_Admin/Views/ApplDestructedEdit.cshtml");
        }

        public ActionResult Edit_Exception_Code(string act, int id, string detail, string code)
        {
            //act could be create, edit or delete
            ApplDestructed.EditExceptionCode(act, id, detail, code, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load exception codes list
            ViewBag.ExceptionCodes = ApplDestructed.LoadExceptionCodes();

            return View("~/Egrants_Admin/Views/ApplDestructedEdit.cshtml");
        }
    }
}

