#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EgrantsFundingController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-05-05
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

using egrants_new.Dashboard.Functions;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

using egrants_new.Egrants.Models;
using egrants_new.Egrants_Funding.Models;
using egrants_new.Functions;
using egrants_new.Models;
using MsgReader.Outlook;
using IronPdf;

#endregion

namespace egrants_new.Controllers
{
    /// <summary>
    ///     The egrants funding controller.
    /// </summary>
    public class EgrantsFundingController : Controller
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        public ActionResult Index(int fy)
        {
            // return fy
            var fiscal_year = 0;

            if (Convert.ToString(fy) == null || Convert.ToString(fy) == string.Empty)
            {
                var current_year = DateTime.Now.Year;
                var current_month = DateTime.Now.Month;

                if (current_month > 9)
                    fiscal_year = current_year + 1;
                else
                    fiscal_year = current_year;
            }
            else
            {
                fiscal_year = fy;
            }

            // set fiscal_year
            this.ViewBag.FY = fiscal_year;

            // get Max Categoryid
            this.ViewBag.MaxCategoryid = EgrantsFunding.GetMaxCategoryid(fy);

            // load funding categories
            this.ViewBag.FundingCategories = EgrantsFunding.LoadFundingCategories(fiscal_year);

            // load funding documents
            this.ViewBag.FundingDocuments = EgrantsFunding.LoadFundingDocs(
                "ViewAll",
                0,
                fiscal_year,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Funding/Views/FundingMaster.cshtml");
        }

        /// <summary>
        ///     The funding_index.
        /// </summary>
        /// <returns>
        ///     The <see cref="ActionResult" /> .
        /// </returns>
        public ActionResult funding_index()
        {
            return this.View("~/Egrants_Funding/Views/Index.cshtml");
        }

        /// <summary>
        /// The view_search.
        /// </summary>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        public ActionResult view_search(int serial_num, int fy)
        {
            // return fy
            this.ViewBag.FY = fy;
            this.ViewBag.SearchStr = Convert.ToString(serial_num);

            // get Max Categoryid
            this.ViewBag.MaxCategoryid = EgrantsFunding.GetMaxCategoryid(fy);

            // load funding categories
            this.ViewBag.FundingCategories = EgrantsFunding.LoadFundingCategories(fy);

            // load funding documents
            this.ViewBag.FundingDocuments = EgrantsFunding.LoadFundingDocs(
                "view_search",
                serial_num,
                fy,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Funding/Views/FundingMaster.cshtml");
        }

        /// <summary>
        /// The ViewAll.
        /// </summary>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        public ActionResult ViewAll(int fy)
        {
            // return fy
            this.ViewBag.FY = fy;

            // get Max Categoryid
            this.ViewBag.MaxCategoryid = EgrantsFunding.GetMaxCategoryid(fy);

            // load funding categories
            this.ViewBag.FundingCategories = EgrantsFunding.LoadFundingCategories(fy);

            // load funding documents
            this.ViewBag.FundingDocuments = EgrantsFunding.LoadFundingDocs(
                "ViewAll",
                0,
                fy,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Funding/Views/FundingMaster.cshtml");
        }

        /// <summary>
        /// The view_arra.
        /// </summary>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        public ActionResult view_arra(int fy)
        {
            // return fy
            this.ViewBag.FY = fy;

            // get Max Categoryid
            this.ViewBag.MaxCategoryid = EgrantsFunding.GetMaxCategoryid(fy);

            // load funding categories
            this.ViewBag.FundingCategories = EgrantsFunding.LoadFundingCategories(fy);

            // load funding documents
            this.ViewBag.FundingDocuments = EgrantsFunding.LoadFundingDocs(
                "view_arra",
                0,
                fy,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Funding/Views/FundingMaster.cshtml");
        }

        /// <summary>
        /// The view_edit.
        /// </summary>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        public ActionResult view_edit(int fy)
        {
            // return fy
            this.ViewBag.FY = fy;

            // load funding documents to edit
            this.ViewBag.FundingDocs = EgrantsFunding.LoadFundingDocs(
                "view_edit",
                0,
                fy,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Funding/Views/FundingDocEdit.cshtml");
        }

        /// <summary>
        /// The funding_doc_default.
        /// </summary>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="previous_url">
        /// The previous_url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        public ActionResult funding_doc_default(string admin_code, int serial_num, int appl_id, string previous_url = null)
        {
            this.ViewBag.admincode = admin_code;
            this.ViewBag.serialnum = serial_num;
            this.ViewBag.applid = appl_id;
            this.ViewBag.Previousurl = previous_url;

            this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            this.ViewBag.CategoryList = EgrantsFunding.LoadFundingCategoryList();
            this.ViewBag.GrantYearList = EgrantsAppl.LoadUploadableApplsByApplid(appl_id);

            return this.View("~/Egrants_Funding/Views/FundingDocCreate.cshtml");
        }

        // search appl to add for new doc
        /// <summary>
        /// The load_appls.
        /// </summary>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <param name="previous_url">
        /// The previous_url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        public ActionResult load_appls(string admin_code, int serial_num, string previous_url = null)
        {
            this.ViewBag.admincode = admin_code;
            this.ViewBag.serialnum = serial_num;
            this.ViewBag.Previousurl = previous_url;

            this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            this.ViewBag.CategoryList = EgrantsFunding.LoadFundingCategoryList();
            this.ViewBag.GrantYearList = EgrantsAppl.LoadApplsBySerialnum(admin_code, serial_num);

            return this.View("~/Egrants_Funding/Views/FundingDocCreate.cshtml");
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
        /// <param name="document_date">
        /// The document_date.
        /// </param>
        /// <param name="sub_category">
        /// The sub_category.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        [HttpPost]
        public ActionResult doc_create_by_ddrop(
            HttpPostedFileBase dropedfile,
            int appl_id,
            int category_id,
            string document_date,
            string sub_category)
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
                    var document_id = EgrantsFunding.GetFundingDocID(
                        appl_id,
                        category_id,
                        document_date,
                        sub_category,
                        fileExtension,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]));

                    if (Convert.ToInt32(document_id) > 9999)
                        docName = "0" + document_id + fileExtension;
                    else docName = "00" + document_id + fileExtension;

                    var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded\\nci\\funding\\upload\\";
                    var filePath = Path.Combine(fileFolder, docName);
                    dropedfile.SaveAs(filePath);

                    // create review url
                    this.ViewBag.FileUrl = Convert.ToString(this.Session["ImageServerUrl"]) + "data/" + Convert.ToString(this.Session["EgrantsFundingRelativePath"])
                                         + Convert.ToString(docName);

                    this.ViewBag.Message = "Done! Funding document has been uploaded";

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
        /// <param name="document_date">
        /// The document_date.
        /// </param>
        /// <param name="sub_category">
        /// The sub_category.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        [HttpPost]
        public ActionResult doc_create_pdf_by_ddrop(
            HttpPostedFileBase dropedfile,
            int appl_id,
            int category_id,
            string document_date,
            string sub_category)
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

                    PdfDocument pdfResult = null;
                    if (fileExtension.Equals(".msg", StringComparison.InvariantCultureIgnoreCase))
                    {
                        byte[] fileData;
                        using (var binaryReader = new BinaryReader(dropedfile.InputStream))
                        {
                            fileData = binaryReader.ReadBytes(dropedfile.ContentLength);
                        }
                        //string base64string = Convert.ToBase64String(fileData);
                        //Storage.Message emailFile = new Storage.Message(base64string);     // usually people pass a stream, not a string

                        using (var memoryStream = new MemoryStream(fileData))
                        {
                            Storage.Message emailFile = new Storage.Message(memoryStream);
                            var converter = new EmailConcatenation.PdfConverter();
                            pdfResult = converter.Convert(emailFile);

                        }
                        fileExtension = ".pdf";
                    }

                    // get document_id and creat a new docName
                    var document_id = EgrantsFunding.GetFundingDocID(
                        appl_id,
                        category_id,
                        document_date,
                        sub_category,
                        fileExtension,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]));

                    if (Convert.ToInt32(document_id) > 9999)
                        docName = "0" + document_id + fileExtension;
                    else docName = "00" + document_id + fileExtension;

                    var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded\\nci\\funding\\upload\\";
                    var filePath = Path.Combine(fileFolder, docName);
                    pdfResult.SaveAs(filePath);

                    // create review url
                    this.ViewBag.FileUrl = Convert.ToString(this.Session["ImageServerUrl"]) + "data/" + Convert.ToString(this.Session["EgrantsFundingRelativePath"])
                                         + Convert.ToString(docName);

                    this.ViewBag.Message = "Done! Funding document has been uploaded";

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

        // to create doc by file
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
        /// <param name="document_date">
        /// The document_date.
        /// </param>
        /// <param name="sub_category">
        /// The sub_category.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        [HttpPost]
        public ActionResult doc_create_by_file(HttpPostedFileBase file, int appl_id, int category_id, string document_date, string sub_category)
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
                    var document_id = EgrantsFunding.GetFundingDocID(
                        appl_id,
                        category_id,
                        document_date,
                        sub_category,
                        fileExtension,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]));

