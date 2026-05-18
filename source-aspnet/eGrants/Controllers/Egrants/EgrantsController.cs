#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EgrantsController.cs
// Solution: eGrants
// Project:  eGrants
// Created: 2025-08-01
// Contributors:
//      - Dehuff, Daryl (NIH/NCI) [C] - dehuffdc
// Copyright (c) National Institute of Health
// 
// <Description of the file>
// 
// MIGRATION NOTES (.NET Framework 4.8 to .NET 8):
// ------------------------------------------------
// This controller was migrated from the legacy egrants_new project. Key changes include:
// 
// 1. DEPENDENCY INJECTION: 
//    - Legacy used static classes and Session directly
//    - .NET 8 uses constructor injection for services (IeGrantsService, IDocumentService, etc.)
//    - SessionInfo is now retrieved via ISessionInfoService instead of direct Session access
//
// 2. IsDownloadForm ACTION:
//    - Legacy: Used ActionResult with Json(downloadModel, JsonRequestBehavior.AllowGet)
//    - .NET 8: Uses async Task<IActionResult> with Json(downloadModel)
//    - Legacy stored zip bytes in TempData[handle]
//    - .NET 8: Stores zip bytes in HttpContext.Session.Set() because TempData has size limits
//    - Parameter binding changed from implicit to explicit [FromForm] attributes
//    - Download logic moved to DocumentService.ProcessDocumentDownloadAsync()
//
// 3. Download ACTION:
//    - Legacy: Retrieved data from TempData[fileGuid]
//    - .NET 8: Retrieves from HttpContext.Session.Get(fileGuid)
//    - Content-Disposition header is now handled automatically by ASP.NET Core's File() method
//
// 4. ASYNC PATTERNS:
//    - Most actions converted to async/await pattern for better scalability
//    - Database operations now use async methods
// ------------------------------------------------
// 
// This source is subject to the NIH Software License.
// See https://ncihub.org/resources/899/download/Guidelines_for_Releasing_Research_Software_04062015.pdf
// All other rights reserved.
// \***************************************************************************/

#endregion

#region

using eGrants.Models;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Serilog;

#endregion
namespace eGrants.Controllers.Egrants
{
    /// <summary>
    /// The egrants controller.
    /// Handles main eGrants functionality including document downloads, search, and grant management.
    /// 
    /// MIGRATION CHANGES SUMMARY:
    /// -------------------------
    /// This controller underwent significant changes during the .NET Framework 4.8 to .NET 8 migration:
    /// 
    /// 1. DEPENDENCY INJECTION PATTERN:
    ///    WHY: .NET 8 strongly encourages DI for better testability, maintainability, and loose coupling.
    ///    The legacy code used static classes and direct Session access which made unit testing difficult.
    ///    Services are now injected via constructor (IeGrantsService, IDocumentService, etc.)
    /// 
    /// 2. SESSION HANDLING CHANGES:
    ///    WHY: ASP.NET Core's session is accessed differently than .NET Framework.
    ///    Legacy: Session["key"] direct access
    ///    .NET 8: HttpContext.Session.GetString("key") or via ISessionInfoService abstraction
    ///    This abstraction also improves testability by allowing mock sessions in unit tests.
    /// 
    /// 3. ASYNC/AWAIT PATTERN:
    ///    WHY: .NET 8 emphasizes async patterns for better scalability and thread utilization.
    ///    Synchronous I/O operations can block threads, reducing server throughput.
    ///    Most actions are now async Task<IActionResult> instead of ActionResult.
    /// 
    /// 4. DOWNLOAD MECHANISM REWRITE (IsDownloadForm & Download actions):
    ///    WHY: Multiple breaking changes required a complete rewrite:
    ///    - TempData size limits: ASP.NET Core TempData has serialization overhead and size constraints.
    ///      Large zip files cannot be stored in TempData, so we now use HttpContext.Session.Set().
    ///    - HttpClient replaces WebClient/HttpWebRequest: .NET 8 deprecates WebClient.
    ///      HttpClient with SocketsHttpHandler provides better TLS 1.2/1.3 support and connection pooling.
    ///    - Certificate handling: X509Certificate2 now requires specific KeyStorageFlags for web apps.
    ///    - Form data binding: Explicit [FromForm] attributes replace implicit model binding.
    /// 
    /// 5. JSON SERIALIZATION:
    ///    WHY: .NET 8 defaults to System.Text.Json, but Newtonsoft.Json is retained for compatibility
    ///    with complex serialization scenarios and existing API contracts.
    /// </summary>
    public class EgrantsController : Controller
    {
        const int MAX_RETRIES = 3;

        // .NET 8 Migration: Services are now injected via constructor instead of using static classes
        // This enables better testability and follows SOLID principles
        private readonly IeGrantsService _eGrantsService;
        private readonly IDocumentService _documentService;
        private readonly ICommonService _commonService;
        private readonly ISessionInfoService _sessionInfoService;

