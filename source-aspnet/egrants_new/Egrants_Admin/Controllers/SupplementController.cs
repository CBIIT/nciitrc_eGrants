#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  SupplementController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-05-05
// Contributors:
//      - Briggs, Robin (NIH/NCI) [C] - briggsr2
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

using System;
using System.Web.Mvc;

using egrants_new.Egrants_Admin.Models;
using egrants_new.Models;

#endregion

namespace egrants_new.Controllers
{
    /// <summary>
    /// The supplement controller.
    /// </summary>
    public class SupplementController : Controller
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            // set search value
            var act = "show_notification";

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // loadNotifications 
            this.ViewBag.Notifications = Supplement.LoadNotifications(
                act,
                string.Empty,
                string.Empty,
                0,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Admin/Views/SupplementIndex.cshtml");
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
            // set search value
            var act = "search_notification";

            // save serial_num
            this.ViewBag.SerialNum = serial_num;

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // loadNotifications 
            this.ViewBag.Notifications = Supplement.LoadNotifications(
                act,
                string.Empty,
                string.Empty,
                serial_num,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Admin/Views/SupplementIndex.cshtml");
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
            // set search value
            var act = "review_notification";
            this.ViewBag.ID = Convert.ToString(id);

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // review Notification status
            this.ViewBag.NotificationStatus = Supplement.ReviewNotifications(
                act,
                string.Empty,
                string.Empty,
                id,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            // review email status
            this.ViewBag.EmailStatus = Supplement.ReviewEmailStatus(id);

            // LoadEmailPositionList
            this.ViewBag.EmailPositionList = Supplement.LoadEmailPositionList();

            return this.View("~/Egrants_Admin/Views/SupplementStatus.cshtml");
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
            // set search value
            var act = "delete_notification";

            // delete Notification 
            this.ViewBag.ReturnNotice = Supplement.GetNotice(
                act,
                string.Empty,
                string.Empty,
                id,
                string.Empty,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

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
            // set search value
            var act = "edit_notification";

            // edit Notification 
            this.ViewBag.ReturnNotice = Supplement.GetNotice(
                act,
                pa,
                string.Empty,
                id,
                fgn,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

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
        public ActionResult Resent_Notification(int id, string detail)
        {
            // set search value
            var act = "resent_email_notification";

            // edit Notification 
            this.ViewBag.ReturnNotice = Supplement.GetNotice(
                act,
                string.Empty,
                detail,
                id,
                string.Empty,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

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
            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // get act
            this.ViewBag.Act = "load";

            // load Email Template data 
            this.ViewBag.EmailTemplate = Supplement.LoadEmailTemplates();

            return this.View("~/Egrants_Admin/Views/SupplementEmailTemplate.cshtml");
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
            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // get act
            this.ViewBag.ID = Convert.ToString(id);
            this.ViewBag.Act = "review";

            // load Email Template data 
            this.ViewBag.EmailTemplate = Supplement.LoadEmailTemplates();

            return this.View("~/Egrants_Admin/Views/SupplementEmailTemplate.cshtml");
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
        public ActionResult Create_Email_Template(string name, string subject, string detail)
        {
            // set act
            var act = "create_email_template";
            this.ViewBag.Act = "create";

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load Email Template data 
            this.ViewBag.EmailTemplate = Supplement.LoadEmailTemplates();

            // edit Notification 
            this.ViewBag.ReturnNotice = Supplement.GetNotice(
                act,
                string.Empty,
                detail,
                0,
                name,
                subject,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Admin/Views/SupplementEmailTemplate.cshtml");
        }

        /// <summary>
        /// The load_ workflow.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Load_Workflow()
        {
            // load pa
            this.ViewBag.PA = string.Empty;
            this.ViewBag.ACT = "load_workflow";
            this.ViewBag.Userid = Convert.ToString(this.Session["userid"]);

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load Email Template data 
            this.ViewBag.EmailTemplate = Supplement.LoadEmailTemplates();

            // load email rules list
            this.ViewBag.EmailRules = Supplement.LoadEmailRulesList();

            return this.View("~/Egrants_Admin/Views/SupplementWorkflow.cshtml");
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
        public ActionResult Show_Email_Rule(string act, string pa)
        {
            // load pa
            this.ViewBag.PA = pa;
            this.ViewBag.ACT = act;
            this.ViewBag.Userid = Convert.ToString(this.Session["userid"]);

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load Email Template data 
            this.ViewBag.EmailTemplate = Supplement.LoadEmailTemplates();

            // load email rules list
            this.ViewBag.EmailRules = Supplement.LoadEmailRulesList();

            // load email rule with pa
            this.ViewBag.EmailRule = Supplement.LoadEmailRule(
                act,
                pa,
                string.Empty,
                0,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Admin/Views/SupplementWorkflow.cshtml");
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
        public ActionResult Access_Email_Rule(string act, string pa, string detail, int id, string subject, string name)
        {
            // act could be delete, save create
            this.ViewBag.ReturnNotice = Supplement.GetNotice(
                act,
                pa,
                detail,
                id,
                name,
                subject,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.Load_Workflow();
        }
    }
}