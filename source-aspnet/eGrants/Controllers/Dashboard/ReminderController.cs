
using System.Web;
using eGrants.Models;
using eGrants.Services;
using eGrants.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace eGrants.Controllers
{
    public class ReminderController : Controller
    {
        // Injected dependencies: database context  and reminder service
        private readonly IReminderService _reminderService;
        private readonly ISessionInfoService _sessionInfoService = new SessionInfoService();

        private SessionInfo sessionInfo => _sessionInfoService.GetSessionInfo(HttpContext.Session);

        public ReminderController(IReminderService reminderService, ISessionInfoService sessionInfoService)
        {
            _reminderService = reminderService;
            _sessionInfoService = sessionInfoService;
        }

        public ActionResult Reminder_Load()
        {
            return View("~/Views/Dashboard/Reminder.cshtml");
        }

        public async Task<ActionResult> Reminder_Search(string act, int serial_num)
        {
            //load act
            ViewBag.Act = act;
            ViewBag.SerialNum = serial_num;

            //run db to get data
            ViewBag.Appls = await _reminderService.LoadAppls(serial_num);

            return View("~/Views/Dashboard/Reminder.cshtml");
        }

        public async Task<ActionResult> Reminder_Select(string act, int serial_num, int appl_id)
        {
            //load act
            ViewBag.Act = act;
            ViewBag.SerialNum = serial_num;

            //run db to get data
            ViewBag.Appl = await _reminderService.LoadSelectedAppl(appl_id);

            return View("~/Views/Dashboard/Reminder.cshtml");
        }

        public async Task<ActionResult> Save_Data(string act, string event_type, int appl_id, string effective_date, string reminder_text, string by_email, string by_display)
        {
            //load act
            ViewBag.Act = act;

            //run db
            await _reminderService.run_db(event_type, appl_id, effective_date, reminder_text, by_email, by_display, Convert.ToString(sessionInfo.UserId));

            return View("~/Views/Dashboard/Reminder.cshtml");
        }
    }
}