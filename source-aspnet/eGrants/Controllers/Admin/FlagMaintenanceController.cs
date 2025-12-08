#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  FlagMaintenanceController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2025-12-02
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
using Microsoft.EntityFrameworkCore;

#endregion

namespace eGrants.Controllers.Admin
{
    /// <summary>
    /// The flag maintenance controller.
    /// </summary>
    public class FlagMaintenanceController : Controller
    {
        private readonly ICommonRepository _commonRepository;
        private readonly ICommonService _commonService;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly IFlagMaintenanceService _flagMaintenanceService;

        public FlagMaintenanceController(ICommonRepository commonRepository, ICommonService commonService, 
            ISessionInfoService sessionInfoService, IFlagMaintenanceService flagMaintenanceService)
        {
            _commonRepository = commonRepository;
            _commonService = commonService;
            _sessionInfoService = sessionInfoService;
            _flagMaintenanceService = flagMaintenanceService;
        }

        /// <summary>
        /// The index. GET: FlagMaintenance
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<IActionResult> Index(
            int page = 1,
            string sortColumn = "",
            string sortDirection = "asc",
            string flag_type = "",
            string admin_code = "",
            int serial_num = 0)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load flagtypes
            ViewBag.FlagTypes = _flagMaintenanceService.LoadFlagTypes();

            // load admin codes
            ViewBag.AdminCodes = await _commonService.LoadAdminCodes();

            // load flags
            var act = "show_flags";
            var flags = _flagMaintenanceService.LoadFlags(
                act,
                flag_type,
                admin_code,
                serial_num,
                string.Empty,
                sessionInfo.Ic,
                sessionInfo.UserId);

            // Sort the flags only if sort parameters are provided
            if (!string.IsNullOrEmpty(sortColumn))
            {
                flags = SortFlags(flags, sortColumn, sortDirection ?? "asc");
            }

            // Set ViewBag properties
            ViewBag.Flags = flags;
            ViewBag.CurrentPage = page;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.FlagType = flag_type;
            ViewBag.SerialNumber = serial_num > 0 ? serial_num.ToString() : string.Empty;

            return View("~/Views/Admin/FlagMaintenanceIndex.cshtml");
        }

