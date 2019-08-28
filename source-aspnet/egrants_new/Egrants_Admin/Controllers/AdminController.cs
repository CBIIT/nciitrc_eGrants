using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));
            return View("~/Egrants_Admin/Views/Index.cshtml");          
        }

        //public ActionResult CheckPermission(int id, string action)
        //{
        //    ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

        //    //check run admin menu permission by userid and menu_id
        //    var Permission = EgrantsCommon.CheckAdminMenuPermission(id, Convert.ToString(Session["userid"]));
        //    if (Permission == 0)
        //    {
        //        return View("~/Egrants_Admin/Views/AdminMenuDefault.cshtml");
        //    }
        //    else
        //    {
        //        return RedirectToAction(action,"Egrants_Admin");
        //    }
        //}
    }
}

