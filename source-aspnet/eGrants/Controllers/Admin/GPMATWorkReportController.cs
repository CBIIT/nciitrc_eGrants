#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  GPMATWorkReportController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2025-12-05
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

namespace egrants_new.Controllers
{
    /// <summary>
    /// The gpmat work report controller.
    /// </summary>
    public class GPMATWorkReportController : Controller
    {
        private readonly ICommonRepository _commonRepository;
        private readonly ICommonService _commonService;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly IGPMATWorkReportService _gPMATWorkReportService;

        public GPMATWorkReportController(ICommonRepository commonRepository, ICommonService commonService,
            ISessionInfoService sessionInfoService, IGPMATWorkReportService gPMATWorkReportService)
        {
            _commonRepository = commonRepository;
            _commonService = commonService;
            _sessionInfoService = sessionInfoService;
            _gPMATWorkReportService = gPMATWorkReportService;
        }

        /// <summary>
        /// The index. GET: Admin
        /// </summary>
        /// <param name="page">The current page number for pagination (default: 1).</param>
        /// <param name="sortColumn">Optional. The column name to sort by.</param>
        /// <param name="sortDirection">Sort direction: 'asc' or 'desc' (default: 'asc').</param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public IActionResult Index(
            int page = 1,
            string sortColumn = "",
            string sortDirection = "asc")
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load reports
            var reports = _gPMATWorkReportService.LoadReports(sessionInfo.Ic, sessionInfo.UserId);

            // Sort the reports only if sort parameters are provided
            if (!string.IsNullOrEmpty(sortColumn))
            {
                reports = SortReports(reports, sortColumn, sortDirection);
            }

            // Pagination
            int pageSize = 20;
            int totalRecords = reports.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var pagedReports = reports.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Set ViewBag properties
            ViewBag.Reports = pagedReports;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.TotalRecords = totalRecords;