        // Helper method for sorting
        private List<Flags> SortFlags(List<Flags> flags, string sortColumn, string sortDirection)
        {
            if (flags == null || !flags.Any())
                return flags;

            return sortColumn?.ToLower() switch
            {
                "flag_type" => sortDirection == "asc"
                    ? flags.OrderBy(f => f.flag_type).ToList()
                    : flags.OrderByDescending(f => f.flag_type).ToList(),
                "serial_num" => sortDirection == "asc"
                    ? flags.OrderBy(f => f.serial_num).ToList()
                    : flags.OrderByDescending(f => f.serial_num).ToList(),
                "grant_num" => sortDirection == "asc"
                    ? flags.OrderBy(f => f.grant_num).ToList()
                    : flags.OrderByDescending(f => f.grant_num).ToList(),
                _ => sortDirection == "asc"
                    ? flags.OrderBy(f => f.full_grant_num).ToList()
                    : flags.OrderByDescending(f => f.full_grant_num).ToList()
            };
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
        public async Task<IActionResult> To_Search(string act, string flag_type = "", string admin_code = "", int serial_num = 0)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            // load admin menu list           
            this.ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load flagtypes
            this.ViewBag.FlagTypes = _flagMaintenanceService.LoadFlagTypes();

            // load admin codes
            this.ViewBag.AdminCodes = await _commonService.LoadAdminCodes();

            // set default value
            this.ViewBag.SerialNumber = Convert.ToSingle(serial_num);

            if (flag_type == string.Empty)
                this.ViewBag.FlagType = null;
            else
                this.ViewBag.FlagType = flag_type;

            // load flags
            this.ViewBag.Flags = _flagMaintenanceService.LoadFlags(
                act,
                flag_type,
                admin_code,
                serial_num,
                string.Empty,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return this.View("~/Views/Admin/FlagMaintenanceIndex.cshtml");
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
        public async Task<IActionResult> Show_Flags(string act = "", string flag_type = "")
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            
            // load searching data
            this.ViewBag.FlagType = flag_type;

            // load admin menu list
            this.ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load flagtypes
            this.ViewBag.FlagTypes = _flagMaintenanceService.LoadFlagTypes();

            // load admin codes
            this.ViewBag.AdminCodes = await _commonService.LoadAdminCodes();

            // load flags
            this.ViewBag.Flags = _flagMaintenanceService.LoadFlags(
                act,
                flag_type,
                string.Empty,
                0,
                string.Empty,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return this.View("~/Views/Admin/FlagMaintenanceIndex.cshtml");
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
        public async Task<IActionResult> Show_Flag(string act, string flag_type)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            // load searching data
            this.ViewBag.FlagType = flag_type;

            // load admin menu list
            this.ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId); ;

            // load flagtypes
            this.ViewBag.FlagTypes = _flagMaintenanceService.LoadFlagTypes();

            // load admin codes
            this.ViewBag.AdminCodes = await _commonService.LoadAdminCodes();

            // load flags
            this.ViewBag.Flags = _flagMaintenanceService.LoadFlags(
                act,
                flag_type,
                string.Empty,
                0,
                string.Empty,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return this.View("~/Views/Admin/FlagMaintenanceIndex.cshtml");
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
        public async Task<IActionResult> To_Setup(string flag_type = "")
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            // load act
            this.ViewBag.Act = string.Empty;

            if (flag_type == string.Empty)
                this.ViewBag.FlagType = null;
            else
                this.ViewBag.FlagType = flag_type;

            // load admin menu list
            this.ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load flagtypes
            this.ViewBag.FlagTypes = _flagMaintenanceService.LoadFlagTypes();

            // load admin codes
            this.ViewBag.AdminCodes = await _commonService.LoadAdminCodes();

            return this.View("~/Views/Admin/FlagMaintenanceSetup.cshtml");
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
        public async Task<IActionResult> Show_Appls(string act, int serial_number, string admin_code, string flag_type)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            // load searching data
            this.ViewBag.Act = act;
            this.ViewBag.FlagType = flag_type;
            this.ViewBag.SerialNumber = Convert.ToString(serial_number);           

            // load admin menu list
            this.ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load flagtypes
            this.ViewBag.FlagTypes = _flagMaintenanceService.LoadFlagTypes();

            // load admin codes
            this.ViewBag.AdminCodes = await _commonService.LoadAdminCodes();

            // load appls
            this.ViewBag.Appls = _flagMaintenanceService.LoadFlags(
                act,
                flag_type,
                admin_code,
                serial_number,
                string.Empty,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return this.View("~/Views/Admin/FlagMaintenanceSetup.cshtml");
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
        public Task<IActionResult> Remove_Flags(string act, string id_string = "", string flag_type = "")
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            // remove flags
            _flagMaintenanceService.run_db(act, flag_type, string.Empty, 0, id_string, sessionInfo.Ic, sessionInfo.UserId);

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
        public Task<IActionResult> Setup_Flag(string act = "", string flag_type = "", string admin_code = "", int serial_num = 0)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            _flagMaintenanceService.run_db(
                act,
                flag_type,
                admin_code,
                serial_num,
                string.Empty,
                sessionInfo.Ic,
                sessionInfo.UserId);

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
        public Task<IActionResult> Setup_Flags(string act = "", string flag_type = "", string id_string = "")
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            
            _flagMaintenanceService.run_db(act, flag_type, string.Empty, 0, id_string, sessionInfo.Ic, sessionInfo.UserId);

            return this.Show_Flags("show_flags", flag_type);
        }

        /// <summary>
        /// The show_ grant_ destructed.
        /// </summary>
        /// <param name="page">The current page number for pagination (default: 1).</param>
        /// <param name="sortColumn">Optional. The column name to sort by; if null, uses default order.</param>
        /// <param name="sortDirection">Sort direction: 'asc' or 'desc' (default: 'asc').</param>
        /// <returns>The GrantDestructed view with the filtered, sorted, and paged data.</returns>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public IActionResult Show_Grant_Destructed(
        int page = 1,
        string sortColumn = null,
        string sortDirection = "asc")
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            var act = "show_grant_destructed";

            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // Load, filter, sort and paginate
            var appls = _flagMaintenanceService.LoadAppls(
                act,
                string.Empty,
                string.Empty,
                0,
                string.Empty,
                sessionInfo.Ic,
                sessionInfo.UserId
            );

            if (!string.IsNullOrEmpty(sortColumn))
            {
                appls = sortDirection.ToLower() == "desc"
                    ? appls.OrderByDescending(x => EF.Property<object>(x, sortColumn)).ToList()
                    : appls.OrderBy(x => EF.Property<object>(x, sortColumn)).ToList();
            }

            int pageSize = 50;
            int totalRecords = appls.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var pagedAppls = appls.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Appls = pagedAppls;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.TotalRecords = totalRecords;

            return View("~/Views/Admin/GrantDestructed.cshtml");
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
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            var act = "search_grant_destructed";
            this.ViewBag.SearchStr = search_str;

            // load admin menu list
            this.ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // remove grant destructed
            this.ViewBag.Appls = _flagMaintenanceService.LoadAppls(
                act,
                string.Empty,
                string.Empty,
                0,
                search_str,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return this.View("~/Views/Admin/GrantDestructed.cshtml");
        }
    }
}