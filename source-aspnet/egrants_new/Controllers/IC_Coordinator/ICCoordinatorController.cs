#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  ICCoordinatorController.cs
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

using egrants_new.IC_Coordinator.Models;
using egrants_new.Models;

#endregion

namespace egrants_new.Controllers
{
    /// <summary>
    /// The ic coordinator controller.
    /// </summary>
    public class ICCoordinatorController : Controller
    {
        // GET: Coordiator
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var act = "review";

            // load list for all IC Coordinators
            this.ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();
            this.ViewBag.CoordinatorID = Convert.ToInt16(this.Session["Personid"]);

            // load requested user list created by Coordinator
            this.ViewBag.Users = Coordinator.LoadRequestedUsers(
                act,
                Convert.ToInt16(this.Session["Personid"]),
                0,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Views/IC_Coordinator/ICCoordinatorIndex.cshtml");
        }

        /// <summary>
        /// The to_ review.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Review(int id)
        {
            var act = "review";

            // load list for all IC Coordinators
            this.ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();
            this.ViewBag.CoordinatorID = Convert.ToInt16(this.Session["Personid"]);

            // load requested user list created by Coordinator
            this.ViewBag.Users = Coordinator.LoadRequestedUsers(
                act,
                id,
                0,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Views/IC_Coordinator/ICCoordinatorIndex.cshtml");
        }

        /// <summary>
        /// The to_ add.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Add()
        {
            return this.View("~/Views/IC_Coordinator/ICCoordinatorCreate.cshtml");
        }

        /// <summary>
        /// The to_ request.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Request(int id)
        {
            var act = "request";

            this.ViewBag.User = Coordinator.LoadRequestedUsers(
                act,
                0,
                id,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Views/IC_Coordinator/ICCoordinatorReview.cshtml");
        }

        /// <summary>
        /// The to_ edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Edit(int id)
        {
            var act = "edit";

            // load list for all IC Coordinators
            this.ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();
            this.ViewBag.CoordinatorID = Convert.ToInt16(this.Session["Personid"]);

            this.ViewBag.User = Coordinator.LoadRequestedUsers(
                act,
                0,
                id,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Views/IC_Coordinator/ICCoordinatorEdit.cshtml");
        }

        /// <summary>
        /// The to_ update.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="request_user_id">
        /// The request_user_id.
        /// </param>
        /// <param name="first_name">
        /// The first_name.
        /// </param>
        /// <param name="middle_name">
        /// The middle_name.
        /// </param>
        /// <param name="last_name">
        /// The last_name.
        /// </param>
        /// <param name="login_id">
        /// The login_id.
        /// </param>
        /// <param name="email_address">
        /// The email_address.
        /// </param>
        /// <param name="phone_number">
        /// The phone_number.
        /// </param>
        /// <param name="division">
        /// The division.
        /// </param>
        /// <param name="access_type">
        /// The access_type.
        /// </param>
        /// <param name="start_date">
        /// The start_date.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <param name="comments">
        /// The comments.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Update(
            string act,
            int request_user_id,
            string first_name,
            string middle_name,
            string last_name,
            string login_id,
            string email_address,
            string phone_number,
            string division,
            string access_type,
            string start_date,
            string end_date,
            string comments)
        {
            // load list for all IC Coordinators
            this.ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();
            this.ViewBag.CoordinatorID = Convert.ToInt16(this.Session["Personid"]);

            // update database
            Coordinator.AccessUsers(
                act,
                0,
                request_user_id,
                first_name,
                middle_name,
                last_name,
                login_id,
                email_address,
                phone_number,
                division,
                access_type,
                start_date,
                end_date,
                comments,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            // load  requested user list
            // ViewBag.Users = Coordinator.LoadRequestedUsers("default", 0, 0, "", "", "", "", "", "", "", "", "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            // load requested user list created by Coordinator
            // act = "review";
            // ViewBag.Users = Coordinator.LoadRequestedUsers(act, Convert.ToInt16(Session["Personid"]), 0, "", "", "", "", "", "", "", "", "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
            return this.View("~/Views/IC_Coordinator/ICCoordinatorIndex.cshtml");
        }

        /// <summary>
        /// The to_ create.
        /// </summary>
        /// <param name="first_name">
        /// The first_name.
        /// </param>
        /// <param name="middle_name">
        /// The middle_name.
        /// </param>
        /// <param name="last_name">
        /// The last_name.
        /// </param>
        /// <param name="login_id">
        /// The login_id.
        /// </param>
        /// <param name="email_address">
        /// The email_address.
        /// </param>
        /// <param name="phone_number">
        /// The phone_number.
        /// </param>
        /// <param name="division">
        /// The division.
        /// </param>
        /// <param name="access_type">
        /// The access_type.
        /// </param>
        /// <param name="start_date">
        /// The start_date.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <param name="comments">
        /// The comments.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Create(
            string first_name,
            string middle_name,
            string last_name,
            string login_id,
            string email_address,
            string phone_number,
            string division,
            string access_type,
            string start_date,
            string end_date,
            string comments)
        {
            var act = "create";

            // load list for all IC Coordinators
            this.ViewBag.Coordinators = EgrantsCommon.LoadCoordinators();
            this.ViewBag.CoordinatorID = Convert.ToInt16(this.Session["Personid"]);

            // access db to add new request
            Coordinator.AccessUsers(
                act,
                0,
                0,
                first_name,
                middle_name,
                last_name,
                login_id,
                email_address,
                phone_number,
                division,
                access_type,
                start_date,
                end_date,
                comments,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            // load requested user list created by Coordinator
            // ViewBag.Users = Coordinator.LoadRequestedUsers(act, Convert.ToInt16(Session["Personid"]), 0, "", "", "", "", "", "", "", "", "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
            return this.View("~/Views/IC_Coordinator/ICCoordinatorIndex.cshtml");

            // get person_id for user          
            // return To_Refresh(Convert.ToString(Session["userid"]));
        }

        /// <summary>
        /// The to_ delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Delete(int id, string url)
        {
            var act = "delete";

            Coordinator.AccessUsers(
                act,
                0,
                id,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            // return Index();
            return this.Redirect(url);
        }

        /// <summary>
        /// The to_ refresh.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Refresh(string userid)
        {
            // get person_id
            var person_id = EgrantsCommon.GetPersonID(Convert.ToString(this.Session["userid"]));

            return this.To_Review(person_id);
        }
    }
}