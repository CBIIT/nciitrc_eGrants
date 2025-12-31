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

        public EgrantsDocController(IeGrantsService eGrantsService, ICommonService commonService, IDocumentService documentService, ISessionInfoService sessionInfoService, IConfiguration configuration = null, EgrantsCommon egrantsCommon = null, IApplService applService = null)
        {
            _eGrantsService = eGrantsService;
            _commonService = commonService;
            _sessionInfoService = sessionInfoService;
            _documentService = documentService;
            _configuration = configuration;
            _egrantsCommon = egrantsCommon;
            _applService = applService;
        }

        //// GET: Egrants
        ///// <summary>
        ///// The report error index.
        ///// </summary>
        ///// <param name="document_id">
        ///// The document_id.
        ///// </param>
        ///// <returns>
        ///// The <see cref="ActionResult"/>.
        ///// </returns>
        //public ActionResult ReportErrorIndex(int document_id)
        //{
        //    this.ViewBag.DocID = document_id;

        //    return this.View("~/Egrants/Views/_Modal_Report_Error.cshtml");
        //}

        ///// <summary>
        ///// The report error.
        ///// </summary>
        ///// <param name="errormsg">
        ///// The errormsg.
        ///// </param>
        ///// <param name="document_id">
        ///// The document_id.
        ///// </param>
        ///// <param name="currenturl">
        ///// The currenturl.
        ///// </param>
        ///// <returns>
        ///// The <see cref="ActionResult"/>.
        ///// </returns>
        //public ActionResult ReportError(string errormsg, int document_id, string currenturl)
        //{
        //    this.ViewBag.DocID = document_id;
        //    this.ViewBag.Errormsg = errormsg;
        //    EgrantsDoc.report_doc_error(errormsg, document_id, Convert.ToString(this.Session["ic"]), Convert.ToString(this.Session["userid"]));

        //    return this.Redirect(currenturl);
        //}


        // show era doc
        /// <summary>
        /// The show_era_doc.
        /// </summary>
        /// <param name="docurl">
        /// The docurl.
        /// </param>
        /// <returns>
        /// The <see cref="RedirectResult"/>.
        /// </returns>
        //public async Task<RedirectResult> show_era_doc(string docurl)
        //{
        //    var certUrl = _configuration["AppSettings:certPath"];

        //    // this value should be kept as a secret 
        //    var certPass = _configuration["AppSettings:certPass"];

        //    var certificate = new X509Certificate2(certUrl, certPass);

        //    var handler = new HttpClientHandler();
        //    handler.ClientCertificates.Add(certificate);
        //    handler.AllowAutoRedirect = false; // same as your current code

        //    using var client = new HttpClient(handler);
        //    var response = await client.GetAsync(docurl);

        //    response.EnsureSuccessStatusCode();

        //    var tempLink = await response.Content.ReadAsStringAsync();

        //    return Redirect(tempLink);
        //}

        /// <summary>
        /// Show ERA document by retrieving temporary download link
        /// </summary>
        /// <param name="docurl">The document URL</param>
        /// <returns>Redirect to temporary download link or error view</returns>
        public async Task<IActionResult> show_era_doc(string docurl)
        {
            try
            {

                var certUrl = _configuration["AppSettings:certPath"];
                var certPass = _configuration["AppSettings:certPass"];

                if (string.IsNullOrEmpty(certUrl) || !System.IO.File.Exists(certUrl))
                {
                    Log.Error("Certificate not found at path: {CertPath}", certUrl);
                }

                var certificate = new X509Certificate2(certUrl, certPass);

                var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = false, // Prevent automatic redirects
                    ClientCertificateOptions = ClientCertificateOption.Manual
                };
                handler.ClientCertificates.Add(certificate);

                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("User-Agent", "eGrants");
                client.Timeout = TimeSpan.FromSeconds(30);

                Log.Information("Requesting ERA document: {DocUrl}", docurl);

                var response = await client.GetAsync(docurl);

                // Log response details
                Log.Information("ERA response status: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Log.Error("ERA request failed. Status: {Status}, Content: {Content}",
                        response.StatusCode, errorContent.Substring(0, Math.Min(200, errorContent.Length)));
                    return StatusCode((int)response.StatusCode, "Failed to retrieve document");
                }

                var tempLink = await response.Content.ReadAsStringAsync();
                tempLink = tempLink?.Trim();

                Log.Information("Redirecting to temporary link: {TempLink}", tempLink);

                return Redirect(tempLink);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error in show_era_doc for URL: {DocUrl}", docurl);
                throw;
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

        ///// <summary>
        ///// The process supplement doc.
        ///// </summary>
        ///// <param name="act">
        ///// The act.
        ///// </param>
        ///// <param name="grant_id">
        ///// The grant_id.
        ///// </param>
        ///// <param name="support_year">
        ///// The support_year.
        ///// </param>
        ///// <param name="suffix_code">
        ///// The suffix_code.
        ///// </param>
        ///// <param name="former_applid">
        ///// The former_applid.
        ///// </param>
        ///// <param name="docid_str">
        ///// The docid_str.
        ///// </param>
        ///// <returns>
        ///// The <see cref="ActionResult"/>.
        ///// </returns>
        //public ActionResult ProcessSupplementDoc(string act, int grant_id, int support_year, string suffix_code, int former_applid, string docid_str)
        //{
        //    this.ViewBag.Status = "Done";
        //    this.ViewBag.GrantID = grant_id;
        //    this.ViewBag.FormerAppls = EgrantsDoc.LoadFormerAppls(grant_id);

        //    this.ViewBag.Supplement = EgrantsDoc.LoadSupplement(
        //        act,
        //        grant_id,
        //        support_year,
        //        suffix_code,
        //        former_applid,
        //        docid_str,
        //        Convert.ToString(this.Session["ic"]),
        //        Convert.ToString(this.Session["userid"]));

        //    return this.View("~/Egrants/Views/_Modal_Supplement.cshtml");
        //}

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

        //// to show doc upload modal default
        ///// <summary>
        ///// The doc_upload_modal.
        ///// </summary>
        ///// <param name="doc_id">
        ///// The doc_id.
        ///// </param>
        ///// <returns>
        ///// The <see cref="ActionResult"/>.
        ///// </returns>
        //public ActionResult doc_upload_modal(int doc_id)
        //{
        //    this.ViewBag.DocId = doc_id;
        //    this.ViewBag.DocInfo = EgrantsDoc.GetDocInfo(doc_id);

        //    return this.View("~/Egrants/Views/_Modal_Doc_Upload.cshtml");
        //}

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

        //// to upload doc by pdf file --added at 4/15/2019 FOR REFRESH AFTER UPLOAD
        ///// <summary>
        ///// The doc_upload_pdf_by_file.
        ///// </summary>
        ///// <param name="file">
        ///// The file.
        ///// </param>
        ///// <param name="doc_id">
        ///// The doc_id.
        ///// </param>
        ///// <returns>
        ///// The <see cref="ActionResult"/>.
        ///// </returns>
        //[OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        //[HttpPost]
        //public ActionResult doc_upload_pdf_by_file(IEnumerable<HttpPostedFileBase> files, int doc_id)
        //{
        //    var docName = string.Empty;
        //    string url = null;
        //    string mssg = null;
        //    string fileExtension = string.Empty;
        //    var pdfDocs = new List<PdfDocument>();
        //    var converter = new EmailConcatenation.PdfConverter();

        //    if (files != null && files.Any())
        //        try
        //        {
        //            var unsupportedFilesList = EgrantsCommon.GetUnsupportedFileList(files);

        //            foreach (var file in files)
        //            {
        //                // get file name and file Extension
        //                var fileName = Path.GetFileName(file.FileName);
        //                fileExtension = Path.GetExtension(fileName);

        //                byte[] fileData;
        //                using (var binaryReader = new BinaryReader(file.InputStream))
        //                {
        //                    fileData = binaryReader.ReadBytes(file.ContentLength);
        //                }

        //                PdfDocument pdfResult = null;

        //                if (fileExtension.Equals(".msg", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    using (var memoryStream = new MemoryStream(fileData))
        //                    {
        //                        var emailFile = new Storage.Message(memoryStream);
        //                        pdfResult = converter.Convert(emailFile);
        //                    }
        //                }
        //                else
        //                {
        //                    using (var memoryStream = new MemoryStream(fileData))
        //                    {
        //                        pdfResult = converter.Convert(memoryStream, file.FileName);
        //                    }
        //                }

        //                if (pdfResult != null)
        //                {
        //                    pdfDocs.Add(pdfResult);
        //                }
        //            }

        //            fileExtension = ".pdf";

        //            var sb = new StringBuilder();
        //            if (pdfDocs.Any())
        //            {
        //                // update url for document
        //                EgrantsDoc.doc_modify(
        //                    "to_upload",
        //                    0,
        //                    0,
        //                    string.Empty,
        //                    string.Empty,
        //                    Convert.ToString(doc_id),
        //                    fileExtension,
        //                    Convert.ToString(this.Session["ic"]),
        //                    Convert.ToString(this.Session["userid"]));

        //                // get document id and create new document name       
        //                docName = Convert.ToString(doc_id) + fileExtension;


        //                var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded\\nci\\modify\\";

        //                var filePath = Path.Combine(fileFolder, docName);

        //                var pdfDoc = PdfDocument.Merge(pdfDocs);
        //                pdfDoc.SaveAs(filePath);

        //                // create review url
        //                this.ViewBag.FileUrl = Convert.ToString(this.Session["ImageServerUrl"]) + Convert.ToString(this.Session["EgrantsDocModifyRelativePath"])
        //                                                                                        + Convert.ToString(docName);
        //                sb.Append("Done! New document has been created**#7|n3br3@k#**");
        //            }
        //            else
        //            {
        //                sb.Append("No documents were found to convert**#7|n3br3@k#**");
        //            }

        //            if (unsupportedFilesList.Count > 0)
        //            {
        //                sb.AppendLine("IMPORTANT! The following email attachments were not converted, please add them separately: **#h3@d3r#****#7|n3br3@k#**");
        //                foreach (var unsupportedFile in unsupportedFilesList)
        //                {
        //                    sb.AppendLine($"{unsupportedFile.Truncate(50)}**#7|n3br3@k#**");
        //                }
        //            }

        //            url = this.ViewBag.FileUrl;
        //            mssg = sb.ToString();

        //        }
        //        catch (Exception ex)
        //        {
        //            this.ViewBag.Message = "ERROR: The file could not be converted!";
        //        }
        //    else
        //        this.ViewBag.Message = "Error while uploading the files.";

        //    return this.Json(new { url, message = mssg });
        //}

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
        public async Task<ActionResult> doc_index_modify(string act, int appl_id, int document_id, int category_id, string sub_category, string document_date, string previous_url)
        {
            var docids = Convert.ToString(document_id);

            await _documentService.DocIndexModifyAsync(act, appl_id, category_id, sub_category, document_date, docids, sessionInfo);

            return Redirect(previous_url);
        }

        //// to modify document index for unidentified documrnt
        ///// <summary>
        ///// The unidentified_doc_modify.
        ///// </summary>
        ///// <param name="document_id">
        ///// The document_id.
        ///// </param>
        ///// <param name="category_id">
        ///// The category_id.
        ///// </param>
        ///// <param name="document_date">
        ///// The document_date.
        ///// </param>
        ///// <param name="previous_url">
        ///// The previous_url.
        ///// </param>
        ///// <returns>
        ///// The <see cref="ActionResult"/>.
        ///// </returns>
        //public ActionResult unidentified_doc_modify(int document_id, int category_id, string document_date, string previous_url)
        //{
        //    this.ViewBag.docid = document_id;
        //    this.ViewBag.categoryid = category_id;
        //    this.ViewBag.docdate = document_date;
        //    this.ViewBag.Previousurl = previous_url;

        //    this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
        //    this.ViewBag.CategoryList = EgrantsDoc.LoadCategories(Convert.ToString(this.Session["ic"]));
        //    this.ViewBag.MaxCategoryid = EgrantsDoc.GetMaxCategoryid(Convert.ToString(this.Session["ic"]));
        //    this.ViewBag.SubCategoryList = EgrantsDoc.LoadSubCategoryList();

        //    return this.View("~/Egrants/Views/egrantsDocUpdate.cshtml");
        //}

        //// public ActionResult doc_index_modify(string act, int appl_id, int document_id, int category_id, string sub_category, string document_date, int specialist_id)
        //// {
        //// ViewBag.Status = "Done";
        //// ViewBag.applid = appl_id;
        //// string docids = Convert.ToString(document_id);
        //// EgrantsDoc.doc_modify(act, appl_id, category_id, sub_category, document_date, docids, "", specialist_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

        //// return RedirectToAction("by_appl", "Egrants", new { appl_id = ViewBag.applid, mode="qc" });
        //// }

        /// <summary>
        /// Display the application creation page
        /// </summary>
        /// <param name="adminCode">The admin code</param>
        /// <param name="serialNum">The serial number</param>
        /// <returns>The application creation view</returns>
        [HttpGet]
        public async Task<ActionResult> ApplCreateDefault(string adminCode, int serialNum)
        {
            ViewBag.admincode = adminCode;
            ViewBag.serialnum = serialNum;
            ViewBag.AdminCodeList = await _commonService.LoadAdminCodes();
            ViewBag.ApplTypeList = await _applService.LoadApplTypeAsync();
            ViewBag.ActivityCodeList = await _applService.LoadActivityCodeAsync(adminCode);
            ViewBag.GrantYearList = await _applService.LoadApplsBySerialNumAsync(adminCode, serialNum);

            return View("~/Views/Egrants/EgrantsApplCreate.cshtml");
        }

        ///// <summary>
        ///// The create_new_appl.
        ///// </summary>
        ///// <param name="admin_code">
        ///// The admin_code.
        ///// </param>
        ///// <param name="serial_num">
        ///// The serial_num.
        ///// </param>
        ///// <param name="appl_type">
        ///// The appl_type.
        ///// </param>
        ///// <param name="activity_code">
        ///// The activity_code.
        ///// </param>
        ///// <param name="support_year">
        ///// The support_year.
        ///// </param>
        ///// <param name="suffix_code">
        ///// The suffix_code.
        ///// </param>
        ///// <returns>
        ///// The <see cref="ActionResult"/>.
        ///// </returns>
        //public ActionResult create_new_appl(
        //    string admin_code,
        //    int serial_num,
        //    int appl_type,
        //    string activity_code,
        //    int support_year,
        //    string suffix_code)
        //{
        //    this.ViewBag.admincode = admin_code;
        //    this.ViewBag.serialnum = serial_num;

        //    this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
        //    this.ViewBag.ApplTypeList = EgrantsAppl.LoadApplType();
        //    this.ViewBag.ActivityCodeList = EgrantsAppl.LoadActivityCode(admin_code);

        //    this.ViewBag.Message = EgrantsAppl.CreateNewAppl(
        //        admin_code,
        //        serial_num,
        //        appl_type,
        //        activity_code,
        //        support_year,
        //        suffix_code,
        //        Convert.ToString(this.Session["ic"]),
        //        Convert.ToString(this.Session["userid"]));

        //    this.ViewBag.GrantYearList = EgrantsAppl.LoadApplsBySerialnum(admin_code, serial_num);

        //    return this.View("~/Egrants/Views/EgrantsApplCreate.cshtml");
        //}

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

        //// show Closeout Notification
        ///// <summary>
        ///// The closeout_notif.
        ///// </summary>
        ///// <param name="applid">
        ///// The applid.
        ///// </param>
        ///// <param name="notifName">
        ///// The notif name.
        ///// </param>
        ///// <returns>
        ///// The <see cref="ActionResult"/>.
        ///// </returns>
        //public ActionResult closeout_notif(string applid, string notifName)
        //{
        //    this.ViewBag.notification = EgrantsDoc.getCloseoutNotif(applid, notifName);
        //    this.ViewBag.applid = applid;

        //    return this.View("~/Egrants/Views/CloseoutNotif.cshtml");
        //}
    }
}