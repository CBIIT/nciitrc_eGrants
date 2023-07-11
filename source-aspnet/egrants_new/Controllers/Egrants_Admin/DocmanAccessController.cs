#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  DocmanAccessController.cs
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

namespace egrants_new.Controllers.Egrants_Admin
{
    /// <summary>
    /// The docman access controller.
    /// </summary>
    public class DocmanAccessController : Controller
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="index_id">
        /// The index_id.
        /// </param>
        /// <param name="active_id">
        /// The active_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index(int index_id, int active_id)
        {
            // save info
            this.ViewBag.IndexID = index_id;
            this.ViewBag.ActiveID = active_id;

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load Character Index
            this.ViewBag.CharacterIndex = EgrantsCommon.LoadCharacterIndex();

            // load user list
            this.ViewBag.LoadUsers = DocmanAccess.LoadUsers(
                "load",
                index_id,
                active_id,
                0,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                0,
                0,
                1,
                0,
                0,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Views/Egrants_Admin/DocmanAccessIndex.cshtml");
        }

        /// <summary>
        /// The to_ add.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Add()
        {
            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load profiles
            this.ViewBag.Profiles = EgrantsCommon.LoadProfiles();

            // Load Coordinators
            this.ViewBag.Coordinators = DocmanCommon.LoadCoordinators();

            return this.View("~/Views/Egrants_Admin/DocmanAccessCreate.cshtml");
        }

        /// <summary>
        /// The to_ create.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="active_id">
        /// The active_id.
        /// </param>
        /// <param name="login_id">
        /// The login_id.
        /// </param>
        /// <param name="first_name">
        /// The first_name.
        /// </param>
        /// <param name="last_name">
        /// The last_name.
        /// </param>
        /// <param name="middle_name">
        /// The middle_name.
        /// </param>
        /// <param name="email_address">
        /// The email_address.
        /// </param>
        /// <param name="phone_num">
        /// The phone_num.
        /// </param>
        /// <param name="docman_tab">
        /// The docman_tab.
        /// </param>
        /// <param name="cft_tab">
        /// The cft_tab.
        /// </param>
        /// <param name="position_id">
        /// The position_id.
        /// </param>
        /// <param name="coordinator_id">
        /// The coordinator_id.
        /// </param>
        /// <param name="is_coordinator">
        /// The is_coordinator.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Create(
            string act,
            int active_id,
            string login_id,
            string first_name,
            string last_name,
            string middle_name,
            string email_address,
            string phone_num,
            int docman_tab,
            int cft_tab,
            int position_id,
            int coordinator_id,
            int is_coordinator,
            string end_date)
        {
            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // run db and create user data                  
            DocmanAccess.run_db(
                act,
                0,
                active_id,
                0,
                login_id,
                last_name,
                first_name,
                middle_name,
                email_address,
                phone_num,
                coordinator_id,
                position_id,
                docman_tab,
                cft_tab,
                is_coordinator,
                end_date,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            // return default index
            return this.Index(2, 1);
        }

        /// <summary>
        /// The to_ search.
        /// </summary>
        /// <param name="person_id">
        /// The person_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Search(int person_id)
        {
            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load profiles
            this.ViewBag.Profiles = EgrantsCommon.LoadProfiles();

            // Load Coordinators
            this.ViewBag.Coordinators = DocmanCommon.LoadCoordinators();

            // load user's  data
            this.ViewBag.User = DocmanAccess.LoadUsers(
                "search",
                0,
                0,
                person_id,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                0,
                0,
                1,
                0,
                0,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Views/Egrants_Admin/DocmanAccessUpdate.cshtml");
        }

        /// <summary>
        /// The to_ update.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="user_id">
        /// The user_id.
        /// </param>
        /// <param name="login_id">
        /// The login_id.
        /// </param>
        /// <param name="first_name">
        /// The first_name.
        /// </param>
        /// <param name="last_name">
        /// The last_name.
        /// </param>
        /// <param name="middle_name">
        /// The middle_name.
        /// </param>
        /// <param name="email_address">
        /// The email_address.
        /// </param>
        /// <param name="phone_num">
        /// The phone_num.
        /// </param>
        /// <param name="docman_tab">
        /// The docman_tab.
        /// </param>
        /// <param name="cft_tab">
        /// The cft_tab.
        /// </param>
        /// <param name="position_id">
        /// The position_id.
        /// </param>
        /// <param name="coordinator_id">
        /// The coordinator_id.
        /// </param>
        /// <param name="is_coordinator">
        /// The is_coordinator.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Update(
            string act,
            int user_id,
            string login_id,
            string first_name,
            string last_name,
            string middle_name,
            string email_address,
            string phone_num,
            int docman_tab,
            int cft_tab,
            int position_id,
            int coordinator_id,
            int is_coordinator,
            string end_date)
        {
            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // run db and update user data                  
            DocmanAccess.run_db(
                act,
                0,
                0,
                user_id,
                login_id,
                last_name,
                first_name,
                middle_name,
                email_address,
                phone_num,
                coordinator_id,
                position_id,
                docman_tab,
                cft_tab,
                is_coordinator,
                end_date,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            // return default index
            return this.Index(2, 1);
        }

        /// <summary>
        /// The to_ change_ status.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="user_id">
        /// The user_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Change_Status(string act, int user_id)
        {
            // run db and update user status
            DocmanAccess.run_db(
                act,
                0,
                0,
                user_id,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                0,
                0,
                1,
                0,
                0,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.To_Search(user_id);
        }
    }
}