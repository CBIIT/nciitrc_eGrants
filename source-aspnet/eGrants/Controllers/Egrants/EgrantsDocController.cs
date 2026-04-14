#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EgrantsDocController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2025-12-03
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

//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Security.Cryptography.X509Certificates;
//using System.Text;
//using System.Web;
//using System.Web.Mvc;

//using DocumentFormat.OpenXml.Wordprocessing;

//using eGrants.Services.Interfaces;

//using egrants_new.Egrants.Functions;
//using egrants_new.Functions;
//using egrants_new.Integration.WebServices;
//using egrants_new.Models;

//using EmailConcatenation;

//using IronPdf;

//using Microsoft.AspNetCore.Mvc;

//using MsgReader.Outlook;

//using Newtonsoft.Json;

//using WebGrease.Activities;

//using static System.Net.WebRequestMethods;
//using static egrants_new.Egrants_Admin.Models.Supplement;

#endregion

using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

using eGrants.Common;
using eGrants.Functions;
using eGrants.Models;
using eGrants.Services;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using EmailConcatenation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

using MsgReader.Outlook;

using Serilog;

namespace eGrants.Controllers.Egrants
{
    /// <summary>
/// The egrants doc controller.
    /// Handles document-related operations including viewing, uploading, creating, and modifying documents.
    /// 
    /// MIGRATION CHANGES SUMMARY:
    /// -------------------------
    /// This controller was migrated from egrants_new (.NET Framework 4.8) to .NET 8. Key changes:
    /// 
    /// 1. DEPENDENCY INJECTION:
    ///    WHY: .NET 8 requires constructor-based DI instead of static helper classes.
    ///    Legacy code used static classes like EgrantsDoc.LoadFormerAppls() directly.
    ///    Now uses injected services (IDocumentService, IeGrantsService, etc.) for testability.
    /// 
    /// 2. SESSION INFO PROPERTY:
    ///    WHY: Provides cleaner access to session data throughout the controller.
    ///The sessionInfo property uses ISessionInfoService to abstract Session access,
    ///    making the code more testable and consistent with .NET 8 patterns.
  /// 
    /// 3. SHOW_ERA_DOC ACTION CHANGES:
    ///  WHY: Complete rewrite required due to .NET 8 HTTP client changes:
    ///    - SocketsHttpHandler replaces HttpClientHandler for better TLS control
    ///    - Explicit SslProtocols.Tls12 | Tls13 required (older protocols deprecated)
    ///    - X509Certificate2 KeyStorageFlags (MachineKeySet, PersistKeySet, Exportable)
    ///    are required for IIS/web app environments to properly load private keys
    ///    - Comprehensive error handling with Serilog logging for diagnostics
    /// 
    /// 4. FILE UPLOAD CHANGES (IFormFile):
    ///    WHY: ASP.NET Core uses IFormFile instead of HttpPostedFileBase.
 ///    - IFormFile provides async streaming (CopyToAsync) for better performance
    ///    - OpenReadStream() replaces InputStream property
    ///    - File operations moved to service layer for separation of concerns
  /// 
    /// 5. PDF CONVERSION (EmailConcatenation.PdfConverter):
    ///  WHY: The legacy Rotativa/ViewAsPdf approach doesn't work in .NET 8.
    ///    EmailConcatenation.PdfConverter provides cross-platform PDF generation
    ///    without requiring external browser dependencies.
    /// 
    /// 6. RESPONSE CACHING ATTRIBUTES:
    ///    WHY: [OutputCache(NoStore = true)] replaced with [ResponseCache(...)]
    ///    ASP.NET Core uses different caching attributes and middleware.
 /// 
    /// 7. CONFIGURATION ACCESS:
    ///WHY: IConfiguration replaces ConfigurationManager.AppSettings
    ///    .NET 8 uses appsettings.json and IConfiguration for settings access.
    /// </summary>
    public class EgrantsDocController : Controller
    {

        private readonly IeGrantsService _eGrantsService;
        private readonly IDocumentService _documentService;
        private readonly ICommonService _commonService;
        private readonly IApplService _applService;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly IConfiguration _configuration;
        private readonly EgrantsCommon _egrantsCommon;

        private SessionInfo sessionInfo => _sessionInfoService.GetSessionInfo(HttpContext.Session);

        public EgrantsDocController(IeGrantsService eGrantsService, ICommonService commonService, IDocumentService documentService, ISessionInfoService sessionInfoService, IApplService applService, IConfiguration configuration = null, EgrantsCommon egrantsCommon = null)
        {
            _eGrantsService = eGrantsService;
            _commonService = commonService;
            _sessionInfoService = sessionInfoService;
            _documentService = documentService;
            _configuration = configuration;
            _egrantsCommon = egrantsCommon;
            _applService = applService;
        }

        // GET: Egrants
        /// <summary>
        /// The report error index.
        /// </summary>
        /// <param name="document_id">
        /// The document_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult ReportErrorIndex(int document_id)
        {
            this.ViewBag.DocID = 1; // document_id;

            return this.View("~/Views/Egrants/_Modal_Report_Error.cshtml");
        }

