#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  DocTransactionReportController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2025-11-23
// Contributors:
//      - Dehuff, Daryl (NIH/NCI) [C] - dehuffdc
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
using eGrants.Services;
using eGrants.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

#endregion

namespace eGrants.Controllers.Management
{
    /// <summary>
    /// The doc transaction report controller.
    /// </summary>
    public class DocTransactionReportController : Controller
    {
        // Injected dependencies: database context and management service

        private readonly IManagementService _managementService;
        private readonly ISessionInfoService _sessionInfoService = new SessionInfoService();
        private SessionInfo sessionInfo => _sessionInfoService.GetSessionInfo(HttpContext.Session);

        public DocTransactionReportController(IManagementService managementService, ISessionInfoService sessionInfoService)
        {
            _managementService = managementService;
            _sessionInfoService = sessionInfoService;
        }
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> Index()
        {
            // load egrants specialist list
            ViewBag.Specialists = await _managementService.LoadSpecialists(sessionInfo.Ic);

            ViewBag.HasSearched = false;

            return View("~/Views/Management/DocTransactionReport.cshtml");
        }

        // search by click button
        /// <summary>
        /// The to_ show_ report.
        /// </summary>
        /// <param name="transaction_type">
        /// The transaction_type.
        /// </param>
        /// <param name="person_id">
        /// The person_id.
        /// </param>
        /// <param name="date_range">
        /// The date_range.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> To_Show_Report(
            string transaction_type,
            int person_id,
            string date_range,
            string start_date,
            string end_date,
            int pageIndex = 1)
        {
            // load egrants specialist list
            ViewBag.Specialists = await _managementService.LoadSpecialists(sessionInfo.Ic);

            ViewBag.PersonID = person_id;
            ViewBag.TransactionType = transaction_type;
            ViewBag.DateRange = date_range;
            ViewBag.UsingDateFilter = false;

            //var start_date = string.Empty;
            //var end_date = string.Empty;

            // load docs Transaction history
            var docs = await _managementService.LoadDocTransactionHistory(
                transaction_type,
                person_id,
                start_date,
                end_date,
                date_range,
                Convert.ToString(sessionInfo.Ic),
                Convert.ToString(sessionInfo.UserId));

            // Paging setup
            int pageSize = 50; // same as WebGrid rowsPerPage
            int totalCount = docs.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedDocs = docs
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Put into ViewBag
            ViewBag.EgrantsDocs = pagedDocs;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            ViewBag.HasSearched = !string.IsNullOrEmpty(transaction_type);

            return View("~/Views/Management/DocTransactionReport.cshtml");
        }


        /// <summary>
        /// The to_ create_ report.
        /// </summary>
        /// <param name="transaction_type">
        /// The transaction_type.
        /// </param>
        /// <param name="person_id">
        /// The person_id.
        /// </param>
        /// <param name="start_date">
        /// The start_date.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> To_Create_Report(string transaction_type, int person_id, string start_date, string end_date)
        {
            // load egrants specialist list
            ViewBag.Specialists = await _managementService.LoadSpecialists(Convert.ToString(sessionInfo.Ic));

            ViewBag.PersonID = person_id;
            ViewBag.TransactionType = transaction_type;
            ViewBag.StartDate = start_date;
            ViewBag.EndDate = end_date;
            ViewBag.UsingDateFilter = true;

            // load docs Transaction history
            var docs = await _managementService.LoadDocTransactionHistory(
                transaction_type,
                person_id,
                start_date,
                end_date,
                string.Empty,
                Convert.ToString(sessionInfo.Ic),
                Convert.ToString(sessionInfo.UserId));

            ViewBag.EgrantsDocs = docs.AsEnumerable();

            ViewBag.HasSearched = !string.IsNullOrEmpty(transaction_type);

            return View("~/Views/Management/DocTransactionReport.cshtml");
        }

        public async Task<ActionResult> To_Report(
            string transaction_type,
            int person_id,
            string? date_range,
            string? start_date,
            string? end_date,
            int pageIndex = 1)
        {
            // load egrants specialist list
            ViewBag.Specialists = await _managementService.LoadSpecialists(Convert.ToString(sessionInfo.Ic));

            ViewBag.PersonID = person_id;
            ViewBag.TransactionType = transaction_type;

            // Decide which filter mode we’re in
            if (!string.IsNullOrEmpty(start_date) && !string.IsNullOrEmpty(end_date))
            {
                ViewBag.StartDate = start_date;
                ViewBag.EndDate = end_date;
                ViewBag.UsingDateFilter = true;
            }
            else if (!string.IsNullOrEmpty(date_range))
            {
                ViewBag.DateRange = date_range;
                ViewBag.UsingDateFilter = false;
            }

            // load docs Transaction history
            var docs = await _managementService.LoadDocTransactionHistory(
                transaction_type,
                person_id,
                start_date,
                end_date,
                date_range,
                Convert.ToString(sessionInfo.Ic),
                Convert.ToString(sessionInfo.UserId));

            // Paging setup
            int pageSize = 50;
            int totalCount = docs.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedDocs = docs
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.EgrantsDocs = pagedDocs;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasSearched = !string.IsNullOrEmpty(transaction_type);

            return View("~/Views/Management/DocTransactionReport.cshtml");
        }

    }
}