            return View("~/Views/Admin/GPMATWorkReport.cshtml");
        }

        // Helper method for sorting
        private List<PMATWorkReports> SortReports(List<PMATWorkReports> reports, string sortColumn, string sortDirection)
        {
            if (reports == null || !reports.Any())
                return reports;

            return sortColumn?.ToLower() switch
            {
                "specialist_name" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.specialist_name).ToList()
                    : reports.OrderByDescending(r => r.specialist_name).ToList(),
                "specialist_code" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.specialist_code).ToList()
                    : reports.OrderByDescending(r => r.specialist_code).ToList(),
                "branch" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.branch).ToList()
                    : reports.OrderByDescending(r => r.branch).ToList(),
                "team" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.team).ToList()
                    : reports.OrderByDescending(r => r.team).ToList(),
                "fy" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.fy).ToList()
                    : reports.OrderByDescending(r => r.fy).ToList(),
                "oct_cnt" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.OCT_CNT).ToList()
                    : reports.OrderByDescending(r => r.OCT_CNT).ToList(),
                "oct_rel" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.OCT_REL).ToList()
                    : reports.OrderByDescending(r => r.OCT_REL).ToList(),
                "oct_wrkld" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.OCT_WRKLD).ToList()
                    : reports.OrderByDescending(r => r.OCT_WRKLD).ToList(),
                "nov_cnt" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.NOV_CNT).ToList()
                    : reports.OrderByDescending(r => r.NOV_CNT).ToList(),
                "nov_rel" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.NOV_REL).ToList()
                    : reports.OrderByDescending(r => r.NOV_REL).ToList(),
                "nov_wrkld" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.NOV_WRKLD).ToList()
                    : reports.OrderByDescending(r => r.NOV_WRKLD).ToList(),
                "dec_cnt" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.DEC_CNT).ToList()
                    : reports.OrderByDescending(r => r.DEC_CNT).ToList(),
                "dec_rel" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.DEC_REL).ToList()
                    : reports.OrderByDescending(r => r.DEC_REL).ToList(),
                "dec_wrkld" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.DEC_WRKLD).ToList()
                    : reports.OrderByDescending(r => r.DEC_WRKLD).ToList(),
                "jan_cnt" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.JAN_CNT).ToList()
                    : reports.OrderByDescending(r => r.JAN_CNT).ToList(),
                "jan_rel" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.JAN_REL).ToList()
                    : reports.OrderByDescending(r => r.JAN_REL).ToList(),
                "jan_wrkld" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.JAN_WRKLD).ToList()
                    : reports.OrderByDescending(r => r.JAN_WRKLD).ToList(),
                "feb_cnt" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.FEB_CNT).ToList()
                    : reports.OrderByDescending(r => r.FEB_CNT).ToList(),
                "feb_rel" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.FEB_REL).ToList()
                    : reports.OrderByDescending(r => r.FEB_REL).ToList(),
                "feb_wrkld" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.FEB_WRKLD).ToList()
                    : reports.OrderByDescending(r => r.FEB_WRKLD).ToList(),
                "mar_cnt" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.MAR_CNT).ToList()
                    : reports.OrderByDescending(r => r.MAR_CNT).ToList(),
                "mar_rel" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.MAR_REL).ToList()
                    : reports.OrderByDescending(r => r.MAR_REL).ToList(),
                "mar_wrkld" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.MAR_WRKLD).ToList()
                    : reports.OrderByDescending(r => r.MAR_WRKLD).ToList(),
                "apr_cnt" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.APR_CNT).ToList()
                    : reports.OrderByDescending(r => r.APR_CNT).ToList(),
                "apr_rel" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.APR_REL).ToList()
                    : reports.OrderByDescending(r => r.APR_REL).ToList(),
                "apr_wrkld" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.APR_WRKLD).ToList()
                    : reports.OrderByDescending(r => r.APR_WRKLD).ToList(),
                "may_cnt" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.MAY_CNT).ToList()
                    : reports.OrderByDescending(r => r.MAY_CNT).ToList(),
                "may_rel" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.MAY_REL).ToList()
                    : reports.OrderByDescending(r => r.MAY_REL).ToList(),
                "may_wrkld" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.MAY_WRKLD).ToList()
                    : reports.OrderByDescending(r => r.MAY_WRKLD).ToList(),
                "jun_cnt" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.JUN_CNT).ToList()
                    : reports.OrderByDescending(r => r.JUN_CNT).ToList(),
                "jun_rel" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.JUN_REL).ToList()
                    : reports.OrderByDescending(r => r.JUN_REL).ToList(),
                "jun_wrkld" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.JUN_WRKLD).ToList()
                    : reports.OrderByDescending(r => r.JUN_WRKLD).ToList(),
                "jul_cnt" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.JUL_CNT).ToList()
                    : reports.OrderByDescending(r => r.JUL_CNT).ToList(),
                "jul_rel" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.JUL_REL).ToList()
                    : reports.OrderByDescending(r => r.JUL_REL).ToList(),
                "jul_wrkld" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.JUL_WRKLD).ToList()
                    : reports.OrderByDescending(r => r.JUL_WRKLD).ToList(),
                "aug_cnt" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.AUG_CNT).ToList()
                    : reports.OrderByDescending(r => r.AUG_CNT).ToList(),
                "aug_rel" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.AUG_REL).ToList()
                    : reports.OrderByDescending(r => r.AUG_REL).ToList(),
                "aug_wrkld" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.AUG_WRKLD).ToList()
                    : reports.OrderByDescending(r => r.AUG_WRKLD).ToList(),
                "sep_cnt" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.SEP_CNT).ToList()
                    : reports.OrderByDescending(r => r.SEP_CNT).ToList(),
                "sep_rel" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.SEP_REL).ToList()
                    : reports.OrderByDescending(r => r.SEP_REL).ToList(),
                "sep_wrkld" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.SEP_WRKLD).ToList()
                    : reports.OrderByDescending(r => r.SEP_WRKLD).ToList(),
                "total_cnt" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.TOTAL_CNT).ToList()
                    : reports.OrderByDescending(r => r.TOTAL_CNT).ToList(),
                "total_rel" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.TOTAL_REL).ToList()
                    : reports.OrderByDescending(r => r.TOTAL_REL).ToList(),
                "total_wrkld" => sortDirection == "asc"
                    ? reports.OrderBy(r => r.TOTAL_WRKLD).ToList()
                    : reports.OrderByDescending(r => r.TOTAL_WRKLD).ToList(),
                _ => reports
            };
        }
    }
}