        /// <summary>
        /// The report error.
        /// </summary>
        /// <param name="errormsg">
        /// The errormsg.
        /// </param>
        /// <param name="document_id">
        /// The document_id.
        /// </param>
        /// <param name="currenturl">
        /// The currenturl.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult ReportError(string errormsg, int document_id, string currenturl)
        {
            this.ViewBag.DocID = document_id;
            this.ViewBag.Errormsg = errormsg;
            _documentService.report_doc_error(errormsg, document_id, sessionInfo.Ic, sessionInfo.UserId);

            return this.Redirect(currenturl);
        }


        /// <summary>
        /// Show ERA document by retrieving temporary download link
        /// </summary>
        /// <param name="docurl">The document URL</param>
        /// <returns>Redirect to temporary download link or error view</returns>
        public async Task<IActionResult> show_era_doc(string docurl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(docurl))
                {
                    Log.Error("show_era_doc called with null or empty docurl");
                    return BadRequest("Document URL is required.");
                }

                var certPath = _configuration["AppSettings:certPath"];
                var certPass = _configuration["AppSettings:certPass"];

                if (string.IsNullOrEmpty(certPath) || !System.IO.File.Exists(certPath))
                {
                    Log.Error("Certificate not found at path: {CertPath}", certPath);
                    return StatusCode(500, "Server configuration error: Certificate not found.");
                }

