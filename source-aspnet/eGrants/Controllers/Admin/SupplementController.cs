#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  SupplementController.cs
// Solution: egrants_new
// Project:  egrants
// Created: 2025-12-08
// Contributors:
//      - Feroz, Aalyaan (NIH/NCI) [C] - feroza2
//      -
// Copyright (c) National Institute of Health
// 
// <Description of the file>
// 
// This source is subject to the NIH Softwre License.
// See https://ncihub.org/resources/899/download/Guidelines_for_Releasing_Research_Software_04062015.pdf
// All other rights reserved.
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT ARE DISCLAIMED. IN NO EVENT SHALL THE NATIONAL
// CANCER INSTITUTE (THE PROVIDER), THE NATIONAL INSTITUTES OF HEALTH, THE
// U.S. GOVERNMENT OR THE INDIVIDUAL DEVELOPERS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
// \***************************************************************************/

#endregion

#region

using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

#endregion

namespace eGrants.Controllers.Admin
{
    /// <summary>
    /// The supplement controller.
    /// </summary>
    public class SupplementController : Controller
    {
        private readonly ICommonRepository _commonRepository;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly ISupplementService _supplementService;

        public SupplementController(
            ICommonRepository commonRepository,
            ISessionInfoService sessionInfoService,
            ISupplementService supplementService)
        {
            _commonRepository = commonRepository;
            _sessionInfoService = sessionInfoService;
            _supplementService = supplementService;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index(
            int page = 1,
            string sortColumn = "",
            string sortDirection = "asc")
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // set search value
            var act = "show_notification";

            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // loadNotifications 
            var notifications = _supplementService.LoadNotifications(
                act,
                string.Empty,
                string.Empty,
                0,
                sessionInfo.Ic,
                sessionInfo.UserId);

            // Sort the notifications only if sort parameters are provided
            if (!string.IsNullOrEmpty(sortColumn))
            {
                notifications = SortNotifications(notifications, sortColumn, sortDirection ?? "asc");
            }

            // Set ViewBag properties
            ViewBag.Notifications = notifications;
            ViewBag.CurrentPage = page;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;

            return View("~/Views/Admin/SupplementIndex.cshtml");
        }

        // Helper method for sorting notifications
        private List<Notifications> SortNotifications(List<Notifications> notifications, string sortColumn, string sortDirection)
        {
            if (notifications == null || !notifications.Any())
                return notifications;

            return sortColumn?.ToLower() switch
            {
                "full_grant_num" => sortDirection == "asc"
                    ? notifications.OrderBy(n => n.full_grant_num).ToList()
                    : notifications.OrderByDescending(n => n.full_grant_num).ToList(),
                "pa" => sortDirection == "asc"
                    ? notifications.OrderBy(n => n.pa).ToList()
                    : notifications.OrderByDescending(n => n.pa).ToList(),
                "notrcvd_dt" => sortDirection == "asc"
                    ? notifications.OrderBy(n => n.NotRcvd_dt).ToList()
                    : notifications.OrderByDescending(n => n.NotRcvd_dt).ToList(),
                "created_date" => sortDirection == "asc"
                    ? notifications.OrderBy(n => n.created_date).ToList()
                    : notifications.OrderByDescending(n => n.created_date).ToList(),
                _ => sortDirection == "asc"
                    ? notifications.OrderBy(n => n.full_grant_num).ToList()
                    : notifications.OrderByDescending(n => n.full_grant_num).ToList()
            };
        }

        /// <summary>
        /// The search_ notification.
        /// </summary>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Search_Notification(int serial_num)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // set search value
            var act = "search_notification";

            // save serial_num
            ViewBag.SerialNum = serial_num;

            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // loadNotifications 
            ViewBag.Notifications = _supplementService.LoadNotifications(
                act,
                string.Empty,
                string.Empty,
                serial_num,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return View("~/Views/Admin/SupplementIndex.cshtml");
        }

        /// <summary>
        /// The review_ notification.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Review_Notification(int id)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // set search value
            var act = "review_notification";
            ViewBag.ID = Convert.ToString(id);

            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // review Notification status
            ViewBag.NotificationStatus = _supplementService.ReviewNotifications(
                act,
                string.Empty,
                string.Empty,
                id,
                sessionInfo.Ic,
                sessionInfo.UserId);

            // review email status
            ViewBag.EmailStatus = _supplementService.ReviewEmailStatus(id);

