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
    public class CategoryEditController : Controller
    {      
        public ActionResult Index()
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //Load Common Categroies list
            ViewBag.CommonCategroies = CategoryEdit.LoadCommonCategroies(Convert.ToString(Session["ic"]));

            //Load local Categroies list
            ViewBag.LocalCategroies = CategoryEdit.LoadLocalCategroies(Convert.ToString(Session["ic"]));

            //return View
            ViewBag.Message = "";
            return View("~/Egrants_Admin/Views/CategoryEditIndex.cshtml");
        }

        public ActionResult To_Move(string category_id)
        {
            var act = "remove_out";
            int categoryid = Convert.ToInt32(category_id);
            var category_name = "";
            ViewBag.Message = CategoryEdit.run_db(act, categoryid, category_name, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //Load Common Categroies list
            ViewBag.CommonCategroies = CategoryEdit.LoadCommonCategroies(Convert.ToString(Session["ic"]));

            //Load local Categroies list
            ViewBag.LocalCategroies = CategoryEdit.LoadLocalCategroies(Convert.ToString(Session["ic"]));

            //return View
            return View("~/Egrants_Admin/Views/CategoryEditIndex.cshtml");
        }

        public ActionResult To_Add(string category_id)
        {
            var act = "add_in";
            int categoryid = Convert.ToInt32(category_id);
            var category_name = "";
            ViewBag.Message = CategoryEdit.run_db(act, categoryid, category_name, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //Load Common Categroies list
            ViewBag.CommonCategroies = CategoryEdit.LoadCommonCategroies(Convert.ToString(Session["ic"]));

            //Load local Categroies list
            ViewBag.LocalCategroies = CategoryEdit.LoadLocalCategroies(Convert.ToString(Session["ic"]));

            //return View
            return View("~/Egrants_Admin/Views/CategoryEditIndex.cshtml");
        }

        public ActionResult To_Create(string category_name)
        {
            var act = "create_new";
            var category_id = 0;
            ViewBag.Message = CategoryEdit.run_db(act, category_id, category_name, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //Load Common Categroies list
            ViewBag.CommonCategroies = CategoryEdit.LoadCommonCategroies(Convert.ToString(Session["ic"]));

            //Load local Categroies list
            ViewBag.LocalCategroies = CategoryEdit.LoadLocalCategroies(Convert.ToString(Session["ic"]));

            //return View
            return View("~/Egrants_Admin/Views/CategoryEditIndex.cshtml");
        }      
    }
}