                    if (Convert.ToInt32(document_id) > 9999)
                        docName = "0" + document_id + fileExtension;
                    else docName = "00" + document_id + fileExtension;

                    // upload to image server
                    var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded\\nci\\funding\\upload\\";
                    var filePath = Path.Combine(fileFolder, docName);
                    file.SaveAs(filePath);

                    // create review url
                    this.ViewBag.FileUrl = Convert.ToString(this.Session["ImageServer"]) + "data/" + Convert.ToString(this.Session["EgrantsFundingRelativePath"])
                                         + Convert.ToString(docName);

                    this.ViewBag.Message = "Done! Funding document has been uploaded";

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

        // to create doc by file
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
        /// <param name="document_date">
        /// The document_date.
        /// </param>
        /// <param name="sub_category">
        /// The sub_category.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        [HttpPost]
        public ActionResult doc_create_pdf_by_file(HttpPostedFileBase file, int appl_id, int category_id, string document_date, string sub_category)
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

                    PdfDocument pdfResult = null;
                    if (fileExtension.Equals(".msg", StringComparison.InvariantCultureIgnoreCase))
                    {
                        byte[] fileData;
                        using (var binaryReader = new BinaryReader(file.InputStream))
                        {
                            fileData = binaryReader.ReadBytes(file.ContentLength);
                        }

                        using (var memoryStream = new MemoryStream(fileData))
                        {
                            Storage.Message emailFile = new Storage.Message(memoryStream);
                            var converter = new EmailConcatenation.PdfConverter();
                            pdfResult = converter.Convert(emailFile);

                        }
                        fileExtension = ".pdf";
                    }

