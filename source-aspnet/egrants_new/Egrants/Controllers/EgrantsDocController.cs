#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EgrantsDocController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-03-31
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
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using egrants_new.Models;
using Newtonsoft.Json;
using egrants_new.Functions;
using MsgReader.Outlook;
using static System.Net.WebRequestMethods;
using egrants_new.Integration.WebServices;
using static egrants_new.Egrants_Admin.Models.Supplement;
using IronPdf;
using System.Text;
using EmailConcatenation;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using WebGrease.Activities;

#endregion

namespace egrants_new.Controllers
{
    /// <summary>
    /// The egrants doc controller.
    /// </summary>
    public class EgrantsDocController : Controller
    {

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
            this.ViewBag.DocID = document_id;

            return this.View("~/Egrants/Views/_Modal_Report_Error.cshtml");
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
            EgrantsDoc.report_doc_error(errormsg, document_id, Convert.ToString(this.Session["ic"]), Convert.ToString(this.Session["userid"]));

            return this.Redirect(currenturl);
        }


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
        public RedirectResult show_era_doc(string docurl)
        {
            var cert_url = ConfigurationManager.ConnectionStrings["certPath"].ToString();
            var cert_pass = ConfigurationManager.ConnectionStrings["certPass"].ToString();
            var certificate = new X509Certificate2(cert_url, cert_pass);
            var uri = new Uri(docurl);
            var webreq = (HttpWebRequest)WebRequest.Create(uri);
            webreq.KeepAlive = false;
            webreq.Method = "GET";

            // webreq.Accept = "text/xml";
            webreq.AllowAutoRedirect = false;
            webreq.ClientCertificates.Add(certificate);
            HttpWebResponse webresp;
            webresp = (HttpWebResponse)webreq.GetResponse();

            // string responsecode = webresp.StatusCode.ToString(); 
            string tempLink;

            var postStream = webresp.GetResponseStream();

            using (var reader = new StreamReader(postStream))
            {
                tempLink = reader.ReadToEnd();
            }

            return this.Redirect(tempLink);
        }

