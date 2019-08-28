using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;
using egrants_new.IC_Coordinator.Models;

namespace egrants_new.Controllers
{
    public class ICCoordinatorController : Controller
    {
        // GET: Coordiator
        public ActionResult Index()
        {
            string act = "review";
            //load list for all IC Coordinators
            ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();
            ViewBag.CoordinatorID = Convert.ToInt16(Session["Personid"]);

            //load requested user list created by Coordinator
            ViewBag.Users = Coordinator.LoadRequestedUsers(act, Convert.ToInt16(Session["Personid"]), 0, "", "", "", "", "", "", "", "", "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/IC_Coordinator/Views/ICCoordinatorIndex.cshtml");
        }

        public ActionResult To_Review(int id)
        {
            string act = "review";
            //load list for all IC Coordinators
            ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();
            ViewBag.CoordinatorID = Convert.ToInt16(Session["Personid"]);

            //load requested user list created by Coordinator
            ViewBag.Users = Coordinator.LoadRequestedUsers(act, id, 0, "", "", "", "", "", "", "", "", "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/IC_Coordinator/Views/ICCoordinatorIndex.cshtml");
        }

        public ActionResult To_Add()
        {
            return View("~/IC_Coordinator/Views/ICCoordinatorCreate.cshtml");
        }

        public ActionResult To_Request(int id)
        {
            string act = "request";
            ViewBag.User = Coordinator.LoadRequestedUsers(act, 0, id, "", "", "", "", "", "", "", "", "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/IC_Coordinator/Views/ICCoordinatorReview.cshtml");
        }

        public ActionResult To_Edit(int id)
        {
            string act = "edit";
            //load list for all IC Coordinators
            ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();
            ViewBag.CoordinatorID = Convert.ToInt16(Session["Personid"]);

            ViewBag.User = Coordinator.LoadRequestedUsers(act, 0, id, "", "", "", "", "", "", "", "", "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/IC_Coordinator/Views/ICCoordinatorEdit.cshtml");
        }

        public ActionResult To_Update(string act, int request_user_id, string first_name, string middle_name, string last_name, string login_id, string email_address, string phone_number, string division, string access_type, string start_date, string end_date, string comments)
        {
            //load list for all IC Coordinators
            ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();
            ViewBag.CoordinatorID = Convert.ToInt16(Session["Personid"]);

            //update database
            Coordinator.AccessUsers(act, 0, request_user_id, first_name, middle_name, last_name, login_id, email_address, phone_number, division, access_type, start_date, end_date, comments, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load  requested user list
            //ViewBag.Users = Coordinator.LoadRequestedUsers("default", 0, 0, "", "", "", "", "", "", "", "", "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load requested user list created by Coordinator
            //act = "review";
            //ViewBag.Users = Coordinator.LoadRequestedUsers(act, Convert.ToInt16(Session["Personid"]), 0, "", "", "", "", "", "", "", "", "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/IC_Coordinator/Views/ICCoordinatorIndex.cshtml");
        }

        public ActionResult To_Create(string first_name, string middle_name, string last_name, string login_id, string email_address, string phone_number, string division, string access_type, string start_date, string end_date, string comments)
        {
            string act = "create";

            //load list for all IC Coordinators
            ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();
            ViewBag.CoordinatorID = Convert.ToInt16(Session["Personid"]);

            //access db to add new request
            Coordinator.AccessUsers(act, 0, 0, first_name, middle_name, last_name, login_id, email_address, phone_number, division, access_type, start_date, end_date, comments, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load requested user list created by Coordinator
            //ViewBag.Users = Coordinator.LoadRequestedUsers(act, Convert.ToInt16(Session["Personid"]), 0, "", "", "", "", "", "", "", "", "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/IC_Coordinator/Views/ICCoordinatorIndex.cshtml");
            //get person_id for user          
            //return To_Refresh(Convert.ToString(Session["userid"]));
        }

        public ActionResult To_Delete(int id, string url)
        {
            string act = "delete";
            Coordinator.AccessUsers(act, 0, id, "", "", "", "", "", "", "", "", "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //return Index();
            return Redirect(url);
        }

        public ActionResult To_Refresh(string userid)
        {
            //get person_id
            var person_id = EgrantsCommon.GetPersonID(Convert.ToString(Session["userid"]));
            return To_Review(person_id);
        }

    }
}