                    // get document_id and creat a new docName
                    var document_id = EgrantsFunding.GetFundingDocID(
                        appl_id,
                        category_id,
                        document_date,
                        sub_category,
                        fileExtension,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]));

                    if (Convert.ToInt32(document_id) > 9999)
                        docName = "0" + document_id + fileExtension;
                    else docName = "00" + document_id + fileExtension;

                    // upload to image server
                    var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded\\nci\\funding\\upload\\";
                    var filePath = Path.Combine(fileFolder, docName);
                    pdfResult.SaveAs(filePath);

                    // create review url
                    this.ViewBag.FileUrl = Convert.ToString(this.Session["ImageServer"]) + "data/" + Convert.ToString(this.Session["EgrantsFundingRelativePath"])
                                         + Convert.ToString(docName);

                    this.ViewBag.Message = "Done! Funding document has been uploaded";

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

        /// <summary>
        /// The doc_edit.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="doc_id">
        /// The doc_id.
        /// </param>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        public ActionResult doc_edit(string act, int appl_id, int doc_id, int fy)
        {
            // return fy
            this.ViewBag.FY = fy;

            // edit doc
            EgrantsFunding.EditFundingDoc(act, appl_id, doc_id, Convert.ToString(this.Session["ic"]), Convert.ToString(this.Session["userid"]));

            // load funding documents to edit
            this.ViewBag.FundingDocs = EgrantsFunding.LoadFundingDocs(
                "view_edit",
                0,
                fy,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Funding/Views/FundingDocEdit.cshtml");
        }

        /// <summary>
        /// The doc_create.
        /// </summary>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        public ActionResult doc_create(int appl_id, string admin_code, int serial_num)
        {
            // return variable
            this.ViewBag.Applid = appl_id;

            if (serial_num != 0)
                this.ViewBag.SerialNum = serial_num;

            // load Profiles 
            this.ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            // load appls
            if (appl_id != 0)

                // load one appls by appl_id   
                this.ViewBag.Appls = EgrantsAppl.LoadUploadableApplsByApplid(appl_id);
            else

                // load all appls by serial_num and admin_code 
                this.ViewBag.Appls = EgrantsAppl.LoadUploadableApplsBySerialnum(admin_code, serial_num);

            return this.View("~/Egrants_Funding/Views/FundingDocCreate.cshtml");
        }

        /// <summary>
        /// The doc_add.
        /// </summary>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        public ActionResult doc_add(int appl_id, string file)
        {
            // return variable
            this.ViewBag.Applid = appl_id;
            this.ViewBag.FileLocation = file;

            // load Profiles 
            this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();

            // load one appl by appl_id      
            this.ViewBag.Appls = EgrantsAppl.LoadUploadableApplsByApplid(appl_id);

            return this.View("~/Egrants_Funding/Views/FundingDocCreate.cshtml");
        }

        /// <summary>
        /// The load_doc_appls.
        /// </summary>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="doc_id">
        /// The doc_id.
        /// </param>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        public ActionResult load_doc_appls(int serial_num, string admin_code, int doc_id, int fy)
        {
            // return fy
            this.ViewBag.FY = fy;
            this.ViewBag.Docid = doc_id;
            this.ViewBag.admincode = admin_code;
            this.ViewBag.SerialNum = serial_num;

            // load Profiles 
            this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();

            // load appls
            this.ViewBag.GrantYearList = EgrantsFunding.LoadFullGrantNumbers(serial_num, admin_code, doc_id);

            // load all appls by doc_id
            this.ViewBag.DocAppls = EgrantsFunding.LoadDocAppls(doc_id);

            return this.View("~/Egrants_Funding/Views/FundingApplEdit.cshtml");
        }

        // load appl edit page for this doc_id to add or delete
        /// <summary>
        /// The appl_edit_default.
        /// </summary>
        /// <param name="doc_id">
        /// The doc_id.
        /// </param>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        public ActionResult appl_edit_default(int doc_id, int fy)
        {
            // return fy
            this.ViewBag.FY = fy;
            this.ViewBag.Docid = doc_id;

            // ViewBag.Applid = appl_id;

            // load Profiles 
            this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();

            // load all appls by doc_id
            this.ViewBag.DocAppls = EgrantsFunding.LoadDocAppls(doc_id);

            return this.View("~/Egrants_Funding/Views/FundingApplEdit.cshtml");
        }

        // to add appl or delete appl for this doc_id
        /// <summary>
        /// The appl_edit.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="doc_id">
        /// The doc_id.
        /// </param>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/> .
        /// </returns>
        public ActionResult appl_edit(string act, int appl_id, int doc_id, int fy)
        {
            this.ViewBag.FY = fy;
            this.ViewBag.Docid = doc_id;

            // edit appl
            EgrantsFunding.EditFundingAppl(act, appl_id, doc_id, Convert.ToString(this.Session["ic"]), Convert.ToString(this.Session["userid"]));

            // load Profiles
            this.ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();

            // load all appls by doc_id
            this.ViewBag.DocAppls = EgrantsFunding.LoadDocAppls(doc_id);

            return this.View("~/Egrants_Funding/Views/FundingApplEdit.cshtml");
        }
    }
}