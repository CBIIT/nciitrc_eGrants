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
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

#endregion
namespace eGrants.Controllers.Egrants
{
    /// <summary>
    /// The egrants controller.
    /// </summary>
    public class EgrantsController : Controller
    {
        const int MAX_RETRIES = 3;
        // Injected dependencies: database context and product service

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
        /// </summary>
        /// <param name="appl">The application ID</param>
        /// <param name="fullGrantNumber">The full grant number</param>
        /// <param name="listOfUrl">List of URLs to download</param>
        /// <returns>JSON result with download model</returns>
        [HttpPost]
        public async Task<IActionResult> IsDownloadForm(string appl, string fullGrantNumber, [FromQuery] IList<string> listOfUrl)
        {

            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            var request = new DownloadRequest
            {
                ApplId = appl,
                FullGrantNumber = fullGrantNumber,
                ListOfUrl = listOfUrl,
                SessionInfo = sessionInfo
            };


            var downloadModel = await _documentService.ProcessDocumentDownloadAsync(request);

            // Store zip bytes in SESSION (.NET 8) if successful // was storing in TempData in legacy
            if (downloadModel.ZipFileBytes != null)
            {
                HttpContext.Session.Set(downloadModel.Handle, downloadModel.ZipFileBytes);
            }

            return Json(downloadModel);
        }

        /// <summary>
        /// Download action to serve the zip file
        /// </summary>
        /// <param name="fileGuid">The file GUID from TempData</param>
        /// <param name="fileName">The filename</param>
        /// <returns>File result</returns>
        public virtual IActionResult Download(string fileGuid, string fileName)
        {
            // Retrieve from SESSION instead of TempData
            var data = HttpContext.Session.Get(fileGuid);

            // Content-Disposition header set in ASP.NET Core,
            // so the explicit header addition (from legacy) is technically redundant, not required
            if (data != null)
            {
                HttpContext.Session.Remove(fileGuid);
                return File(data, "application/zip", fileName);
            }

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
            int applId = 0,
            string mode = null,
            string str = null)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            eGrantsSearchViewModel eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByApplAsync(applId, mode, str, sessionInfo);

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

        //    /// <summary>
        //    /// The by_filters_page.
        //    /// </summary>
        //    /// <param name="tab_num">
        //    /// The tab_num.
        //    /// </param>
        //    /// <param name="page_num">
        //    /// The page_num.
        //    /// </param>
        //    /// <param name="package">
        //    /// The package.
        //    /// </param>
        //    /// <param name="fy">
        //    /// The fy.
        //    /// </param>
        //    /// <param name="mechanism">
        //    /// The mechanism.
        //    /// </param>
        //    /// <param name="admincode">
        //    /// The admincode.
        //    /// </param>
        //    /// <param name="serialnum">
        //    /// The serialnum.
        //    /// </param>
        //    /// <returns>
        //    /// The <see cref="ActionResult"/>.
        //    /// </returns>
        //    public ActionResult by_filters_page(
        //        int tab_num = 0,
        //        int page_num = 0,
        //        string package = null,
        //        int fiscalYear = 0,
        //        string mechanism = null,
        //        string adminCode = null,
        //        int serialNumber = 0)
        //    {
        //        ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

        //        /*string.IsNullOrEmpty(admincode) &&*/
        //        if (fiscalYear == 0 && string.IsNullOrEmpty(mechanism) && serialNumber == 0)
        //        {
        //            ViewBag.Message = "No data found for the search";
        //            ViewBag.grantlayer = null;
        //        }
        //        else if (tab_num == 0 || page_num == 0 || string.IsNullOrEmpty(package) || package != "by_filters")
        //        {
        //            ViewBag.Message = "No data found for the search";
        //            ViewBag.grantlayer = null;
        //        }
        //        else
        //        {
        //            ViewBag.SearchStyle = package;
        //            ViewBag.CurrentTab = tab_num;
        //            ViewBag.CurrentPage = page_num;