        public EgrantsController(IeGrantsService eGrantsService, ICommonService commonService, IDocumentService documentService, ISessionInfoService sessionInfoService)
        {
            _eGrantsService = eGrantsService;
            _commonService = commonService;
            _sessionInfoService = sessionInfoService;
            _documentService = documentService;
        }

        // go to default 
        /// <summary>
        /// The go_to_default.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.CIS
        /// </returns>
        public ActionResult Go_to_default()
        {
            return View("~/Views/Shared/Go_to_Default.cshtml");
        }

        // GET: Egrants
        /// <summary>   
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<IActionResult> Index()
        {
            // May want to move this to a base controller, an action filter, or use a shared service in the long term.

            eGrantsSearchViewModel eGrantsSearchViewModelList = new eGrantsSearchViewModel();

            eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();

            return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
        }


        /// <summary>
        /// HttpPost - Download files and create zip
        /// 
        /// MIGRATION NOTES:
        /// ----------------
        /// Legacy (.NET Framework):
        /// - Used synchronous ActionResult
        /// - Parameters bound implicitly from form/query
        /// - Created HttpWebRequest for ERA files with certificate
        /// - Used WebClient for standard file downloads
        /// - Stored zip bytes in TempData[handle]
        /// - Used ViewAsPdf (Rotativa) for closeout notifications
        /// 
        /// .NET 8:
        /// - Uses async Task<IActionResult> for non-blocking I/O
        /// - [FromForm] attributes explicitly bind parameters from POST body
        /// - Download logic moved to DocumentService.ProcessDocumentDownloadAsync()
        /// - Uses HttpClient with HttpClientHandler for all HTTP requests
        /// - Stores zip bytes in Session (TempData has size limits in .NET Core)
        /// - Uses EmailConcatenation.PdfConverter for closeout notification PDFs
        /// - Certificate handling uses X509Certificate2 with SslProtocols.Tls12 | Tls13
        /// </summary>
        /// <param name="appl">The application ID</param>
        /// <param name="fullGrantNumber">The full grant number</param>
        /// <param name="listOfUrl">List of URLs to download (pipe-delimited format: url|category|subcategory|docId|docName|docDate)</param>
        /// <returns>JSON result with download model containing success/failure status and zip file handle</returns>
        [HttpPost]
        public async Task<IActionResult> IsDownloadForm([FromForm] string appl, [FromForm] string fullGrantNumber, [FromForm] IList<string> listOfUrl)
        {
            // .NET 8 Migration: SessionInfo retrieved via service instead of direct Session access
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // .NET 8 Migration: Request object encapsulates all download parameters
            // This replaces the inline parameter handling in the legacy controller
            var request = new DownloadRequest
            {
                ApplId = appl,
                FullGrantNumber = fullGrantNumber,
                ListOfUrl = listOfUrl,
                SessionInfo = sessionInfo
            };

            // .NET 8 Migration: Download processing moved to service layer
            // Legacy had all download logic inline in the controller action
            var downloadModel = await _documentService.ProcessDocumentDownloadAsync(request);

            // .NET 8 Migration: Store zip bytes in Session instead of TempData
            // TempData in ASP.NET Core has size limitations and requires serialization
            // Session.Set() handles byte arrays directly
            if (downloadModel.ZipFileBytes != null)
            {
                HttpContext.Session.Set(downloadModel.Handle, downloadModel.ZipFileBytes);
            }

            return Json(downloadModel);
        }

        /// <summary>
        /// Download action to serve the zip file
        /// 
        /// MIGRATION NOTES:
        /// ----------------
        /// Legacy (.NET Framework):
        /// - Retrieved byte[] from TempData[fileGuid]
        /// - Manually set Content-Disposition header via Response.AppendHeader()
        /// - Used ContentDisposition class for header formatting
        /// 
        /// .NET 8:
        /// - Retrieves byte[] from HttpContext.Session.Get()
        /// - File() method automatically sets Content-Disposition header
        /// - Returns NotFound() instead of EmptyResult for missing files
        /// </summary>
        /// <param name="fileGuid">The file GUID (handle) stored in session</param>
        /// <param name="fileName">The filename to use for the download</param>
        /// <returns>File result with zip content or NotFound</returns>
        public virtual IActionResult Download(string fileGuid, string fileName)
        {
            // .NET 8 Migration: Retrieve from Session instead of TempData
            var data = HttpContext.Session.Get(fileGuid);

            if (data != null)
            {
                // Clean up session after retrieving the file
                HttpContext.Session.Remove(fileGuid);

                // .NET 8 Migration: File() method handles Content-Disposition automatically
                // Third parameter (fileName) triggers attachment disposition
                return File(data, "application/zip", fileName);
            }

            // .NET 8 Migration: Return proper 404 instead of EmptyResult
            return NotFound();
        }

