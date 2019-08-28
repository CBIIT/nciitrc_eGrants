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
    public class SupplementController : Controller
    {
        public ActionResult Index()
        {
            //set search value
            string act = "show_notification";

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //loadNotifications 
            ViewBag.Notifications = Supplement.LoadNotifications(act, "" ,"",0, Convert.ToString(Session["ic"]),Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/SupplementIndex.cshtml");
        }

        public ActionResult Search_Notification(int serial_num)
        {
            //set search value
            string act = "search_notification";
            
            //save serial_num
            ViewBag.SerialNum = serial_num;

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //loadNotifications 
            ViewBag.Notifications = Supplement.LoadNotifications(act, "", "", serial_num, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/SupplementIndex.cshtml");
        }

        public ActionResult Review_Notification(int id)
        {
            //set search value
            string act = "review_notification";
            ViewBag.ID = Convert.ToString(id);

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //review Notification status
            ViewBag.NotificationStatus = Supplement.ReviewNotifications(act, "", "", id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //review email status
            ViewBag.EmailStatus = Supplement.ReviewEmailStatus(id);

            //LoadEmailPositionList
            ViewBag.EmailPositionList = Supplement.LoadEmailPositionList();

            return View("~/Egrants_Admin/Views/SupplementStatus.cshtml");
        }

        public ActionResult Delete_Notification(int id)
        {
            //set search value
            string act = "delete_notification";

            //delete Notification 
            ViewBag.ReturnNotice = Supplement.GetNotice(act, "", "", id, "","",Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return Index();
        }

        public ActionResult Save_Notification(int id, string fgn, string pa)
        {
            //set search value
            string act = "edit_notification";

            //edit Notification 
            ViewBag.ReturnNotice = Supplement.GetNotice(act, pa, "", id, fgn, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return Index();
        }

        public ActionResult Resent_Notification(int id, string detail)
        {
            //set search value
            string act = "resent_email_notification";

            //edit Notification 
            ViewBag.ReturnNotice = Supplement.GetNotice(act, "", detail, id, "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return Index();
        }

        public ActionResult Load_Email_Template()
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //get act
            ViewBag.Act = "load";

            //load Email Template data 
            ViewBag.EmailTemplate = Supplement.LoadEmailTemplates();

            return View("~/Egrants_Admin/Views/SupplementEmailTemplate.cshtml");
        }

        public ActionResult View_Email_Template(int id)
        {
            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //get act
            ViewBag.ID = Convert.ToString(id);
            ViewBag.Act = "review";

            //load Email Template data 
            ViewBag.EmailTemplate = Supplement.LoadEmailTemplates();

            return View("~/Egrants_Admin/Views/SupplementEmailTemplate.cshtml");

        }

        public ActionResult Create_Email_Template(string name, string subject, string detail)
        {
            //set act
            string act = "create_email_template";
            ViewBag.Act = "create";

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load Email Template data 
            ViewBag.EmailTemplate = Supplement.LoadEmailTemplates();

            //edit Notification 
            ViewBag.ReturnNotice = Supplement.GetNotice(act, "", detail, 0, name, subject, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/SupplementEmailTemplate.cshtml");
        }

        public ActionResult Load_Workflow()
        {
            //load pa
            ViewBag.PA = "";
            ViewBag.ACT = "load_workflow";
            ViewBag.Userid = Convert.ToString(Session["userid"]);

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load Email Template data 
            ViewBag.EmailTemplate = Supplement.LoadEmailTemplates();

            //load email rules list
            ViewBag.EmailRules = Supplement.LoadEmailRulesList();

            return View("~/Egrants_Admin/Views/SupplementWorkflow.cshtml");
        }

        public ActionResult Show_Email_Rule(string act, string pa)
        {
            //load pa
            ViewBag.PA = pa;
            ViewBag.ACT = act;
            ViewBag.Userid = Convert.ToString(Session["userid"]);

            //load admin menu list
            ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(Session["userid"]));

            //load Email Template data 
            ViewBag.EmailTemplate = Supplement.LoadEmailTemplates();

            //load email rules list
            ViewBag.EmailRules = Supplement.LoadEmailRulesList();

            //load email rule with pa
            ViewBag.EmailRule = Supplement.LoadEmailRule(act, pa, "", 0, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Admin/Views/SupplementWorkflow.cshtml");
        }
     
        public ActionResult Access_Email_Rule(string act, string pa, string detail, int id, string subject, string name)
        {
            //act could be delete, save create
            ViewBag.ReturnNotice = Supplement.GetNotice(act, pa, detail, id, name, subject, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return Load_Workflow();
        }
    }
}