        //            // create return value
        //            if (fiscalYear != 0)
        //                ViewBag.FilterFY = fiscalYear;
        //            else
        //                ViewBag.FilterFY = string.Empty;

        //            ViewBag.FilterMechanism = mechanism;
        //            ViewBag.FilterAdminCode = adminCode;

        //            if (serialNumber != 0)
        //                ViewBag.FilterSerialNumber = serialNumber;

        //            // create filters search sql query
        //            var FilterSearchQuery = Dashboard.Functions.Egrants.GetSearchQuery(
        //                fiscalYear,
        //                mechanism,
        //                adminCode,
        //                serialNumber,
        //                page_num,
        //                Convert.ToString(this.Session["browser"]),
        //                Convert.ToString(this.Session["ic"]),
        //                Convert.ToString(this.Session["userid"]));

        //            // load data
        //            Search.egrants_search(
        //                FilterSearchQuery,
        //                0,
        //                package,
        //                0,
        //                page_num,
        //                Convert.ToString(this.Session["browser"]),
        //                Convert.ToString(this.Session["ic"]),
        //                Convert.ToString(this.Session["userid"]));

        //            ViewBag.grantlayer = Search.grantlayerproperty;
        //            ViewBag.appllayer = Search.appllayerproperty;
        //            ViewBag.appllayer_All = Search.appllayerproperty;
        //            ViewBag.ApplCount = ViewBag.appllayer.Count;

        //            // show Pagination 
        //            ViewBag.Pagination = Dashboard.Functions.Egrants.LoadPagination(
        //                FilterSearchQuery,
        //                Convert.ToString(this.Session["ic"]),
        //                Convert.ToString(this.Session["userid"]),
        //                package);
        //        }

        //        return View("~/Egrants/Views/Index.cshtml");
        //    }

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

        public JsonResult LoadDocsGridForDownload(int appl_id, string search_type = null, string category_list = null, string mode = null)
        {
            //Search_by_appl_id.LoadDocs(
            //    appl_id,
            //    search_type,
            //    category_list,
            //    Convert.ToString(this.Session["ic"]),
            //    Convert.ToString(this.Session["userid"]));

            //ViewBag.doclayer = Search_by_appl_id.doclayerproperty;

            //// ViewBag.doclayer = Search_by_appl_id.doclayerproperty.ToList();
            //dynamic res = new { data = ViewBag.doclayer };

            //return Json(res, JsonRequestBehavior.AllowGet);
            return Json(null);
        }

        //    /// <summary>
        //    /// The stop_notice.
        //    /// </summary>
        //    /// <param name="grant_id">
        //    /// The grant_id.
        //    /// </param>
        //    /// <returns>
        //    /// The <see cref="ActionResult"/>.
        //    /// </returns>
        //    public ActionResult stop_notice(int grant_id)
        //    {
        //        ViewBag.StopNotice = Dashboard.Functions.Egrants.LoadStopNotice(grant_id, Convert.ToString(this.Session["ic"]));

        //        return View("~/Egrants/Views/_Modal_Stop_Notice.cshtml");
        //    }

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

        //    public string impac_docs_data(string act, int appl_id)
        //    {
        //        try
        //        {
        //            ViewBag.ImpacDocs = EgrantsDoc.LoadImpacDocs(act, appl_id);
        //            ViewBag.act = act;
        //            ViewBag.appl_id = appl_id;

        //            List<ImpacDocs> list = EgrantsDoc.LoadImpacDocs(act, appl_id);
        //            return JsonConvert.SerializeObject(list);
        //        }
        //        catch (Exception err)
        //        {
        //            Console.WriteLine(err);
        //        }

        //        return null;
        //    }

        //    public string doc_attachments_data(int document_id)
        //    {
        //        try
        //        {

        //            List<DocAttachment> list = EgrantsDoc.LoadDocAttachments(document_id);

        //            return JsonConvert.SerializeObject(list);

        //        }
        //        catch (Exception err)
        //        {
        //            Console.WriteLine(err);
        //        }

        //        return null;
        //    }
        //}

    }
}