        // get appls list with documents by (admin_code and serial_num) added by Ayu at 3/15/2019
        /// <summary>
        /// The load years.
        /// </summary>
        /// <param name="fiscalYear">
        /// The fy.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism.
        /// </param>
        /// <param name="adminCode">
        /// The adminCode.
        /// </param>
        /// <param name="serialNumber">
        /// The serialNumber.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public async Task<string> LoadYears(
            string fiscalYear = null,
            string mechanism = null,
            string adminCode = null,
            string serialNumber = null)
        {
            var yearList = new List<string>();
            var list = await _eGrantsService.GetYearList(fiscalYear, mechanism, adminCode, serialNumber);

            foreach (GrantDataYears val in list)
            {
                yearList.Add(val.full_grant_num + ":" + val.appl_id);
            }

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(yearList);
        }

        // load all appls list with or without documents
        /// <summary>
        /// The get all appls list.
        /// </summary>
        /// <param name="adminCode">
        /// The admin_code.
        /// </param>
        /// <param name="serialNum">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public async Task<string> GetAllApplsList(string adminCode, string serialNum)
        {
            var list = await _eGrantsService.GetAllApplsListAsync(adminCode, serialNum);

            return JsonConvert.SerializeObject(list);
        }

        // get category list by grant_id and years
        /// <summary>
        /// The load categories.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <param name="years">
        /// The years.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public async Task<string> LoadCategories(int grantId, string years)
        {
            var list = await _eGrantsService.GetCategoryList(grantId, years);

            return JsonConvert.SerializeObject(list);
        }