        /// <summary>
        /// The load supplement doc.
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
        public ActionResult LoadSupplementDoc(string act, int grant_id)
        {
            this.ViewBag.Act = act;
            this.ViewBag.GrantID = grant_id;
            this.ViewBag.FormerAppls = EgrantsDoc.LoadFormerAppls(grant_id);

            this.ViewBag.Supplement = EgrantsDoc.LoadSupplement(
                act,
                grant_id,
                0,
                string.Empty,
                0,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants/Views/_Modal_Supplement.cshtml");
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
        public ActionResult ProcessSupplementDoc(string act, int grant_id, int support_year, string suffix_code, int former_applid, string docid_str)
        {
            this.ViewBag.Status = "Done";
            this.ViewBag.GrantID = grant_id;
            this.ViewBag.FormerAppls = EgrantsDoc.LoadFormerAppls(grant_id);

            this.ViewBag.Supplement = EgrantsDoc.LoadSupplement(
                act,
                grant_id,
                support_year,
                suffix_code,
                former_applid,
                docid_str,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants/Views/_Modal_Supplement.cshtml");
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
        public ActionResult LoadSupplement(string act, int grant_id)
        {
            this.ViewBag.Act = act;
            this.ViewBag.GrantID = grant_id;
            this.ViewBag.FormerAppls = EgrantsDoc.LoadFormerAppls(grant_id);

            this.ViewBag.Supplement = EgrantsDoc.LoadSupplement(
                act,
                grant_id,
                0,
                string.Empty,
                0,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants/Views/Supplement.cshtml");
        }

        /// <summary>
        /// The process supplement.
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
        public ActionResult ProcessSupplement(string act, int grant_id, int support_year, string suffix_code, int former_applid, string docid_str)
        {
            this.ViewBag.Status = "Done";
            this.ViewBag.GrantID = grant_id;
            this.ViewBag.FormerAppls = EgrantsDoc.LoadFormerAppls(grant_id);

            this.ViewBag.Supplement = EgrantsDoc.LoadSupplement(
                act,
                grant_id,
                support_year,
                suffix_code,
                former_applid,
                docid_str,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants/Views/Supplement.cshtml");
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
            this.ViewBag.Status = "Done";
            EgrantsDoc.doc_modify(act, 0, 0, string.Empty, string.Empty, docids, string.Empty, Convert.ToString(this.Session["ic"]), Convert.ToString(this.Session["userid"]));

            // return View("~/Egrants/Views/DocProcess.cshtml");
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
        public ActionResult doc_create_with_applid(
            string act,
            string admin_code,
            int serial_num,
            int appl_id = 0,
            int document_id = 0,
            int category_id = 0,
            string sub_category = null,
            string document_date = null,
            string previous_url = null)
        {
            var userId = Convert.ToString(this.Session["userid"]);
            if (userId == "hindsrr")
            {
                this.Session["ic"] = "NCI";
            }
            this.ViewBag.Act = "Add";
            this.ViewBag.admincode = admin_code;
            this.ViewBag.serialnum = serial_num;
            this.ViewBag.applid = appl_id;
            this.ViewBag.Previousurl = previous_url;
            this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            this.ViewBag.CategoryList = EgrantsDoc.LoadCategories(Convert.ToString(this.Session["ic"])); // load categories that could only be upload
            this.ViewBag.SubCategoryList = EgrantsDoc.LoadSubCategoryList();
            this.ViewBag.MaxCategoryid = EgrantsDoc.GetMaxCategoryid(Convert.ToString(this.Session["ic"]));

            return this.View("~/Egrants/Views/egrantsDocCreate.cshtml");
        }

        // create new doc without selected appl_id
        /// <summary>
        /// The doc_create_without_applid.
        /// </summary>
        /// <param name="previous_url">
        /// The previous_url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult doc_create_without_applid(string previous_url = null)
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
            this.ViewBag.Act = "Add";
            this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            this.ViewBag.CategoryList = EgrantsDoc.LoadCategories(Convert.ToString(this.Session["ic"])); // load categories that could only be upload
            this.ViewBag.SubCategoryList = EgrantsDoc.LoadSubCategoryList();
            this.ViewBag.MaxCategoryid = EgrantsDoc.GetMaxCategoryid(Convert.ToString(this.Session["ic"]));
            this.ViewBag.Previousurl = previous_url;

            return this.View("~/Egrants/Views/egrantsDocCreate.cshtml");
        }

        // to create doc by file input
        /// <summary>
        /// The doc_create_by_file.
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
        public ActionResult doc_create_by_file(
            HttpPostedFileBase file,
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

            if (file != null && file.ContentLength > 0)
                try
                {
                    // get file name and file Extension
                    var fileName = Path.GetFileName(file.FileName);
                    var fileExtension = Path.GetExtension(fileName);

                    // get document_id and creat a new docName
                    var document_id = EgrantsDoc.GetDocID(
                        appl_id,
                        category_id,
                        sub_category,
                        doc_date,
                        fileExtension,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]));

                    docName = Convert.ToString(document_id) + fileExtension;

                    // upload to image sever 
                    var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded2\\nci\\main\\";

                    var filePath = Path.Combine(fileFolder, docName);

                    file.SaveAs(filePath);

                    // create review url
                    this.ViewBag.FileUrl = Convert.ToString(this.Session["ImageServerUrl"]) + Convert.ToString(this.Session["EgrantsDocNewRelativePath"])
                                         + Convert.ToString(docName);

                    this.ViewBag.Message = "Done! New document has been created";

                    url = this.ViewBag.FileUrl;
                    mssg = this.ViewBag.Message;
                }
                catch (Exception ex)
                {
                    this.ViewBag.Message = "ERROR:" + ex.Message;
                }
            else
                this.ViewBag.Message = "You have not specified a file.";

            return this.Json(new { url, message = mssg });
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
            IEnumerable<HttpPostedFileBase> files,
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
                    foreach (var file in files)
                    {
                        // get file name and file Extension
                        var fileName = Path.GetFileName(file.FileName);
                        fileExtension = Path.GetExtension(fileName);

                        byte[] fileData;
                        using (var binaryReader = new BinaryReader(file.InputStream))
                        {
                            fileData = binaryReader.ReadBytes(file.ContentLength);
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

                    // get document_id and creat a new docName
                    var document_id = EgrantsDoc.GetDocID(
                        appl_id,
                        category_id,
                        sub_category,
                        doc_date,
                        fileExtension,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]));

                    docName = Convert.ToString(document_id) + fileExtension;

                    // upload to image sever 
                    var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded2\\nci\\main\\";

                    //fileFolder = "C:\\Users\\hooverrl\\Desktop\\NCI\\nciitrc_eGrants\\source-aspnet\\temp";

                    var filePath = Path.Combine(fileFolder, docName);

                    if (!pdfDocs.Any())
                    {
                        pdfDocs.Add(converter.CreateEmptyDocument());
                    }

                    var pdfDoc = PdfDocument.Merge(pdfDocs);
                    pdfDoc.SaveAs(filePath);

                    // create review url
                    this.ViewBag.FileUrl = Convert.ToString(this.Session["ImageServerUrl"]) + Convert.ToString(this.Session["EgrantsDocNewRelativePath"])
                                            + Convert.ToString(docName);

                    this.ViewBag.Message = "Done! New document has been created";

                    url = this.ViewBag.FileUrl;
                    mssg = this.ViewBag.Message;
                }
                catch (Exception ex)
                {
                    this.ViewBag.Message = "ERROR:" + ex.Message;

                    //Response.StatusCode = 500; //Write your own error code
                    //StringBuilder sb = new StringBuilder();
                    //sb.AppendLine($"exception type: {ex.GetType().Name}");
                    //sb.AppendLine($"exception message: {ex.Message}");
                    ////sb.AppendLine($"captured file path diagnostic after Path.Combine: {filePathDiangostic}");
                    //if (ex.InnerException != null)
                    //{
                    //    sb.AppendLine($"inner exception type: {ex.InnerException.GetType().Name}");
                    //    sb.AppendLine($"inner exception message: {ex.Message}");
                    //}
                    //Response.Write(sb.ToString());
                    //return null;
                }
            }
            else
                this.ViewBag.Message = "You have not specified a file.";

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
        [HttpPost]
        public ActionResult doc_create_by_ddrop(
            HttpPostedFileBase dropedfile,
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

            if (dropedfile != null && dropedfile.ContentLength > 0)
                try
                {
                    // get file name and file Extension
                    var fileName = Path.GetFileName(dropedfile.FileName);
                    var fileExtension = Path.GetExtension(fileName);

                    // get document_id and creat a new docName
                    var document_id = EgrantsDoc.GetDocID(
                        appl_id,
                        category_id,
                        sub_category,
                        doc_date,
                        fileExtension,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]));

                    docName = Convert.ToString(document_id) + fileExtension;


                    var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded2\\nci\\main\\";

                    var filePath = Path.Combine(fileFolder, docName);

                    dropedfile.SaveAs(filePath);

                    // create review url
                    this.ViewBag.FileUrl = Convert.ToString(this.Session["ImageServerUrl"]) + Convert.ToString(this.Session["EgrantsDocNewRelativePath"])
                                                                                            + Convert.ToString(docName);

                    this.ViewBag.Message = "Done! New document has been created";

                    url = this.ViewBag.FileUrl;
                    mssg = this.ViewBag.Message;
                }
                catch (Exception ex)
                {
                    this.ViewBag.Message = "ERROR:" + ex.Message;
                }
            else
                this.ViewBag.Message = "You have not specified a file.";

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
        [HttpPost]
        public ActionResult convert_to_pdf_by_ddrop(
            IEnumerable<HttpPostedFileBase> dropedfiles,
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
                    foreach (var dropedfile in dropedfiles)
                    {
                        // get file name and file Extension
                        var fileName = Path.GetFileName(dropedfile.FileName);
                        fileExtension = Path.GetExtension(fileName);

                        byte[] fileData;
                        using (var binaryReader = new BinaryReader(dropedfile.InputStream))
                        {
                            fileData = binaryReader.ReadBytes(dropedfile.ContentLength);
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

                    // get document_id and creat a new docName
                    var document_id = EgrantsDoc.GetDocID(
                        appl_id,
                        category_id,
                        sub_category,
                        doc_date,
                        fileExtension,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]));

                    docName = Convert.ToString(document_id) + fileExtension;


                    var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded2\\nci\\main\\";

                    var filePath = Path.Combine(fileFolder, docName);

                    if (!pdfDocs.Any())
                    {
                        pdfDocs.Add(converter.CreateEmptyDocument());
                    }

                    var pdfDoc = PdfDocument.Merge(pdfDocs);
                    pdfDoc.SaveAs(filePath);

                    // create review url
                    this.ViewBag.FileUrl = Convert.ToString(this.Session["ImageServerUrl"]) + Convert.ToString(this.Session["EgrantsDocNewRelativePath"])
                                                                                            + Convert.ToString(docName);
                    this.ViewBag.Message = "Done! New document has been created";

                    url = this.ViewBag.FileUrl;
                    mssg = this.ViewBag.Message;
                    
                }
                catch (Exception ex)
                {
                    this.ViewBag.Message = "ERROR:" + ex.Message;
                }
            else
                this.ViewBag.Message = "You have not specified a file.";

            return this.Json(new { url, message = mssg });
        }

        // string full_grant_num, int appl_id, string full_grant_num, int appl_id, 
        /// <summary>
        /// The doc_upload_default.
        /// </summary>
        /// <param name="doc_id">
        /// The doc_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult doc_upload_default(int doc_id)
        {
            // ViewBag.DocId = doc_id;
            // ViewBag.ApplId = appl_id;
            // ViewBag.DocName = doc_name;
            // ViewBag.DocDate = doc_date;
            // ViewBag.FullGrantNum = full_grant_num;
            var DocInfor = EgrantsDoc.GetDocInfo(doc_id);

            foreach (var doc in DocInfor)
            {
                this.ViewBag.DocId = doc.document_id;
                this.ViewBag.ApplId = doc.appl_id;
                this.ViewBag.DocName = doc.document_name;
                this.ViewBag.DocDate = doc.document_date;
                this.ViewBag.FullGrantNum = doc.full_grant_num;
            }

            return this.View("~/Egrants/Views/egrantsDocUpload.cshtml");
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
        public ActionResult doc_upload_modal(int doc_id)
        {
            this.ViewBag.DocId = doc_id;
            this.ViewBag.DocInfo = EgrantsDoc.GetDocInfo(doc_id);

            return this.View("~/Egrants/Views/_Modal_Doc_Upload.cshtml");
        }

        // to upload doc by file --added at 4/15/2019 FOR REFRESH AFTER UPLOAD
        /// <summary>
        /// The doc_upload_by_file.
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
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        [HttpPost]
        public ActionResult doc_upload_by_file(HttpPostedFileBase file, int doc_id)
        {
            var docName = string.Empty;
            string url = null;
            string mssg = null;

            if (file != null && file.ContentLength > 0)
                try
                {
                    // get file name and file Extension
                    var fileName = Path.GetFileName(file.FileName);
                    var fileExtension = Path.GetExtension(fileName);

                    // update url for document
                    EgrantsDoc.doc_modify(
                        "to_upload",
                        0,
                        0,
                        string.Empty,
                        string.Empty,
                        Convert.ToString(doc_id),
                        fileExtension,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]));

                    // get document id and create new document name       
                    docName = Convert.ToString(doc_id) + fileExtension;


                    var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded\\nci\\modify\\";

                    var filePath = Path.Combine(fileFolder, docName);
                    file.SaveAs(filePath);


                    // create review url
                    this.ViewBag.FileUrl = Convert.ToString(this.Session["ImageServerUrl"]) + Convert.ToString(this.Session["EgrantsDocModifyRelativePath"])
                                                                                         + Convert.ToString(docName);

                    this.ViewBag.Message = "Done! New document has been created";

                    // ViewBag.Message = "please waiting window refresh...";
                    url = this.ViewBag.FileUrl;
                    mssg = this.ViewBag.Message;
                }
                catch (Exception ex)
                {
                    this.ViewBag.Message = "ERROR:" + ex.Message;
                }
            else
                this.ViewBag.Message = "Error while uploading the files.";

            return this.Json(new { url, message = mssg });
        }

