using System.Web;

using eGrants.Models;
using eGrants.Services;
using eGrants.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;


namespace eGrants.Controllers
{
    public class DashboardController : Controller
    {
        // Injected dependencies: database context, common and dashboard service

        private readonly IDashboardService _dashboardService;
        private readonly ICommonService _commonService;
        private readonly ISessionInfoService _sessionInfoService = new SessionInfoService();
        private SessionInfo sessionInfo => _sessionInfoService.GetSessionInfo(HttpContext.Session);

        public DashboardController(IDashboardService dashboardService, ICommonService commonService, ISessionInfoService sessionInfoService)
        {
            _dashboardService = dashboardService;
            _commonService = commonService;
            _sessionInfoService = sessionInfoService;
        }

        public async Task<ActionResult> Index()
        {       
            sessionInfo.Dashboard = 1;
            var act = "get_assignment";
            var idstr = "";
            ViewBag.ICList = await _commonService.LoadAdminCodes();

            //get GetTotalWidget
            ViewBag.TotalWidgets = await _dashboardService.GetTotalWidgets();

            //load default org
            ViewBag.Widgets = await _dashboardService.LoadWidgets(act, idstr, sessionInfo.Ic, sessionInfo.UserId);

            //load user selected Widgets
            ViewBag.SelectedWidgets = await _dashboardService.LoadSeletedWidgets(sessionInfo.UserId);

            //load link list
            ViewBag.LinkLists = await _dashboardService.LoadLinkList();

            //load grants togo cc
            ViewBag.GrantsTogoCC = await _dashboardService.LoadGrantsTogoCC(sessionInfo.UserId, "cc");

            //load grants togo nc
            ViewBag.GrantsTogoNC = await _dashboardService.LoadGrantsTogoNC(sessionInfo.UserId, "nc");

            //load grants delayed
            ViewBag.GrantsExpedited = await _dashboardService.LoadGrantsExpedited(sessionInfo.UserId);

            //load late grants 
            ViewBag.GrantsDelayed = await _dashboardService.LoadGrantsDelayed(sessionInfo.UserId);

            //load new grants 
            ViewBag.GrantsNew = await _dashboardService.LoadGrantsNew(sessionInfo.UserId, "");

            //load Avgtime
            ViewBag.Avgtime = await _dashboardService.LoadAvgtime(sessionInfo.UserId);

            //load Grants Status 
            ViewBag.GrantsStatus = await _dashboardService.LoadGrantsStatus();

            //load audit report
            ViewBag.AuditReport = await _dashboardService.LoadAuditReport();

            return View("~/Views/Dashboard/Index.cshtml");
        }

        public async Task<ActionResult> Save_Selection(string act, string idstr)
        {
            //save selection
            await _dashboardService.save_selected(act, idstr, Convert.ToString(sessionInfo.Ic), Convert.ToString(sessionInfo.UserId));

            return RedirectToAction("Index");
        }

        public ActionResult Reminder_Load()
        {
            return View("~/Views/Dashboard/Reminder.cshtml");
        }
    }
}