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
    public class DocmanAccessController : Controller
    {
        public ActionResult Index(int index_id, int active_id)
        {
            //save info
            ViewBag.IndexID = index_id;
            ViewBag.ActiveID = active_id;

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load Character Index
            ViewBag.CharacterIndex = EgrantsCommon.LoadCharacterIndex();

            //load user list
            ViewBag.LoadUsers = DocmanAccess.LoadUsers("load", index_id, active_id, 0, "", "", "", "", "", "", 0, 0, 1, 0, 0, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/DocmanAccessIndex.cshtml");
        }

        public ActionResult To_Add()
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load profiles
            ViewBag.Profiles = EgrantsCommon.LoadProfiles();

            //Load Coordinators
            ViewBag.Coordinators = DocmanCommon.LoadCoordinators();

            return View("~/Egrants_Admin/Views/DocmanAccessCreate.cshtml");
        }

        public ActionResult To_Create(string act, int active_id, string login_id, string first_name, string last_name, string middle_name, string email_address, string phone_num, int docman_tab, int cft_tab,  int position_id, int coordinator_id, int is_coordinator, string end_date)
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //run db and create user data                  
            DocmanAccess.run_db(act, 0, active_id, 0, login_id, last_name, first_name, middle_name, email_address, phone_num, coordinator_id, position_id, docman_tab, cft_tab, is_coordinator, end_date, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //return default index
            return Index(2, 1);
        }

        public ActionResult To_Search(int person_id)
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load profiles
            ViewBag.Profiles = EgrantsCommon.LoadProfiles();

            //Load Coordinators
            ViewBag.Coordinators =DocmanCommon.LoadCoordinators();

            //load user's  data
            ViewBag.User = DocmanAccess.LoadUsers("search", 0, 0, person_id, "", "", "", "", "", "", 0, 0, 1, 0, 0, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/DocmanAccessUpdate.cshtml");
        }

        public ActionResult To_Update(string act, int user_id, string login_id, string first_name, string last_name, string middle_name, string email_address, string phone_num, int docman_tab, int cft_tab, int position_id, int coordinator_id, int is_coordinator, string end_date)
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //run db and update user data                  
            DocmanAccess.run_db(act, 0, 0, user_id, login_id, last_name, first_name, middle_name, email_address, phone_num, coordinator_id, position_id, docman_tab, cft_tab, is_coordinator, end_date, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //return default index
            return Index(2, 1);
        }

        public ActionResult To_Change_Status(string act, int user_id)
        {
            //run db and update user status
            DocmanAccess.run_db(act, 0, 0, user_id, "", "", "", "", "", "", 0, 0, 1,  0, 0, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return To_Search(user_id);
        }
    }
}