        // get category list by grant_id and years
        /// <summary>
        /// The load categories.
        /// </summary>
        /// <param name="name">
        /// The new label for the grant year
        /// </param>
        /// <param name="applId">
        /// The appl_id for the grant year about to be renamed
        /// </param>
        /// <returns>
        /// The function returns true if successful<see cref="bool"/>.
        /// </returns>
        public bool NewGrantYearName(string name, int applId)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = string.Empty;
            }
            var length = name.Length;
            var truncatedName = name.Substring(0, Math.Min(length, 10));

            _eGrantsService.SetGrantYearLabel(name, applId);

            return true;
        }


        /// <summary>
        /// The by_str.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> by_str(string str, string mode = null)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            eGrantsSearchViewModel eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByStrAsync(str, 0, 0, 0, sessionInfo);

            eGrantsSearchViewModelList.Mode = mode;
            eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();
            return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
        }

        /// <summary>
        /// The by_grant.
        /// </summary>
        /// <param name="grantId">
        /// The grant_id.
        /// </param>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <param name="categories">
        /// The categories.
        /// </param>
        /// <param name="applsList">
        /// The appls_list.
        /// </param>
        /// <param name="years">
        /// The years.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<IActionResult> by_grant(
            int grantId = 0,
            string package = "",
            string categories = "",
            string applsList = "",
            string years = "",
            string mode = "")
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            eGrantsSearchViewModel eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByGrantAsync(string.Empty,
                grantId, package, 0, 0, categories, applsList, years, mode, sessionInfo);

            eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();
            return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
        }

        /// <summary>
        /// The by_grant.
        /// </summary>
        /// <param name="grantId">
        /// The grant_id.
        /// </param>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <param name="categories">
        /// The categories.
        /// </param>
        /// <param name="applsList">
        /// The appls_list.
        /// </param>
        /// <param name="years">
        /// The years.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<IActionResult> by_appl(
            int appl_id = 0,
            string mode = null,
            string str = null)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            eGrantsSearchViewModel eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByApplAsync(appl_id, mode, str, sessionInfo);

            eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();

            return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
        }


        /// <summary>
        /// The by_qc.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> by_qc(string str = null)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            eGrantsSearchViewModel eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByQCAsync("qc", 0, string.Empty, 0, 1, sessionInfo, _documentService);

            eGrantsSearchViewModelList.Mode = "qc";
            eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();
            return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
        }

        /// <summary>
        /// The by_filters.
        /// </summary>
        /// <param name="fiscalYear">
        /// The fiscalYear.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism.
        /// </param>
        /// <param name="adminCode">
        /// The adminCode.
        /// </param>
        /// <param name="serialNum">
        /// The serialNumber.
        /// </param>
        /// <param name="pageNum">
        /// The page number
        /// </param>
        /// <param name="tabNum">
        /// The tab number
        /// </param>
        /// <param name="packages">
        /// The package name
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<IActionResult> by_filters(int fiscalYear = 0, string mechanism = null, string adminCode = null, int serialNum = 0, int pageNum = 1, int tabNum = 1, string packages = "")
        {
            eGrantsSearchViewModel eGrantsSearchViewModelList = new eGrantsSearchViewModel();

            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByFilterAsync(fiscalYear, mechanism, serialNum, adminCode, 0, 0, pageNum, sessionInfo, tabNum, packages);
            eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();

            return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
        }

        /// <summary>
        /// The by_page.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="tabNum">
        /// The tab_num.
        /// </param>
        /// <param name="pageNum">
        /// The page_num.
        /// </param>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> by_page(string str = null, int tabNum = 0, int pageNum = 0, string package = null, string mode = null)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            eGrantsSearchViewModel eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByPageAsync(str, 0, 0, pageNum, tabNum, sessionInfo, _documentService);

            eGrantsSearchViewModelList.Mode = str == "qc" ? "qc" : mode;
            eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();
            return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
        }

        // Autocomplete for fy, activity_code and serial_number
        /// <summary>
        /// The load_data_autocomplete.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="term">
        /// The term.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism.
        /// </param>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <param name="admincode">
        /// The admincode.
        /// </param>
        /// <param name="serialnum">
        /// The serialnum.
        /// </param>
        /// <returns>
        /// The <see cref="JsonResult"/>.
        /// </returns>
        public async Task<JsonResult> load_data_autocomplete(
            string type,
            string term,
            string mechanism = null,
            string fy = null,
            string adminCode = null,
            string serialNum = null)
        {
            var viewModel = new eGrantsSearchViewModel
            {
                admincode = string.IsNullOrWhiteSpace(adminCode) || adminCode == "undefined" ? string.Empty : adminCode,
                FilterMechanism = mechanism,
                FilterAdminCode = adminCode
            };

            if (int.TryParse(fy, out int parsedFy))
                viewModel.FilterFY = parsedFy;
            else
                fy = null;

            if (int.TryParse(serialNum, out int parsedSerial))
                viewModel.FilterSerialNumber = parsedSerial;
            else
                serialNum = null;

            viewModel.ICList = await _commonService.LoadAdminCodes();

            var dataList = await _eGrantsService.LoadDataAutocomplete(type, term, mechanism, fy, adminCode, serialNum);

            return Json(dataList);
        }

        // load documents by appl_id
        /// <summary>
        /// The load docs grid.
        /// </summary>
        /// <param name="applId">
        /// The appl_id.
        /// </param>
        /// <param name="searchType">
        /// The search_type.
        /// </param>
        /// <param name="categoryList">
        /// The category_list.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="JsonResult"/>.
        /// </returns>
        /// 
        public JsonResult LoadDocsGrid(int applId, string searchType = null, string categoryList = null, string mode = null)
        {
            var docs = _documentService.LoadDocs(applId, searchType, categoryList, mode, HttpContext.Session);
            return Json(new { data = docs });
        }

        /// <summary>
        /// The stop_notice.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult stop_notice(int grant_id)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            ViewBag.StopNotice = _eGrantsService.LoadStopNotice(grant_id, sessionInfo.Ic);

            return View("~/Views/Egrants/_Modal_Stop_Notice.cshtml");
        }

        /// <summary>
        /// The supplement.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> supplement(int grant_id)
        {
            var act = "to_view";
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            List<supplement> supplements = await _eGrantsService.GetSupplements(act,
                grant_id,
                0,
                string.Empty,
                string.Empty,
                0,
                sessionInfo.Ic,
                sessionInfo.UserId);

            SupplementObjectViewModel supplementObjectViewModel = new SupplementObjectViewModel();

            supplementObjectViewModel.GrantID = grant_id;
            supplementObjectViewModel.Act = act;
            supplementObjectViewModel.Supplement = supplements;
            supplementObjectViewModel.FormerAppls = new List<former_appls>();

            return View("~/Views/eGrants/_Modal_Supplement.cshtml", supplementObjectViewModel);
        }

        /// <summary>
        /// Gets IMPAC docs data as JSON string for the specified application (legacy format)
        /// </summary>
        /// <param name="act">The action to perform</param>
        /// <param name="appl_id">The application ID</param>
        /// <returns>JSON serialized string of IMPAC docs</returns>
        [HttpPost]
        public async Task<string> impac_docs_data(string act, int appl_id)
        {
            try
            {
                var list = await _eGrantsService.LoadImpacDocs(act, appl_id);
                return JsonConvert.SerializeObject(list);
            }
            catch (Exception ex)
            {
                // Log error appropriately
                Log.Error($"Error loading IMPAC docs: {ex}");
                return null;
            }
        }

        public async Task<string> doc_attachments_data(int document_id)
        {
            try
            {

                List<DocAttachment> list = await _documentService.LoadDocAttachmentsAsync(document_id);

                return JsonConvert.SerializeObject(list);

            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }

            return null;
        }
    }
}
