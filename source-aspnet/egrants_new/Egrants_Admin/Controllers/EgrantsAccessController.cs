using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using egrants_new.Models;
using egrants_new.Egrants_Admin.Models;

namespace egrants_new.Controllers
{
    public class EgrantsAccessController : Controller
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
            ViewBag.LoadUsers = EgrantsAccess.LoadUsers("load", index_id, active_id, 0, "", "", "", "", "", "", 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/EgrantsAccessIndex.cshtml");
        }

        //load all appls list with or without documents
        public int To_Check_Userid(string userid)
        {
            int count_userid = EgrantsAccess.ToCheckUserid(userid);
            //JavaScriptSerializer js = new JavaScriptSerializer();
            //return js.Serialize(Convert.ToInt16(count_userid));
            return count_userid;
        }

        public ActionResult To_Search(int person_id)
        {   
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load profiles
            ViewBag.Profiles = EgrantsCommon.LoadProfiles();

            //load positions
            ViewBag.Positions = EgrantsCommon.LoadPositions();

            //Load Coordinators
            ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();

            //load user data 
            ViewBag.User = EgrantsAccess.LoadUsers("search", 0, 0, person_id, "", "", "", "", "", "", 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/EgrantsAccessUpdate.cshtml");
        }

        [HttpGet]
        public ActionResult To_Add()
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load profiles
            ViewBag.Profiles = EgrantsCommon.LoadProfiles();

            //load positions
            ViewBag.Positions = EgrantsCommon.LoadPositions();

            //Load Coordinators
            ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();

            //Load user data
            ViewBag.User = null;

            return View("~/Egrants_Admin/Views/EgrantsAccessCreate.cshtml");
        }

        public ActionResult To_Update(string act, int user_id, string login_id, string first_name, string last_name, string middle_name, string email_address, string phone_num, int egrants_tab, int mgt_tab, int admin_tab, int docman_tab, int cft_tab, int dashboard_tab, int iccoord_tab, int ic_id, int position_id, int coordinator_id, int is_coordinator, string end_date)
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //run db and update user data --commented by Leon 6/4/2019                     
            //EgrantsAccess.run_db(act, 0, 0, user_id, login_id, last_name, first_name, middle_name, email_address, phone_num, coordinator_id, position_id, ic_id, egrants_tab, mgt_tab, admin_tab, docman_tab, cft_tab, dashboard_tab, iccoord_tab, is_coordinator, end_date, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //get return notice
            string return_notice = EgrantsAccess.to_preview(act, 0, 0, user_id, login_id, last_name, first_name, middle_name, email_address, phone_num, coordinator_id, position_id, ic_id, egrants_tab, mgt_tab, admin_tab, docman_tab, cft_tab, dashboard_tab, iccoord_tab, is_coordinator, end_date, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //user data has been updated
            if (return_notice == "done")
            {
                //get the first letter from last name
                var first_letter = last_name.Substring(0, 1);
                ViewBag.FirstLetter = first_letter;
                int index_id = EgrantsAccess.getCharacterIndex(first_letter);

                //return default index
                return Index(index_id, 1);
            }
            else
            {
                //user data duplicate and return error message
                ViewBag.ReturnNotice = return_notice;

                //load profiles
                ViewBag.Profiles = EgrantsCommon.LoadProfiles();

                //load positions
                ViewBag.Positions = EgrantsCommon.LoadPositions();

                //Load Coordinators
                ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();

                //load user data 
                ViewBag.User = EgrantsAccess.LoadUsers("search", 0, 0, user_id, "", "", "", "", "", "", 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

                return View("~/Egrants_Admin/Views/EgrantsAccessUpdate.cshtml");
            }
        }

        public ActionResult To_Create(string act, int active_id, string login_id, string first_name, string last_name, string middle_name, string email_address, string phone_num, int egrants_tab, int mgt_tab, int admin_tab, int docman_tab, int cft_tab, int dashboard_tab, int iccoord_tab, int ic_id, int position_id, int coordinator_id, int is_coordinator, string end_date)
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //run db and update user data --commented by Leon 6/4/2019               
            //EgrantsAccess.run_db(act, 0, active_id, 0, login_id, last_name, first_name, middle_name, email_address, phone_num, coordinator_id, position_id, ic_id, egrants_tab, mgt_tab, admin_tab, docman_tab, cft_tab, dashboard_tab, iccoord_tab, is_coordinator, end_date, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //get return notice
            string return_notice = EgrantsAccess.to_preview(act, 0, active_id, 0, login_id, last_name, first_name, middle_name, email_address, phone_num, coordinator_id, position_id, ic_id, egrants_tab, mgt_tab, admin_tab, docman_tab, cft_tab, dashboard_tab, iccoord_tab, is_coordinator, end_date, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //new user has been created and show index page
            if (return_notice == "done")
            {
                //get the first letter from last name
                var first_letter = last_name.Substring(0, 1);
                ViewBag.FirstLetter = first_letter;
                int index_id = EgrantsAccess.getCharacterIndex(first_letter);

                //return default index
                return Index(index_id, 1);
            }
            else  
            {
                //user data duplicate and return error message
                ViewBag.ReturnNotice = return_notice;
           
                //load profiles
                ViewBag.Profiles = EgrantsCommon.LoadProfiles();

                //load positions
                ViewBag.Positions = EgrantsCommon.LoadPositions();

                //Load Coordinators
                ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();

                //Load user data
                ViewBag.User = null;

                return View("~/Egrants_Admin/Views/EgrantsAccessCreate.cshtml");
            }
        }

        public ActionResult To_Change_Status(string act, int user_id)
        {
            //run db and update user status
            EgrantsAccess.run_db(act, 0, 0, user_id, "", "", "", "", "", "", 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return To_Search(user_id);
        }

        public ActionResult To_LoadAccept(int accept_user_id)
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load profiles
            ViewBag.Profiles = EgrantsCommon.LoadProfiles();

            //load positions
            ViewBag.Positions = EgrantsCommon.LoadPositions();

            //Load Coordinators
            ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();

            //run db and load request  user data                
            ViewBag.RequestUser = EgrantsAccess.LoadAccept(accept_user_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/EgrantsAccessRequest.cshtml");
        }
    }
}