        // to upload doc by dragdrop---added at 4/15/2019 FOR REFRESH AFTER UPLOAD
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
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        [HttpPost]
        public ActionResult doc_upload_by_ddrop(HttpPostedFileBase dropedfile, int doc_id)
        {
            var docName = string.Empty;
            string url = null;
            string mssg = null;

            if (dropedfile != null && dropedfile.ContentLength > 0)
                try
                {
                    // get file name and file Extension
                    var fileName = Path.GetFileName(dropedfile.FileName);
                    var fileExtension = Path.GetExtension(fileName);

                    // get document id and create new document name       
                    docName = Convert.ToString(doc_id) + fileExtension;

                    // update url for document
                    EgrantsDoc.doc_modify(
                        "to_upload",
                        0,
                        0,
                        string.Empty,
                        string.Empty,
                        Convert.ToString(doc_id),
                        fileExtension,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]));


                    var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded\\nci\\modify\\";

                    var filePath = Path.Combine(fileFolder, docName);
                    dropedfile.SaveAs(filePath);


                    // create review url
                    this.ViewBag.FileUrl = Convert.ToString(this.Session["ImageServerUrl"]) + Convert.ToString(this.Session["EgrantsDocModifyRelativePath"])
                                                                                            + Convert.ToString(docName);

