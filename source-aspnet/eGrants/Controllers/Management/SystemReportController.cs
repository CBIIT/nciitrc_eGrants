#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  SystemReportController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2025-11-25
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

using eGrants.Models;
using eGrants.Services;
using eGrants.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

#endregion

namespace eGrants.Controllers.Management
{
    /// <summary>
    /// The system report controller.
    /// </summary>
    public class SystemReportController : Controller
    {

        private readonly IManagementService _managementService;
        private readonly ISessionInfoService _sessionInfoService = new SessionInfoService();
        private SessionInfo sessionInfo => _sessionInfoService.GetSessionInfo(HttpContext.Session);

        public SystemReportController(IManagementService managementService, ISessionInfoService sessionInfoService)
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
            // load egrants accession list
            ViewBag.Accessions = await _managementService.LoadAccessions(sessionInfo.Ic);

            ViewBag.HasSearched = false;

            return View("~/Views/Management/SystemReport.cshtml");
        }

        /// <summary>
        /// The by_ serialnum.
        /// </summary>
        /// <param name="serial_number">
        /// The serial_number.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> by_Serialnum(int serial_number, int pageIndex = 1)
        {
            var act = "by_serialnumber";
            ViewBag.SerialNumber = serial_number;

            // load egrants accession list
            ViewBag.Accessions = await _managementService.LoadAccessions(sessionInfo.Ic);
            ViewBag.SearchType = "SerialNumber"; // in by_Serialnum


            // load folders by serial number search
            var docs = await _managementService.LoadFolders(
                act,
                serial_number,
                sessionInfo.Ic,
                sessionInfo.UserId);

            // Paging setup
            int pageSize = 50; // same as WebGrid rowsPerPage
            int totalCount = docs.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedDocs = docs
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Put into ViewBag
            ViewBag.EgrantsFolders = pagedDocs;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            ViewBag.HasSearched = !string.IsNullOrEmpty(serial_number.ToString());

            return this.View("~/Views/Management/SystemReport.cshtml");
        }

        /// <summary>
        /// The by_ accessionid.
        /// </summary>
        /// <param name="accession_id">
        /// The accession_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> by_Accessionid(int accession_id, int pageIndex = 1)
        {
            var act = "by_accessionid";
            ViewBag.AccessionID = accession_id;
            ViewBag.SearchType = "AccessionID"; // in by_Accessionid

            // load egrants accession list
            ViewBag.Accessions = await _managementService.LoadAccessions(sessionInfo.Ic);

            // load folders by accession id search
            var docs = await _managementService.LoadFolders(
                act,
                accession_id,
                sessionInfo.Ic,
                sessionInfo.UserId);

            // Paging setup
            int pageSize = 20; // same as WebGrid rowsPerPage
            int totalCount = docs.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedDocs = docs
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Put into ViewBag
            ViewBag.EgrantsFolders = pagedDocs;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            ViewBag.HasSearched = !string.IsNullOrEmpty(accession_id.ToString());

            return this.View("~/Views/Management/SystemReport.cshtml");
        }
    }
}