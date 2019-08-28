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
using egrants_new.Reminder.Models;

namespace egrants_new.Controllers
{
    public class ReminderController : Controller
    {
        public ActionResult Reminder_Load()
        {
            return View("~/Dashboard/Views/Reminder.cshtml");
        }

        public ActionResult Reminder_Search(string act, int serial_num)
        {
            //load act
            ViewBag.Act = act;
            ViewBag.SerialNum = serial_num;

            //run db to get data
            ViewBag.Appls = Reminder.Models.Reminder.LoadAppls(serial_num);

            return View("~/Dashboard/Views/Reminder.cshtml");
        }

        public ActionResult Reminder_Select(string act,int serial_num, int appl_id)
        {
            //load act
            ViewBag.Act = act;
            ViewBag.SerialNum = serial_num;

            //run db to get data
            ViewBag.Appl = Reminder.Models.Reminder.LoadSelectedAppl(appl_id);

            return View("~/Dashboard/Views/Reminder.cshtml");
        }

        public ActionResult Save_Data(string act, string event_type, int appl_id, string effective_date, string reminder_text, string by_email, string by_display)
        {
            //load act
            ViewBag.Act = act;

            //run db
            Reminder.Models.Reminder.run_db(event_type, appl_id, effective_date, reminder_text, by_email, by_display, Convert.ToString(Session["userid"]));

            return View("~/Dashboard/Views/Reminder.cshtml");
        }
    }
}