                    this.ViewBag.Message = "Done! New document has been created";

                    // ViewBag.Message = "please waiting window refresh...";
                    url = this.ViewBag.FileUrl;
                    mssg = this.ViewBag.Message;
                }
                catch (Exception ex)
                {
                    this.ViewBag.Message = "ERROR:" + ex.Message;
                }
            else
                this.ViewBag.Message = "Error while uploading the files.";

            return this.Json(new { url, message = mssg });
        }

        // public ActionResult impac_docs(string act, int appl_id)
        // {
        //     this.ViewBag.ImpacDocs = EgrantsDoc.LoadImpacDocs(act, appl_id);
        //     this.ViewBag.act = act;
        //     this.ViewBag.appl_id = appl_id;
        //
        //     return this.View("~/Egrants/Views/_Modal_Impac_Docs.cshtml");
        // }


        // to update document index for normal documents
        /// <summary>
        /// The doc_index_update_default.
        /// </summary>
        /// <param name="document_id">
        /// The document_id.
        /// </param>
        /// <param name="previous_url">
        /// The previous_url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult doc_index_update_default(int document_id, string previous_url)
        {
            var DocInfor = EgrantsDoc.GetDocInfo(document_id);

            foreach (var doc in DocInfor)
            {
                this.ViewBag.Act = "Update";
                this.ViewBag.admincode = doc.admin_phs_org_code;
                this.ViewBag.serialnum = doc.serial_num;
                this.ViewBag.applid = doc.appl_id;
                this.ViewBag.docid = doc.document_id;
                this.ViewBag.categoryid = doc.category_id;
                this.ViewBag.subcategory = doc.sub_category_name;
                this.ViewBag.docdate = doc.document_date;
                this.ViewBag.Previousurl = previous_url;
                this.ViewBag.Status = "default";
            }

            int appl_id = Convert.ToInt32(this.ViewBag.applid);
            this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            this.ViewBag.CategoryList = EgrantsDoc.LoadCategories(Convert.ToString(this.Session["ic"]));
            this.ViewBag.MaxCategoryid = EgrantsDoc.GetMaxCategoryid(Convert.ToString(this.Session["ic"]));
            this.ViewBag.SubCategoryList = EgrantsDoc.LoadSubCategoryList();

            this.ViewBag.GrantYearList = EgrantsAppl.LoadApplsByApplid(appl_id);

            // ViewBag.UserList = EgrantsDoc.LoadUsers(Convert.ToString(Session["ic"]));
            return this.View("~/Egrants/Views/egrantsDocUpdate.cshtml");
        }

        /// <summary>
        /// The doc_index_modify.
        /// </summary>
        /// <param name="act">
        /// The act.
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
        public ActionResult doc_index_modify(
            string act,
            int appl_id,
            int document_id,
            int category_id,
            string sub_category,
            string document_date,
            string previous_url)
        {
            this.ViewBag.Status = "Done";
            this.ViewBag.applid = appl_id;
            this.ViewBag.Previousurl = previous_url;
            var docids = Convert.ToString(document_id);

            EgrantsDoc.doc_modify(
                act,
                appl_id,
                category_id,
                sub_category,
                document_date,
                docids,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.Redirect(previous_url);
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
        public ActionResult unidentified_doc_modify(int document_id, int category_id, string document_date, string previous_url)
        {
            this.ViewBag.docid = document_id;
            this.ViewBag.categoryid = category_id;
            this.ViewBag.docdate = document_date;
            this.ViewBag.Previousurl = previous_url;

            this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            this.ViewBag.CategoryList = EgrantsDoc.LoadCategories(Convert.ToString(this.Session["ic"]));
            this.ViewBag.MaxCategoryid = EgrantsDoc.GetMaxCategoryid(Convert.ToString(this.Session["ic"]));
            this.ViewBag.SubCategoryList = EgrantsDoc.LoadSubCategoryList();

            return this.View("~/Egrants/Views/egrantsDocUpdate.cshtml");
        }

        // public ActionResult doc_index_modify(string act, int appl_id, int document_id, int category_id, string sub_category, string document_date, int specialist_id)
        // {
        // ViewBag.Status = "Done";
        // ViewBag.applid = appl_id;
        // string docids = Convert.ToString(document_id);
        // EgrantsDoc.doc_modify(act, appl_id, category_id, sub_category, document_date, docids, "", specialist_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

        // return RedirectToAction("by_appl", "Egrants", new { appl_id = ViewBag.applid, mode="qc" });
        // }

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
        public ActionResult appl_create_default(string admin_code, int serial_num)
        {
            this.ViewBag.admincode = admin_code;
            this.ViewBag.serialnum = serial_num;

            this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            this.ViewBag.ApplTypeList = EgrantsAppl.LoadApplType();
            this.ViewBag.ActivityCodeList = EgrantsAppl.LoadActivityCode(admin_code);
            this.ViewBag.GrantYearList = EgrantsAppl.LoadApplsBySerialnum(admin_code, serial_num);

            return this.View("~/Egrants/Views/EgrantsApplCreate.cshtml");
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
        public ActionResult create_new_appl(
            string admin_code,
            int serial_num,
            int appl_type,
            string activity_code,
            int support_year,
            string suffix_code)
        {
            this.ViewBag.admincode = admin_code;
            this.ViewBag.serialnum = serial_num;

            this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            this.ViewBag.ApplTypeList = EgrantsAppl.LoadApplType();
            this.ViewBag.ActivityCodeList = EgrantsAppl.LoadActivityCode(admin_code);

            this.ViewBag.Message = EgrantsAppl.CreateNewAppl(
                admin_code,
                serial_num,
                appl_type,
                activity_code,
                support_year,
                suffix_code,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            this.ViewBag.GrantYearList = EgrantsAppl.LoadApplsBySerialnum(admin_code, serial_num);

            return this.View("~/Egrants/Views/EgrantsApplCreate.cshtml");
        }

        // show attachments docs
        /// <summary>
        /// The doc_attachments.
        /// </summary>
        /// <param name="document_id">
        /// The document_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult doc_attachments(int document_id)
        {
            this.ViewBag.ImageServer = Convert.ToString(this.Session["ImageServerUrl"]);
            this.ViewBag.Attachments = EgrantsDoc.LoadDocAttachments(document_id);

            return this.View("~/Egrants/Views/_Modal_Doc_Attachments.cshtml");
        }

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
        public ActionResult impac_docs(string act, int appl_id)
        {
            this.ViewBag.ImpacDocs = EgrantsDoc.LoadImpacDocs(act, appl_id);
            this.ViewBag.act = act;
            this.ViewBag.appl_id = appl_id;

            return this.View("~/Egrants/Views/_Modal_Impac_Docs.cshtml");
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
            this.ViewBag.notification = EgrantsDoc.getCloseoutNotif(applid, notifName);
            this.ViewBag.applid = applid;

            return this.View("~/Egrants/Views/CloseoutNotif.cshtml");
        }
    }
}