            // LoadEmailPositionList
            ViewBag.EmailPositionList = _supplementService.LoadEmailPositionList();

            return View("~/Views/Admin/SupplementStatus.cshtml");
        }

        /// <summary>
        /// The delete_ notification.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Delete_Notification(int id)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // set search value
            var act = "delete_notification";

            // delete Notification 
            ViewBag.ReturnNotice = _supplementService.GetNotice(
                act,
                string.Empty,
                string.Empty,
                id,
                string.Empty,
                string.Empty,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return this.Index();
        }

        /// <summary>
        /// The save_ notification.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="fgn">
        /// The fgn.
        /// </param>
        /// <param name="pa">
        /// The pa.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Save_Notification(int id, string fgn, string pa)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // set search value
            var act = "edit_notification";

            // edit Notification 
            ViewBag.ReturnNotice = _supplementService.GetNotice(
                act,
                pa,
                string.Empty,
                id,
                fgn,
                string.Empty,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return this.Index();
        }

        /// <summary>
        /// The resent_ notification.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="detail">
        /// The detail.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Resent_Notification(int id, string detail = "")
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // set search value
            var act = "resent_email_notification";

            // edit Notification 
            ViewBag.ReturnNotice = _supplementService.GetNotice(
                act,
                string.Empty,
                detail,
                id,
                string.Empty,
                string.Empty,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return this.Index();
        }

        /// <summary>
        /// The load_ email_ template.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Load_Email_Template()
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // get act
            ViewBag.Act = "load";

            // load Email Template data 
            ViewBag.EmailTemplate = _supplementService.LoadEmailTemplates();

            return View("~/Views/Admin/SupplementEmailTemplate.cshtml");
        }

        /// <summary>
        /// The view_ email_ template.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult View_Email_Template(int id)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // get act
            ViewBag.ID = Convert.ToString(id);
            ViewBag.Act = "review";

            // load Email Template data 
            ViewBag.EmailTemplate = _supplementService.LoadEmailTemplates();

            return View("~/Views/Admin/SupplementEmailTemplate.cshtml");
        }

        /// <summary>
        /// The create_ email_ template.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="subject">
        /// The subject.
        /// </param>
        /// <param name="detail">
        /// The detail.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create_Email_Template(string name = "", string subject = "", string detail = "")
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // set act
            var act = "create_email_template";
            ViewBag.Act = "create";

            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load Email Template data 
            ViewBag.EmailTemplate = _supplementService.LoadEmailTemplates();

            // edit Notification 
            ViewBag.ReturnNotice = _supplementService.GetNotice(
                act,
                string.Empty,
                detail,
                0,
                name,
                subject,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return View("~/Views/Admin/SupplementEmailTemplate.cshtml");
        }

        /// <summary>
        /// The load_ workflow.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Load_Workflow()
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // load pa
            ViewBag.PA = string.Empty;
            ViewBag.ACT = "load_workflow";
            ViewBag.Userid = sessionInfo.UserId;

            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load Email Template data 
            ViewBag.EmailTemplate = _supplementService.LoadEmailTemplates();

            // load email rules list
            ViewBag.EmailRules = _supplementService.LoadEmailRulesList();

            return View("~/Views/Admin/SupplementWorkflow.cshtml");
        }

        /// <summary>
        /// The show_ email_ rule.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="pa">
        /// The pa.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Show_Email_Rule(string act = "", string pa = "")
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // load pa
            ViewBag.PA = pa;
            ViewBag.ACT = act;
            ViewBag.Userid = sessionInfo.UserId;

            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load Email Template data 
            ViewBag.EmailTemplate = _supplementService.LoadEmailTemplates();

            // load email rules list
            ViewBag.EmailRules = _supplementService.LoadEmailRulesList();

            // load email rule with pa
            ViewBag.EmailRule = _supplementService.LoadEmailRule(
                act,
                pa,
                string.Empty,
                0,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return View("~/Views/Admin/SupplementWorkflow.cshtml");
        }

        /// <summary>
        /// The access_ email_ rule.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="pa">
        /// The pa.
        /// </param>
        /// <param name="detail">
        /// The detail.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="subject">
        /// The subject.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Access_Email_Rule(string act = "", string pa = "", string detail = "", int id = 0, string subject = "", string name = "")
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // act could be delete, save create
            ViewBag.ReturnNotice = _supplementService.GetNotice(
                act,
                pa,
                detail,
                id,
                name,
                subject,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return this.Load_Workflow();
        }
    }
}