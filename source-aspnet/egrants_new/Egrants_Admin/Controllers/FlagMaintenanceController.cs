#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  FlagMaintenanceController.cs
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
    /// The flag maintenance controller.
    /// </summary>
    public class FlagMaintenanceController : Controller
    {
        /// <summary>
        /// The index. GET: FlagMaintenance
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load flagtypes
            this.ViewBag.Flagtypes = FlagMaintenance.LoadFlagTypes();

            // load admin codes
            this.ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            // load flags
            var act = "show_flags";

            this.ViewBag.Flags = FlagMaintenance.LoadFlags(
                act,
                string.Empty,
                string.Empty,
                0,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Admin/Views/FlagMaintenanceIndex.cshtml");
        }

        /// <summary>
        /// The to_ search.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Search(string act, string flag_type, string admin_code, int serial_num)
        {
            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load flagtypes
            this.ViewBag.FlagTypes = FlagMaintenance.LoadFlagTypes();

            // load admin codes
            this.ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            // set default value
            this.ViewBag.SerialNumber = Convert.ToSingle(serial_num);

            if (flag_type == string.Empty)
                this.ViewBag.FlagType = null;
            else
                this.ViewBag.FlagType = flag_type;

            // load flags
            this.ViewBag.Flags = FlagMaintenance.LoadFlags(
                act,
                flag_type,
                admin_code,
                serial_num,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Admin/Views/FlagMaintenanceIndex.cshtml");
        }

        /// <summary>
        /// The show_ flags.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Show_Flags(string act, string flag_type)
        {
            // load searching data
            this.ViewBag.FlagType = flag_type;

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load flagtypes
            this.ViewBag.FlagTypes = FlagMaintenance.LoadFlagTypes();

            // load admin codes
            this.ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            // load flags
            this.ViewBag.Flags = FlagMaintenance.LoadFlags(
                act,
                flag_type,
                string.Empty,
                0,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Admin/Views/FlagMaintenanceIndex.cshtml");
        }

        /// <summary>
        /// The show_ flag.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Show_Flag(string act, string flag_type)
        {
            // load searching data
            this.ViewBag.FlagType = flag_type;

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load flagtypes
            this.ViewBag.FlagTypes = FlagMaintenance.LoadFlagTypes();

            // load admin codes
            this.ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            // load flags
            this.ViewBag.Flags = FlagMaintenance.LoadFlags(
                act,
                flag_type,
                string.Empty,
                0,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Admin/Views/FlagMaintenanceIndex.cshtml");
        }

        /// <summary>
        /// The to_ setup.
        /// </summary>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Setup(string flag_type)
        {
            // load act
            this.ViewBag.Act = string.Empty;

            if (flag_type == string.Empty)
                this.ViewBag.FlagType = null;
            else
                this.ViewBag.FlagType = flag_type;

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load flagtypes
            this.ViewBag.FlagTypes = FlagMaintenance.LoadFlagTypes();

            // load admin codes
            this.ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            return this.View("~/Egrants_Admin/Views/FlagMaintenanceSetup.cshtml");
        }

        /// <summary>
        /// The show_ appls.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="serial_number">
        /// The serial_number.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Show_Appls(string act, int serial_number, string admin_code, string flag_type)
        {
            // load searching data
            this.ViewBag.Act = act;
            this.ViewBag.FlagType = flag_type;
            this.ViewBag.SerialNumber = Convert.ToString(serial_number);

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load flagtypes
            this.ViewBag.FlagTypes = FlagMaintenance.LoadFlagTypes();

            // load admin codes
            this.ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            // load appls
            this.ViewBag.Appls = FlagMaintenance.LoadFlags(
                act,
                flag_type,
                admin_code,
                serial_number,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Admin/Views/FlagMaintenanceSetup.cshtml");
        }

        /// <summary>
        /// The remove_ flags.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="id_string">
        /// The id_string.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Remove_Flags(string act, string id_string, string flag_type)
        {
            // remove flags
            FlagMaintenance.run_db(act, flag_type, string.Empty, 0, id_string, Convert.ToString(this.Session["ic"]), Convert.ToString(this.Session["userid"]));

            return this.Show_Flags("show_flags", flag_type);
        }

        /// <summary>
        /// The setup_ flag.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Setup_Flag(string act, string flag_type, string admin_code, int serial_num)
        {
            // remove flags
            FlagMaintenance.run_db(
                act,
                flag_type,
                admin_code,
                serial_num,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.Show_Flags("show_flags", flag_type);
        }

        /// <summary>
        /// The setup_ flags.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <param name="id_string">
        /// The id_string.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Setup_Flags(string act, string flag_type, string id_string)
        {
            // remove flags
            FlagMaintenance.run_db(act, flag_type, string.Empty, 0, id_string, Convert.ToString(this.Session["ic"]), Convert.ToString(this.Session["userid"]));

            return this.Show_Flags("show_flags", flag_type);
        }

        /// <summary>
        /// The show_ grant_ destructed.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Show_Grant_Destructed()
        {
            var act = "show_grant_destructed";

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // remove grant destructed
            this.ViewBag.Appls = FlagMaintenance.LoadAppls(
                act,
                string.Empty,
                string.Empty,
                0,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Admin/Views/GrantDestructed.cshtml");
        }

        /// <summary>
        /// The search_ grant_ destructed.
        /// </summary>
        /// <param name="search_str">
        /// The search_str.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Search_Grant_Destructed(string search_str)
        {
            var act = "search_grant_destructed";
            this.ViewBag.SearchStr = search_str;

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // remove grant destructed
            this.ViewBag.Appls = FlagMaintenance.LoadAppls(
                act,
                string.Empty,
                string.Empty,
                0,
                search_str,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Admin/Views/GrantDestructed.cshtml");
        }
    }
}