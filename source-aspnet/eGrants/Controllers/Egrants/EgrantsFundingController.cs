#region FileHeader
// /****************************** Module Header ******************************\
// Module Name:  EgrantsFundingController.cs
// Solution: egrants_new
// Project:  egrants
// Created: 2025-12-16
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


using eGrants.Models;
using eGrants.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eGrants.Controllers.Funding
{
    /// <summary>
    /// Controller for eGrants Funding Files operations
    /// </summary>
    public class EgrantsFundingController : Controller
    {
        private readonly IEgrantsFundingService _fundingService;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly ICommonService _commonService;
        private readonly IeGrantsService _egrantsService;

        private SessionInfo sessionInfo => _sessionInfoService.GetSessionInfo(HttpContext.Session);

        public EgrantsFundingController(
            IEgrantsFundingService fundingService,
            ISessionInfoService sessionInfoService,
            ICommonService commonService,
            IeGrantsService egrantsService)
        {
            _fundingService = fundingService ?? throw new ArgumentNullException(nameof(fundingService));
            _sessionInfoService = sessionInfoService ?? throw new ArgumentNullException(nameof(sessionInfoService));
            _commonService = commonService ?? throw new ArgumentNullException(nameof(commonService));
            _egrantsService = egrantsService ?? throw new ArgumentNullException(nameof(egrantsService));
        }

        /// <summary>
        /// Displays the funding master page with categories and documents
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int fy = 0)
        {
            int fiscalYear = fy;

            if (fiscalYear == 0)
            {
                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;

                fiscalYear = currentMonth > 9 ? currentYear + 1 : currentYear;
            }

            ViewBag.FY = fiscalYear;
            ViewBag.MaxCategoryid = await _fundingService.GetMaxCategoryIdAsync(fiscalYear);
            ViewBag.FundingCategories = await _fundingService.LoadFundingCategoriesAsync(fiscalYear);
            ViewBag.FundingDocuments = await _fundingService.LoadFundingDocsAsync(
                "ViewAll",
                0,
                fiscalYear,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return View("~/Views/FundingFiles/FundingMaster.cshtml");
        }

        /// <summary>
        /// Displays the funding index page
        /// </summary>
        [HttpGet]
        public IActionResult FundingIndex()
        {
            return View("~/Views/FundingFiles/Index.cshtml");
        }

        /// <summary>
        /// Searches funding documents by serial number
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewSearch(int serialNum, int fy)
        {
            ViewBag.FY = fy;
            ViewBag.SearchStr = serialNum.ToString();
            ViewBag.MaxCategoryid = await _fundingService.GetMaxCategoryIdAsync(fy);
            ViewBag.FundingCategories = await _fundingService.LoadFundingCategoriesAsync(fy);
            ViewBag.FundingDocuments = await _fundingService.LoadFundingDocsAsync(
                "view_search",
                serialNum,
                fy,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return View("~/Views/FundingFiles/FundingMaster.cshtml");
        }

        /// <summary>
        /// Views all funding documents for a fiscal year
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewAll(int fy)
        {
            ViewBag.FY = fy;
            ViewBag.MaxCategoryid = await _fundingService.GetMaxCategoryIdAsync(fy);
            ViewBag.FundingCategories = await _fundingService.LoadFundingCategoriesAsync(fy);
            ViewBag.FundingDocuments = await _fundingService.LoadFundingDocsAsync(
                "ViewAll",
                0,
                fy,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return View("~/Views/FundingFiles/FundingMaster.cshtml");
        }

        /// <summary>
        /// Views ARRA funding documents
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewArra(int fy)
        {
            ViewBag.FY = fy;
            ViewBag.MaxCategoryid = await _fundingService.GetMaxCategoryIdAsync(fy);
            ViewBag.FundingCategories = await _fundingService.LoadFundingCategoriesAsync(fy);
            ViewBag.FundingDocuments = await _fundingService.LoadFundingDocsAsync(
                "view_arra",
                0,
                fy,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return View("~/Views/FundingFiles/FundingMaster.cshtml");
        }

        /// <summary>
        /// Views funding documents for editing
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewEdit(int fy, string sortColumn = null, string sortDirection = "asc")
        {
            ViewBag.FY = fy;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;

            var fundingDocs = await _fundingService.LoadFundingDocsAsync(
                "view_edit",
                0,
                fy,
                sessionInfo.Ic,
                sessionInfo.UserId);

            // Apply sorting if specified
            if (!string.IsNullOrEmpty(sortColumn))
            {
                switch (sortColumn.ToLower())
                {
                    case "category_name":
                        fundingDocs = sortDirection == "asc"
                            ? fundingDocs.OrderBy(d => d.category_name).ToList()
                            : fundingDocs.OrderByDescending(d => d.category_name).ToList();
                        break;
                    case "full_grant_num":
                        fundingDocs = sortDirection == "asc"
                            ? fundingDocs.OrderBy(d => d.full_grant_num).ToList()
                            : fundingDocs.OrderByDescending(d => d.full_grant_num).ToList();
                        break;
                }
            }

            ViewBag.FundingDocs = fundingDocs;

            return View("~/Views/FundingFiles/FundingDocEdit.cshtml");
        }

        /// <summary>
        /// Displays the funding document creation page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FundingDocDefault(string adminCode, int serialNum, int applId, string previousUrl = null)
        {
            ViewBag.admincode = adminCode;
            ViewBag.serialnum = serialNum;
            ViewBag.applid = applId;
            ViewBag.Previousurl = previousUrl;

            ViewBag.AdminCodeList = await _commonService.LoadAdminCodes();
            ViewBag.CategoryList = await _fundingService.LoadFundingCategoryListAsync();
            ViewBag.GrantYearList = await _egrantsService.LoadApplsByApplid(applId);

            return View("~/Views/FundingFiles/FundingDocCreate.cshtml");
        }

        /// <summary>
        /// Loads applications for funding document creation
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LoadAppls(string adminCode, int serialNum, string previousUrl = null)
        {
            ViewBag.admincode = adminCode;
            ViewBag.serialnum = serialNum;
            ViewBag.Previousurl = previousUrl;

            ViewBag.AdminCodeList = await _commonService.LoadAdminCodes();
            ViewBag.CategoryList = await _fundingService.LoadFundingCategoryListAsync();
            ViewBag.GrantYearList = await _egrantsService.GetAllApplsListAsync(adminCode, serialNum.ToString());

            return View("~/Views/FundingFiles/FundingDocCreate.cshtml");
        }

        /// <summary>
        /// Creates funding document by drag and drop
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DocCreateByDdrop(
            IFormFile dropedfile,
            int applId,
            int categoryId,
            DateTime documentDate,
            string subCategory)
        {
            var result = await _fundingService.CreateFundingDocByDdropAsync(
                dropedfile,
                applId,
                categoryId,
                documentDate,
                subCategory,
                sessionInfo);

            return Json(new { url = result.Url, message = result.Message });
        }

        /// <summary>
        /// Creates PDF funding document by drag and drop (multiple files)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DocCreatePdfByDdrop(
            IEnumerable<IFormFile> dropedfiles,
            int applId,
            int categoryId,
            DateTime documentDate,
            string subCategory)
        {
            var result = await _fundingService.CreateFundingPdfByFilesAsync(
                dropedfiles,
                applId,
                categoryId,
                documentDate,
                subCategory,
                sessionInfo);

            return Json(new { url = result.Url, message = result.Message });
        }

        /// <summary>
        /// Creates funding document by file upload
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DocCreateByFile(
            IFormFile file,
            int applId,
            int categoryId,
            DateTime documentDate,
            string subCategory)
        {
            var result = await _fundingService.CreateFundingDocByFileAsync(
                file,
                applId,
                categoryId,
                documentDate,
                subCategory,
                sessionInfo);

            return Json(new { url = result.Url, message = result.Message });
        }

        /// <summary>
        /// Creates PDF funding document by file upload (multiple files)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DocCreatePdfByFile(
            IEnumerable<IFormFile> files,
            int applId,
            int categoryId,
            DateTime documentDate,
            string subCategory)
        {
            var result = await _fundingService.CreateFundingPdfByFilesAsync(
                files,
                applId,
                categoryId,
                documentDate,
                subCategory,
                sessionInfo);

            return Json(new { url = result.Url, message = result.Message });
        }

        /// <summary>
        /// Edits funding document (delete or restore)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DocEdit(string act, int applId, int docId, int fy, string sortColumn = null, string sortDirection = "asc")
        {
            ViewBag.FY = fy;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;

            await _fundingService.EditFundingDocAsync(act, applId, docId, sessionInfo.Ic, sessionInfo.UserId);

            var fundingDocs = await _fundingService.LoadFundingDocsAsync(
                "view_edit",
                0,
                fy,
                sessionInfo.Ic,
                sessionInfo.UserId);

            // Apply sorting if specified
            if (!string.IsNullOrEmpty(sortColumn))
            {
                switch (sortColumn.ToLower())
                {
                    case "category_name":
                        fundingDocs = sortDirection == "asc"
                            ? fundingDocs.OrderBy(d => d.category_name).ToList()
                            : fundingDocs.OrderByDescending(d => d.category_name).ToList();
                        break;
                    case "full_grant_num":
                        fundingDocs = sortDirection == "asc"
                            ? fundingDocs.OrderBy(d => d.full_grant_num).ToList()
                            : fundingDocs.OrderByDescending(d => d.full_grant_num).ToList();
                        break;
                }
            }

            ViewBag.FundingDocs = fundingDocs;

            return View("~/Views/FundingFiles/FundingDocEdit.cshtml");
        }

        /// <summary>
        /// Loads applications for editing document associations
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LoadDocAppls(int serialNum, string adminCode, int docId, int fy)
        {
            ViewBag.FY = fy;
            ViewBag.Docid = docId;
            ViewBag.admincode = adminCode;
            ViewBag.SerialNum = serialNum;

            ViewBag.AdminCodeList = await _commonService.LoadAdminCodes();
            ViewBag.GrantYearList = await _fundingService.LoadFullGrantNumbersAsync(serialNum, adminCode, docId);
            ViewBag.DocAppls = await _fundingService.LoadDocApplsAsync(docId);

            return View("~/Views/FundingFiles/FundingApplEdit.cshtml");
        }

        /// <summary>
        /// Displays the default application edit page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ApplEditDefault(int docId, int fy)
        {
            ViewBag.FY = fy;
            ViewBag.Docid = docId;

            ViewBag.AdminCodeList = await _commonService.LoadAdminCodes();
            ViewBag.DocAppls = await _fundingService.LoadDocApplsAsync(docId);

            return View("~/Views/FundingFiles/FundingApplEdit.cshtml");
        }

        /// <summary>
        /// Edits application association with funding document
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ApplEdit(string act, int applId, int docId, int fy)
        {
            ViewBag.FY = fy;
            ViewBag.Docid = docId;

            await _fundingService.EditFundingApplAsync(act, applId, docId, sessionInfo.Ic, sessionInfo.UserId);

            ViewBag.AdminCodeList = await _commonService.LoadAdminCodes();
            ViewBag.DocAppls = await _fundingService.LoadDocApplsAsync(docId);

            return View("~/Views/FundingFiles/FundingApplEdit.cshtml");
        }
    }
}