                // Load certificate with proper key storage flags for ASP.NET Core / .NET 8
                var certificate = new X509Certificate2(
                    certPath,
                    certPass,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

                Log.Information("Certificate loaded successfully. Subject: {Subject}, HasPrivateKey: {HasPrivateKey}",
                    certificate.Subject, certificate.HasPrivateKey);

                // Use SocketsHttpHandler for better TLS control in .NET 8
                var handler = new SocketsHttpHandler
                {
                    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                    {
                        EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
                        ClientCertificates = new X509Certificate2Collection { certificate },
                        RemoteCertificateValidationCallback = (message, cert, chain, errors) =>
                        {
                            // Log certificate validation details for debugging
                            if (errors != System.Net.Security.SslPolicyErrors.None)
                            {
                                Log.Warning("SSL Certificate validation warning: {Errors}", errors);
                            }
                            return true; // Accept the server certificate (adjust as needed for security)
                        }
                    },
                    AllowAutoRedirect = false
                };

                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("User-Agent", "eGrants");
                client.Timeout = TimeSpan.FromSeconds(30);

                Log.Information("Requesting ERA document: {DocUrl}, Certificate HasPrivateKey: {HasPrivateKey}",
                    docurl, certificate.HasPrivateKey);

                var response = await client.GetAsync(docurl);

                Log.Information("ERA response status: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var truncatedContent = errorContent.Length > 200
                        ? errorContent.Substring(0, 200)
                        : errorContent;
                    Log.Error("ERA request failed. Status: {Status}, Content: {Content}",
                        response.StatusCode, truncatedContent);

                    return StatusCode((int)response.StatusCode,
     $"Failed to retrieve document from ERA. Status: {response.StatusCode}");
                }

                var tempLink = await response.Content.ReadAsStringAsync();
                tempLink = tempLink?.Trim();

                if (string.IsNullOrWhiteSpace(tempLink))
                {
                    Log.Error("ERA returned empty or null temporary link for URL: {DocUrl}", docurl);
                    return StatusCode(502, "ERA service returned an empty response.");
                }

                // Validate that the response looks like a URL
                if (!Uri.TryCreate(tempLink, UriKind.Absolute, out var validatedUri) ||
                    (validatedUri.Scheme != Uri.UriSchemeHttp && validatedUri.Scheme != Uri.UriSchemeHttps))
                {
                    Log.Error("ERA returned invalid URL: {TempLink} for DocUrl: {DocUrl}",
                        tempLink.Length > 200 ? tempLink.Substring(0, 200) : tempLink, docurl);
                    return StatusCode(502, "ERA service returned an invalid response.");
                }

                Log.Information("Redirecting to temporary link: {TempLink}", tempLink);

                return Redirect(tempLink);
            }
            catch (TaskCanceledException ex)
            {
                Log.Error(ex, "Request timeout in show_era_doc for URL: {DocUrl}", docurl);
                return StatusCode(504, "Request to ERA service timed out.");
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "HTTP request error in show_era_doc for URL: {DocUrl}", docurl);
                return StatusCode(502, "Failed to connect to ERA service.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error in show_era_doc for URL: {DocUrl}", docurl);
                return StatusCode(500, "An unexpected error occurred while retrieving the document.");
            }
        }

        public async Task<ActionResult> LoadSupplementDoc(string act, int grantId)
        {
            //this.ViewBag.FormerAppls = EgrantsDoc.LoadFormerAppls(grant_id);
            List<supplement> supplements = await _eGrantsService.GetSupplements(act,
                grantId,
                0,
                string.Empty,
                string.Empty,
                0,
                sessionInfo.Ic,
                sessionInfo.UserId);

            SupplementObjectViewModel supplementObjectViewModel = new SupplementObjectViewModel();

            supplementObjectViewModel.GrantID = grantId;
            supplementObjectViewModel.Act = act;
            supplementObjectViewModel.Supplement = supplements;
            supplementObjectViewModel.FormerAppls = await _documentService.loadFormerAppls(grantId);

            return View("~/Views/eGrants/_Modal_Supplement.cshtml", supplementObjectViewModel);
        }

        /// <summary>
        /// The process supplement doc.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <param name="support_year">
        /// The support_year.
        /// </param>
        /// <param name="suffix_code">
        /// The suffix_code.
        /// </param>
        /// <param name="former_applid">
        /// The former_applid.
        /// </param>
        /// <param name="docid_str">
        /// The docid_str.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task <ActionResult> ProcessSupplementDoc(string act, int grant_id, int support_year, string suffix_code, int former_applid, string docid_str)
        {
            ViewBag.Status = "Done";
            ViewBag.GrantID = grant_id;
            ViewBag.FormerAppls = await _documentService.loadFormerAppls(grant_id);

            ViewBag.Supplement = _eGrantsService.GetSupplements(
                act,
                grant_id,
                support_year,
                suffix_code,
                docid_str,
                former_applid,
                sessionInfo.Ic,
                sessionInfo.UserId);

            SupplementObjectViewModel supplementObjectViewModel = new SupplementObjectViewModel();

            supplementObjectViewModel.GrantID = grant_id;
            supplementObjectViewModel.Act = act;
            supplementObjectViewModel.Supplement = ViewBag.Supplement;
            supplementObjectViewModel.FormerAppls = await _documentService.loadFormerAppls(grant_id);

            return this.View("~/Views/Egrants/_Modal_Supplement.cshtml", supplementObjectViewModel);
        }

        /// <summary>
        /// The load supplement.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> LoadSupplement(string act, int grantId)
        {
            List<supplement> supplements = await _eGrantsService.GetSupplements(act,
                grantId,
                0,
                string.Empty,
                string.Empty,
                0,
                sessionInfo.Ic,
                sessionInfo.UserId);

            SupplementObjectViewModel supplementObjectViewModel = new SupplementObjectViewModel();

            supplementObjectViewModel.GrantID = grantId;
            supplementObjectViewModel.Act = act;
            supplementObjectViewModel.Supplement = supplements;
            supplementObjectViewModel.FormerAppls = await _documentService.loadFormerAppls(grantId);

            return View("~/Views/eGrants/Supplement.cshtml", supplementObjectViewModel);
        }

        public async Task<ActionResult> ProcessSupplement(string act, int grantId, int supportYear, string suffixCode, int formerApplId, string docIdStr)
        {
            List<supplement> supplements = await _eGrantsService.GetSupplements(act,
                grantId,
                supportYear,
                suffixCode,
                docIdStr,
                formerApplId,
                sessionInfo.Ic,
                sessionInfo.UserId);

            SupplementObjectViewModel supplementObjectViewModel = new SupplementObjectViewModel();

            supplementObjectViewModel.GrantID = grantId;
            supplementObjectViewModel.Supplement = supplements;
            supplementObjectViewModel.FormerAppls = await _documentService.loadFormerAppls(grantId);
            supplementObjectViewModel.Status = "Done";

            return View("~/Views/eGrants/Supplement.cshtml", supplementObjectViewModel);
        }

        // modify doc for delete, store or modify doc index
        /// <summary>
        /// The doc_modify.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="docids">
        /// The docids.
        /// </param>
        public void doc_modify(string act, string docids)
        {
            ViewBag.Status = "Done";
            _documentService.DocModify(act, 0, 0, string.Empty, string.Empty, docids, string.Empty, sessionInfo.Ic, sessionInfo.UserId);
        }

        // to create new doc
        /// <summary>
        /// The doc_create_with_applid.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="document_id">
        /// The document_id.
        /// </param>
        /// <param name="category_id">
        /// The category_id.
        /// </param>
        /// <param name="sub_category">
        /// The sub_category.
        /// </param>
        /// <param name="document_date">
        /// The document_date.
        /// </param>
        /// <param name="previous_url">
        /// The previous_url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> doc_create_with_applid(
            string act,
            string admin_code,
            int serial_num,
            int appl_id = 0,
            int document_id = 0,
            int category_id = 0,
            string sub_category = "",
            string document_date = "",
            string previous_url = "")
        {
            var userId = sessionInfo.UserId;
            if (userId == "hindsrr")
            {
                sessionInfo.Ic = "NCI";
            }

            eGrantsDocCreateViewModel eDocViewModel = new eGrantsDocCreateViewModel();
            eDocViewModel.Act = "Add";
            eDocViewModel.AdminCode = admin_code;
            eDocViewModel.SerialNum = serial_num;
            eDocViewModel.ApplId = appl_id;
            eDocViewModel.PreviousUrl = previous_url;
            eDocViewModel.AdminCodeList = await _commonService.LoadAdminCodes();
            eDocViewModel.CategoryList = await _documentService.LoadCategories(sessionInfo.Ic); // load categories that could only be upload
            eDocViewModel.SubCategoryList = await _documentService.LoadSubCategoryList();
            eDocViewModel.MaxCategoryId = await _documentService.GetMaxCategoryid(sessionInfo.Ic);

            return this.View("~/Views/Egrants/EgrantsDocCreate.cshtml", eDocViewModel);
        }

        // create new doc without selected appl_id
        /// <summary>
        /// The doc_create_without_applid.
        /// </summary>
        /// <param name="previousUrl">
        /// The previous_url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> doc_create_without_applid(string previousUrl = "")
        {
            /*
            This code was added to hardcode IC for non-nci user to access file uploading/viewing page
            It was removed on request. This code can potentially be used in the future to hardcode
            access for non-nci employees (Replace "hindsrr" with user id of the user in question)

            var userId = Convert.ToString(this.Session["userid"]);
            if (userId == "hindsrr")
            {
                this.Session["ic"] = "NCI";
            }
            */

            eGrantsDocCreateViewModel eDocViewModel = await _documentService.DocCreateWithoutApplIdAsync(previousUrl, sessionInfo);

            return View("~/Views/Egrants/EgrantsDocCreate.cshtml", eDocViewModel);

        }


        [HttpPost]
        public async Task<ActionResult> doc_create_by_file(IFormFile file, int appl_id, int category_id, string sub_category, DateTime doc_date, string admin_code, int serial_num)
        {
            var result = await _documentService.DocCreateByFileAsync(file, appl_id, category_id, 
                sub_category, doc_date, admin_code, serial_num, sessionInfo);

            return Json(new { url = result.Url, message = result.Message });
        }


        // to create doc by file input
        /// <summary>
        /// The doc_create_pdf_by_file.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="category_id">
        /// The category_id.
        /// </param>
        /// <param name="sub_category">
        /// The sub_category.
        /// </param>
        /// <param name="doc_date">
        /// The doc_date.
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
        [HttpPost]
        public ActionResult doc_create_pdf_by_file(
            IEnumerable<IFormFile> files,
            int appl_id,
            int category_id,
            string sub_category,
            DateTime doc_date,
            string admin_code,
            int serial_num)
        {
            var docName = string.Empty;
            string url = null;
            string mssg = null;
            string fileExtension = string.Empty;
            var pdfDocs = new List<PdfDocument>();
            var converter = new EmailConcatenation.PdfConverter();

            if (files != null && files.Any())
            {
                try
                {
                    var unsupportedFilesList = _egrantsCommon.GetUnsupportedFileList(files);

                    foreach (var file in files)
                    {
                        // get file name and file Extension
                        var fileName = Path.GetFileName(file.FileName);
                        fileExtension = Path.GetExtension(fileName);

                        byte[] fileData;
                        using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                        {
                            fileData = binaryReader.ReadBytes((int)file.Length);
                        }

                        PdfDocument pdfResult = null;

                        if (fileExtension.Equals(".msg", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using (var memoryStream = new MemoryStream(fileData))
                            {
                                var emailFile = new Storage.Message(memoryStream);
                                pdfResult = converter.Convert(emailFile);
                            }
                        }
                        else
                        {
                            using (var memoryStream = new MemoryStream(fileData))
                            {
                                pdfResult = converter.Convert(memoryStream, file.FileName);
                            }
                        }

                        if (pdfResult != null)
                        {
                            pdfDocs.Add(pdfResult);
                        }
                    }
                    fileExtension = ".pdf";

                    var sb = new StringBuilder();
                    if (pdfDocs.Any())
                    {
                        // get document_id and creat a new docName
                        var document_id = _documentService.GetDocID(
                            appl_id,
                            category_id,
                            sub_category,
                            doc_date,
                            fileExtension,
                            Convert.ToString(sessionInfo.Ic),
                            Convert.ToString(sessionInfo.UserId));

                        docName = Convert.ToString(document_id) + fileExtension;

                        // upload to image sever 
#if DEBUG
                        var fileFolder = @"C:\PdfFileOutput\";
#else
                        var fileFolder = @"\\" + HttpContext.Session.GetString("WebGrantUrl") + "\\egrants\\funded2\\nci\\main\\";

#endif
                        // leave in place for now for local testing


                        var filePath = Path.Combine(fileFolder, docName);

                        var pdfDoc = PdfDocument.Merge(pdfDocs);
                        pdfDoc.SaveAs(filePath);

                        // create review url
                        this.ViewBag.FileUrl = sessionInfo.ImageServerUrl + HttpContext.Session.GetString("EgrantsDocNewRelativePath") + Convert.ToString(docName);
                        sb.Append("Done! New document has been created**#7|n3br3@k#**");
                    }
                    else
                    {
                        sb.Append("No documents were found to convert**#7|n3br3@k#**");
                    }


                    if (unsupportedFilesList.Count > 0)
                    {
                        sb.AppendLine("IMPORTANT! The following email attachments were not converted, please add them separately: **#h3@d3r#****#7|n3br3@k#**");
                        foreach (var unsupportedFile in unsupportedFilesList)
                        {
                            sb.AppendLine($"{unsupportedFile.Truncate(50)}**#7|n3br3@k#**");
                        }
                    }

                    url = this.ViewBag.FileUrl;
                    mssg = sb.ToString();
                }
                catch (Exception ex)
                {
                    mssg = "ERROR: The file could not be converted!";


                }
            }
            else
                mssg = "You have not specified a file.";

            return this.Json(new { url, message = mssg });
        }

        // to create doc by dragdrop
        /// <summary>
        /// The convert_to_pdf_by_ddrop.
        /// </summary>
        /// <param name="dropedfile">
        /// The dropedfile.
        /// </param>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="category_id">
        /// The category_id.
        /// </param>
        /// <param name="sub_category">
        /// The sub_category.
        /// </param>
        /// <param name="doc_date">
        /// The doc_date.
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
        [HttpPost]
        public ActionResult convert_to_pdf_by_ddrop(
            IEnumerable<IFormFile> dropedfiles,
            int appl_id,
            int category_id,
            string sub_category,
            DateTime doc_date,
            string admin_code,
            int serial_num)
        {

            var docName = string.Empty;
            string url = null;
            string mssg = null;
            string fileExtension = string.Empty;
            var pdfDocs = new List<PdfDocument>();
            var converter = new EmailConcatenation.PdfConverter();

            if (dropedfiles != null && dropedfiles.Any())
                try
                {
                    var unsupportedFilesList = _egrantsCommon.GetUnsupportedFileList(dropedfiles);

                    foreach (var dropedfile in dropedfiles)
                    {
                        // get file name and file Extension
                        var fileName = Path.GetFileName(dropedfile.FileName);
                        fileExtension = Path.GetExtension(fileName);

                        byte[] fileData;
                        using (var binaryReader = new BinaryReader(dropedfile.OpenReadStream()))
                        {
                            fileData = binaryReader.ReadBytes((int)dropedfile.Length);
                        }

                        PdfDocument pdfResult = null;
                        if (fileExtension.Equals(".msg", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using (var memoryStream = new MemoryStream(fileData))
                            {
                                var emailFile = new Storage.Message(memoryStream);
                                pdfResult = converter.Convert(emailFile);
                            }
                        }
                        else
                        {
                            using (var memoryStream = new MemoryStream(fileData))
                            {
                                pdfResult = converter.Convert(memoryStream, fileName);
                            }
                        }
                        if (pdfResult != null)
                        {
                            pdfDocs.Add(pdfResult);
                        }
                    }
                    fileExtension = ".pdf";

                    var sb = new StringBuilder();
                    if (pdfDocs.Any())
                    {
                        // get document_id and creat a new docName
                        var document_id = _documentService.GetDocID(
                            appl_id,
                            category_id,
                            sub_category,
                            doc_date,
                            fileExtension,
                            sessionInfo.Ic,
                            sessionInfo.UserId);

                        docName = Convert.ToString(document_id) + fileExtension;


                        var fileFolder = @"\\" + Convert.ToString(HttpContext.Session.GetString("WebGrantUrl")) + "\\egrants\\funded2\\nci\\main\\";

                        var filePath = Path.Combine(fileFolder, docName);

                        var pdfDoc = PdfDocument.Merge(pdfDocs);
                        pdfDoc.SaveAs(filePath);

                        // create review url
                        this.ViewBag.FileUrl = sessionInfo.ImageServerUrl + HttpContext.Session.GetString("EgrantsDocNewRelativePath")
                                                                                                + Convert.ToString(docName);
                        sb.Append("Done! New document has been created**#7|n3br3@k#**");
                    }
                    else
                    {
                        sb.Append("No documents were found to convert**#7|n3br3@k#**");
                    }

                    if (unsupportedFilesList.Count > 0)
                    {
                        sb.AppendLine("IMPORTANT! The following email attachments were not converted, please add them separately: **#h3@d3r#****#7|n3br3@k#**");
                        foreach (var unsupportedFile in unsupportedFilesList)
                        {
                            sb.AppendLine($"{unsupportedFile.Truncate(50)}**#7|n3br3@k#**");
                        }
                    }

                    url = this.ViewBag.FileUrl;
                    mssg = sb.ToString();
                }
                catch (Exception ex)
                {
                    mssg = "ERROR: The file could not be converted!";
                }
            else
                mssg = "You have not specified a file.";

            return this.Json(new { url, message = mssg });
        }

        // string full_grant_num, int appl_id, string full_grant_num, int appl_id, 
        /// <summary>
        /// The doc_upload_default.
        /// </summary>
        /// <param name="docId">
        /// The docId.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> doc_upload_default(int docId)
        {
            eGrantsDocUploadViewModel eDocViewModel = await _documentService.DocUploadDefaultAsync(docId);

            return View("~/Views/Egrants/EgrantsDocUpload.cshtml", eDocViewModel);
        }

        // to show doc upload modal default
        /// <summary>
        /// The doc_upload_modal.
        /// </summary>
        /// <param name="doc_id">
        /// The doc_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> doc_upload_modal(int doc_id)
        {
            this.ViewBag.DocId = doc_id;
            this.ViewBag.DocInfo = await _documentService.GetDocInfo(doc_id);

            return this.View("~/Views/Egrants/_Modal_Doc_Upload.cshtml");
        }

        /// <summary>
        /// Upload document by file.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="doc_id">The document ID.</param>
        /// <returns>JSON result with upload status.</returns>
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]        
        public async Task<IActionResult> doc_upload_by_file(IFormFile file, int doc_id)
        {
            var result = await _documentService.DocUploadByFileAsync(file, doc_id, sessionInfo);

            return Json(new { url = result.Url, message = result.Message });
        }

        // to upload doc by pdf file --added at 4/15/2019 FOR REFRESH AFTER UPLOAD
        /// <summary>
        /// The doc_upload_pdf_by_file.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="doc_id">
        /// The doc_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [OutputCache(NoStore = true)]
        [HttpPost]
        public ActionResult doc_upload_pdf_by_file(IEnumerable<IFormFile> files, int doc_id)
        {
            var docName = string.Empty;
            string url = null;
            string mssg = null;
            string fileExtension = string.Empty;
            var pdfDocs = new List<PdfDocument>();
            var converter = new EmailConcatenation.PdfConverter();

            if (files != null && files.Any())
                try
                {
                    var unsupportedFilesList = _egrantsCommon.GetUnsupportedFileList(files);

                    foreach (var file in files)
                    {
                        // get file name and file Extension
                        var fileName = Path.GetFileName(file.FileName);
                        fileExtension = Path.GetExtension(fileName);

                        byte[] fileData;
                        using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                        {
                            fileData = binaryReader.ReadBytes((int)file.Length);
                        }

                        PdfDocument pdfResult = null;

                        if (fileExtension.Equals(".msg", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using (var memoryStream = new MemoryStream(fileData))
                            {
                                var emailFile = new Storage.Message(memoryStream);
                                pdfResult = converter.Convert(emailFile);
                            }
                        }
                        else
                        {
                            using (var memoryStream = new MemoryStream(fileData))
                            {
                                pdfResult = converter.Convert(memoryStream, file.FileName);
                            }
                        }

                        if (pdfResult != null)
                        {
                            pdfDocs.Add(pdfResult);
                        }
                    }

                    fileExtension = ".pdf";

                    var sb = new StringBuilder();
                    if (pdfDocs.Any())
                    {
                        // update url for document
                        _documentService.DocModify(
                            "to_upload",
                            0,
                            0,
                            string.Empty,
                            string.Empty,
                            Convert.ToString(doc_id),
                            fileExtension,
                            sessionInfo.Ic,
                            sessionInfo.UserId);

                        // get document id and create new document name       
                        docName = Convert.ToString(doc_id) + fileExtension;

#if DEBUG
                        var fileFolder = @"C:\PdfFileOutput\";

#else
                        var fileFolder = @"\\" + HttpContext.Session.GetString("WebGrantUrl") + "\\egrants\\funded2\\nci\\main\\";

#endif

                        var filePath = Path.Combine(fileFolder, docName);

                        var pdfDoc = PdfDocument.Merge(pdfDocs);
                        pdfDoc.SaveAs(filePath);

                        // create review url
                        this.ViewBag.FileUrl = sessionInfo.ImageServerUrl + HttpContext.Session.GetString("EgrantsDocNewRelativePath") + Convert.ToString(docName);

                        sb.Append("Done! New document has been created**#7|n3br3@k#**");
                    }
                    else
                    {
                        sb.Append("No documents were found to convert**#7|n3br3@k#**");
                    }

                    if (unsupportedFilesList.Count > 0)
                    {
                        sb.AppendLine("IMPORTANT! The following email attachments were not converted, please add them separately: **#h3@d3r#****#7|n3br3@k#**");
                        foreach (var unsupportedFile in unsupportedFilesList)
                        {
                            sb.AppendLine($"{unsupportedFile.Truncate(50)}**#7|n3br3@k#**");
                        }
                    }

                    url = this.ViewBag.FileUrl;
                    mssg = sb.ToString();

                }
                catch (Exception ex)
                {
                    this.ViewBag.Message = "ERROR: The file could not be converted!";
                }
            else
                this.ViewBag.Message = "Error while uploading the files.";

            return this.Json(new { url, message = mssg });
        }

        // to create doc by dragdrop
        /// <summary>
        /// The doc_create_by_ddrop.
        /// </summary>
        /// <param name="dropedfile">
        /// The dropedfile.
        /// </param>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="category_id">
        /// The category_id.
        /// </param>
        /// <param name="sub_category">
        /// The sub_category.
        /// </param>
        /// <param name="doc_date">
        /// The doc_date.
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
        /// 
        /// 
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost]
        public async Task<ActionResult> doc_create_by_ddrop(IFormFile dropedfile, int appl_id, int category_id, string sub_category, DateTime doc_date, string admin_code, int serial_num)
        {
            var result = await _documentService.DocCreateByDdropAsync(dropedfile, appl_id, category_id, sub_category, doc_date, admin_code, serial_num, sessionInfo);
            
            return Json(new { url = result.Url, message = result.Message });
        }

        // to upload pdf docs by dragdrop
        /// <summary>
        /// The doc_upload_by_ddrop.
        /// </summary>
        /// <param name="dropedfile">
        /// The dropedfile.
        /// </param>
        /// <param name="docId">
        /// The doc_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost]
        public async Task<ActionResult> doc_upload_by_ddrop(IFormFile dropedfile, int docId)
        {
            var result = await _documentService.DocUploadByDdropAsync(dropedfile, docId, sessionInfo);

            return Json(new { url = result.Url, message = result.Message });
        }

        // to upload pdf docs by dragdrop
        /// <summary>
        /// The doc_upload_by_ddrop.
        /// </summary>
        /// <param name="dropedfile">
        /// The dropedfile.
        /// </param>
        /// <param name="doc_id">
        /// The doc_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost]
        public async Task<ActionResult> doc_upload_pdf_by_ddrop(IEnumerable<IFormFile> dropedfiles, int doc_id)
        {
            var docName = string.Empty;
            string url = null;
            string mssg = null;
            string fileExtension = string.Empty;
            var pdfDocs = new List<PdfDocument>();
            var converter = new EmailConcatenation.PdfConverter();

            if (dropedfiles != null && dropedfiles.Any())
                try
                {
                    var unsupportedFilesList = _egrantsCommon.GetUnsupportedFileList(dropedfiles);

                    foreach (var dropedfile in dropedfiles)
                    {

                        // get file name and file Extension
                        var fileName = Path.GetFileName(dropedfile.FileName);
                        fileExtension = Path.GetExtension(fileName);

                        byte[] fileData;
                        using (var binaryReader = new BinaryReader(dropedfile.OpenReadStream()))
                        {
                            fileData = binaryReader.ReadBytes((int)dropedfile.Length);
                        }

                        PdfDocument pdfResult = null;

                        if (fileExtension.Equals(".msg", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using (var memoryStream = new MemoryStream(fileData))
                            {
                                var emailFile = new Storage.Message(memoryStream);
                                pdfResult = converter.Convert(emailFile);
                            }
                        }
                        else
                        {
                            using (var memoryStream = new MemoryStream(fileData))
                            {
                                pdfResult = converter.Convert(memoryStream, dropedfile.FileName);
                            }
                        }

                        if (pdfResult != null)
                        {
                            pdfDocs.Add(pdfResult);
                        }
                    }

                    fileExtension = ".pdf";

                    var sb = new StringBuilder();
                    if (pdfDocs.Any())
                    {

                        // get document id and create new document name       
                        docName = Convert.ToString(doc_id) + fileExtension;

                        // update url for document
                        _documentService.DocModify(
                            "to_upload",
                            0,
                            0,
                            string.Empty,
                            string.Empty,
                            Convert.ToString(doc_id),
                            fileExtension,
                            sessionInfo.Ic,
                            sessionInfo.UserId);

                        var fileFolder = @"\\" + Convert.ToString(HttpContext.Session.GetString("WebGrantUrl")) + "\\egrants\\funded\\nci\\modify\\";

                        var filePath = Path.Combine(fileFolder, docName);

                        var pdfDoc = PdfDocument.Merge(pdfDocs);
                        pdfDoc.SaveAs(filePath);

                        // create review url
                        this.ViewBag.FileUrl = sessionInfo.ImageServerUrl + Convert.ToString(HttpContext.Session.GetString("EgrantsDocModifyRelativePath"))
                                                                                                + Convert.ToString(docName);
                        sb.Append("Done! New document has been created**#7|n3br3@k#**");
                    }
                    else
                    {
                        sb.Append("No documents were found to convert**#7|n3br3@k#**");
                    }

                    if (unsupportedFilesList.Count > 0)
                    {
                        sb.AppendLine("IMPORTANT! The following email attachments were not converted, please add them separately: **#h3@d3r#****#7|n3br3@k#**");
                        foreach (var unsupportedFile in unsupportedFilesList)
                        {
                            sb.AppendLine($"{unsupportedFile.Truncate(50)}**#7|n3br3@k#**");
                        }
                    }

                    url = this.ViewBag.FileUrl;
                    mssg = sb.ToString();
                }
                catch (Exception ex)
                {
                    this.ViewBag.Message = "ERROR: The file could not be converted!";
                }
            else
                this.ViewBag.Message = "Error while uploading the files.";

            return this.Json(new { url, message = mssg });
        }

        // to update document index for normal documents
        /// <summary>
        /// The doc_index_update_default.
        /// </summary>
        /// <param name="documentId">
        /// The document_id.
        /// </param>
        /// <param name="previousUrl">
        /// The previous_url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> doc_index_update_default(int documentId, string previousUrl)
        {
            eGrantsDocUpdateViewModel eDocViewModel = await _documentService.DocUpdateDefaultAsync(documentId, previousUrl, sessionInfo);

            return View("~/Views/Egrants/EgrantsDocUpdate.cshtml", eDocViewModel);
        }

        /// <summary>
        /// The doc_index_modify.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="applId">
        /// The appl_id.
        /// </param>
        /// <param name="document_id">
        /// The document_id.
        /// </param>
        /// <param name="category_id">
        /// The category_id.
        /// </param>
        /// <param name="sub_category">
        /// The sub_category.
        /// </param>
        /// <param name="document_date">
        /// The document_date.
        /// </param>
        /// <param name="previous_url">
        /// The previous_url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <summary>
        /// The doc_index_modify.
        /// </summary>
        /// <param name="act">The act.</param>
        /// <param name="appl_id">The appl_id.</param>
        /// <param name="document_id">The document_id.</param>
        /// <param name="category_id">The category_id.</param>
        /// <param name="sub_category">The sub_category.</param>
        /// <param name="document_date">The document_date.</param>
        /// <param name="previous_url">The previous_url.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public async Task<ActionResult> doc_index_modify(string act = "", int appl_id = 0, int document_id = 0, 
            int category_id = 0, string sub_category = "", string document_date = "", string previous_url = "")
        {
            var docids = Convert.ToString(document_id);

            await _documentService.DocIndexModifyAsync(act, appl_id, category_id, sub_category, document_date, docids, sessionInfo);

            return Redirect(previous_url);
        }

        // to modify document index for unidentified documrnt
        /// <summary>
        /// The unidentified_doc_modify.
        /// </summary>
        /// <param name="document_id">
        /// The document_id.
        /// </param>
        /// <param name="category_id">
        /// The category_id.
        /// </param>
        /// <param name="document_date">
        /// The document_date.
        /// </param>
        /// <param name="previous_url">
        /// The previous_url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> unidentified_doc_modify(int document_id, int category_id, string document_date, string previous_url)
        {
            eGrantsDocUpdateViewModel eDocViewModel = new eGrantsDocUpdateViewModel();
            eDocViewModel.DocId = document_id;
            eDocViewModel.CategoryId = (short?)category_id;
            eDocViewModel.DocDate = document_date;
            eDocViewModel.PreviousUrl = previous_url;

            eDocViewModel.AdminCodeList = await _commonService.LoadAdminCodes();
            eDocViewModel.CategoryList = await _documentService.LoadCategories(sessionInfo.Ic); // load categories that could only be upload
            eDocViewModel.MaxCategoryId = await _documentService.GetMaxCategoryid(sessionInfo.Ic);
            eDocViewModel.SubCategoryList = await _documentService.LoadSubCategoryList();

            return this.View("~/Views/Egrants/EgrantsDocUpdate.cshtml", eDocViewModel);
        }

        //// public ActionResult doc_index_modify(string act, int appl_id, int document_id, int category_id, string sub_category, string document_date, int specialist_id)
        //// {
        //// ViewBag.Status = "Done";
        //// ViewBag.applid = appl_id;
        //// string docids = Convert.ToString(document_id);
        //// EgrantsDoc.doc_modify(act, appl_id, category_id, sub_category, document_date, docids, "", specialist_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

        //// return RedirectToAction("by_appl", "Egrants", new { appl_id = ViewBag.applid, mode="qc" });
        //// }

        /// <summary>
        /// The appl_create_default.
        /// </summary>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> appl_create_default(string admin_code, int serial_num)
        {
            ViewBag.admincode = admin_code;
            ViewBag.serialnum = serial_num;

            ViewBag.AdminCodeList = _commonService.LoadAdminCodes();
            ViewBag.ApplTypeList = await _applService.LoadApplTypeAsync();
            ViewBag.ActivityCodeList = await _applService.LoadActivityCodeAsync(admin_code);
            ViewBag.GrantYearList = await _applService.LoadApplsBySerialNumAsync(admin_code, serial_num);

            return this.View("~/Views/eGrants/EgrantsApplCreate.cshtml");
        }

        /// <summary>
        /// The create_new_appl.
        /// </summary>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <param name="appl_type">
        /// The appl_type.
        /// </param>
        /// <param name="activity_code">
        /// The activity_code.
        /// </param>
        /// <param name="support_year">
        /// The support_year.
        /// </param>
        /// <param name="suffix_code">
        /// The suffix_code.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> create_new_appl(
            string admin_code,
            int serial_num,
            int appl_type,
            string activity_code,
            int support_year,
            string suffix_code = "")
        {
            this.ViewBag.admincode = admin_code;
            this.ViewBag.serialnum = serial_num;

            this.ViewBag.AdminCodeList = _commonService.LoadAdminCodes();
            ViewBag.ApplTypeList = await _applService.LoadApplTypeAsync();
            ViewBag.ActivityCodeList = await _applService.LoadActivityCodeAsync(admin_code);

            this.ViewBag.Message = await _applService.CreateNewAppl(
                admin_code,
                serial_num,
                appl_type,
                activity_code,
                support_year,
                suffix_code,
                sessionInfo.Ic,
                sessionInfo.UserId);

            this.ViewBag.GrantYearList = await _applService.LoadApplsBySerialNumAsync(admin_code, serial_num);

            return this.View("~/Views/Egrants/EgrantsApplCreate.cshtml");
        }

        //// show attachments docs
        ///// <summary>
        ///// The doc_attachments.
        ///// </summary>
        ///// <param name="document_id">
        ///// The document_id.
        ///// </param>
        ///// <returns>
        ///// The <see cref="ActionResult"/>.
        ///// </returns>
        //public ActionResult doc_attachments(int document_id)
        //{
        //    this.ViewBag.ImageServer = Convert.ToString(this.Session["ImageServerUrl"]);
        //    this.ViewBag.Attachments = EgrantsDoc.LoadDocAttachments(document_id);

        //    return this.View("~/Egrants/Views/_Modal_Doc_Attachments.cshtml");
        //}

        // show impac doc FRS or Closeout Notification
        /// <summary>
        /// The impac_docs.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> impac_docs(string act, int appl_id)
        {
            ViewBag.ImpacDocs = await _eGrantsService.LoadImpacDocs(act, appl_id);
            ViewBag.act = act;
            ViewBag.appl_id = appl_id;

            return this.View("~/Views/Egrants/_Modal_Impac_Docs.cshtml");
        }

        // show Closeout Notification
        /// <summary>
        /// The closeout_notif.
        /// </summary>
        /// <param name="applid">
        /// The applid.
        /// </param>
        /// <param name="notifName">
        /// The notif name.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult closeout_notif(string applid, string notifName)
        {
            ViewBag.notification = _documentService.GetCloseoutNotificationAsync(applid, notifName, sessionInfo);
            ViewBag.applid = applid;

            return this.View("~/Views/Egrants/CloseoutNotif.cshtml");
        }
    }
}