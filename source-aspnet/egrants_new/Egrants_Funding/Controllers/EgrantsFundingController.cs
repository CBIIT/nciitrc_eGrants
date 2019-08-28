using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;
using egrants_new.Egrants_Funding.Models;

namespace egrants_new.Controllers
{
    public class EgrantsFundingController : Controller
    {
        public ActionResult Index(int fy)
        {         
            //return fy
            int fiscal_year = 0;
            if (Convert.ToString(fy) == null || Convert.ToString(fy) == "")
            {
                int current_year = DateTime.Now.Year;
                int current_month = DateTime.Now.Month;
                if (current_month > 9)
                {
                    fiscal_year = current_year + 1;
                }
                else
                {
                    fiscal_year = current_year;
                }
            }
            else
            {
                fiscal_year = fy;
            }

            //set fiscal_year
            ViewBag.FY = fiscal_year;

            //get Max Categoryid
            ViewBag.MaxCategoryid = EgrantsFunding.GetMaxCategoryid(fy);

            //load funding categories
            ViewBag.FundingCategories = EgrantsFunding.LoadFundingCategories(fiscal_year);

            //load funding documents
            ViewBag.FundingDocuments = EgrantsFunding.LoadFundingDocs("view_all", 0, fiscal_year, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Funding/Views/FundingMaster.cshtml");        
        }

        public ActionResult funding_index()
        {
           return View("~/Egrants_Funding/Views/Index.cshtml");
        }

        public ActionResult view_search(int serial_num, int fy)
        {
            //return fy
            ViewBag.FY = fy;
            ViewBag.SearchStr = Convert.ToString(serial_num);

            //get Max Categoryid
            ViewBag.MaxCategoryid = EgrantsFunding.GetMaxCategoryid(fy);

            //load funding categories
            ViewBag.FundingCategories = EgrantsFunding.LoadFundingCategories(fy);

            //load funding documents
            ViewBag.FundingDocuments = EgrantsFunding.LoadFundingDocs("view_search", serial_num, fy, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Funding/Views/FundingMaster.cshtml");
        }

        public ActionResult view_all(int fy)
        {
            //return fy
            ViewBag.FY = fy;

            //get Max Categoryid
            ViewBag.MaxCategoryid = EgrantsFunding.GetMaxCategoryid(fy);

            //load funding categories
            ViewBag.FundingCategories = EgrantsFunding.LoadFundingCategories(fy);

            //load funding documents
            ViewBag.FundingDocuments = EgrantsFunding.LoadFundingDocs("view_all", 0, fy, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Funding/Views/FundingMaster.cshtml");
        }

        public ActionResult view_arra(int fy)
        {
            //return fy
            ViewBag.FY = fy;

            //get Max Categoryid
            ViewBag.MaxCategoryid = EgrantsFunding.GetMaxCategoryid(fy);

            //load funding categories
            ViewBag.FundingCategories = EgrantsFunding.LoadFundingCategories(fy);

            //load funding documents
            ViewBag.FundingDocuments = EgrantsFunding.LoadFundingDocs("view_arra", 0, fy, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Funding/Views/FundingMaster.cshtml");
        }

        public ActionResult view_edit(int fy)
        {
            //return fy
            ViewBag.FY = fy;

            //load funding documents to edit
            ViewBag.FundingDocs = EgrantsFunding.LoadFundingDocs("view_edit", 0, fy, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Funding/Views/FundingDocEdit.cshtml");
        }

        public ActionResult funding_doc_default(string admin_code, int serial_num, int appl_id, string previous_url = null)
        {
            ViewBag.admincode = admin_code;
            ViewBag.serialnum = serial_num;
            ViewBag.applid = appl_id;
            ViewBag.Previousurl = previous_url;

            ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            ViewBag.CategoryList = EgrantsFunding.LoadFundingCategoryList();
            ViewBag.GrantYearList = Egrants.Models.EgrantsAppl.LoadUploadableAppls_by_applid(appl_id);

            return View("~/Egrants_Funding/Views/FundingDocCreate.cshtml");
        }

        //search appl to add for new doc
        public ActionResult load_appls(string admin_code, int serial_num, string previous_url = null)
        {
            ViewBag.admincode = admin_code;
            ViewBag.serialnum = serial_num;
            ViewBag.Previousurl = previous_url;

            ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            ViewBag.CategoryList = EgrantsFunding.LoadFundingCategoryList();
            ViewBag.GrantYearList = Egrants.Models.EgrantsAppl.LoadAppls_by_serialnum(admin_code, serial_num);

            return View("~/Egrants_Funding/Views/FundingDocCreate.cshtml");
        }

        //to create doc by dragdrop
        [HttpPost]
        public ActionResult doc_create_by_ddrop(HttpPostedFileBase dropedfile, int appl_id, int category_id, string document_date, string sub_category)
        {
            var docName = "";
            string url = null;
            string mssg = null;

            if (dropedfile != null && dropedfile.ContentLength > 0)
                try
                {
                    //get file name and file Extension
                    var fileName = System.IO.Path.GetFileName(dropedfile.FileName);
                    var fileExtension = System.IO.Path.GetExtension(fileName);             

                    // get document_id and creat a new docName
                    var document_id = EgrantsFunding.GetFundingDocID(appl_id, category_id, document_date, sub_category, fileExtension, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                    if (Convert.ToInt32(document_id) > 9999){
                        docName = "0" + document_id + fileExtension; 
                    }else docName = "00" + document_id + fileExtension;

                    //upload to local server
                    //var filePath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Images"), docName);
                    //dropedfile.SaveAs(filePath);

                    //upload to image server
                    var fileFolder = @"\\" + Convert.ToString(Session["webgrant"]) + "\\egrants\\funded\\nci\\funding\\upload\\";
                    var filePath = System.IO.Path.Combine(fileFolder, docName);
                    dropedfile.SaveAs(filePath);

                    //create review url
                    ViewBag.FileUrl = Convert.ToString(Session["ImageServer"]) + "data/" + Convert.ToString(Session["egrantsFunding"]) + Convert.ToString(docName);
                    ViewBag.Message = "Done! Funding document has been uploaded";

                    url = ViewBag.FileUrl;
                    mssg = ViewBag.Message;
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }

            return Json(new { url = url, message = mssg });
        }

        //to create doc by file
        [HttpPost]
        public ActionResult doc_create_by_file(HttpPostedFileBase file, int appl_id, int category_id, string document_date, string sub_category)  
        {
            var docName = "";
            string url = null;
            string mssg = null;

            if (file != null && file.ContentLength > 0)
                try
                {
                    //get file name and file Extension
                    var fileName = System.IO.Path.GetFileName(file.FileName);
                    var fileExtension = System.IO.Path.GetExtension(fileName);

                    // get document_id and creat a new docName
                    string document_id = EgrantsFunding.GetFundingDocID(appl_id, category_id, document_date, sub_category, fileExtension, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                    if (Convert.ToInt32(document_id) > 9999){
                        docName = "0" + document_id + fileExtension;
                    }else docName = "00" + document_id + fileExtension;

                    //upload to local server for testing
                    //var filePath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Images"), docName);
                    //file.SaveAs(filePath);

                    //upload to image server
                    var fileFolder = @"\\" + Convert.ToString(Session["webgrant"]) + "\\egrants\\funded\\nci\\funding\\upload\\";
                    var filePath = System.IO.Path.Combine(fileFolder, docName);
                    file.SaveAs(filePath);

                    //create review url
                    ViewBag.FileUrl = Convert.ToString(Session["ImageServer"]) + "data/" + Convert.ToString(Session["egrantsFunding"]) + Convert.ToString(docName);
                    ViewBag.Message = "Done! Funding document has been uploaded";

                    url = ViewBag.FileUrl;
                    mssg = ViewBag.Message;
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "Error while uploading the files.";
            }

            return Json(new { url = url, message = mssg });
        }

        public ActionResult doc_edit(string act, int appl_id, int doc_id, int fy)
        {
            //return fy
            ViewBag.FY = fy;

            //edit doc
            EgrantsFunding.EditFundingDoc(act, appl_id, doc_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load funding documents to edit
            ViewBag.FundingDocs = EgrantsFunding.LoadFundingDocs("view_edit", 0, fy, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants_Funding/Views/FundingDocEdit.cshtml");
        }

        public ActionResult doc_create(int appl_id, string admin_code, int serial_num)
        {
            //return variable
            ViewBag.Applid = appl_id;

            if (serial_num!=0)
            {
                ViewBag.SerialNum = serial_num;
            }

            //load Profiles 
            ViewBag.AdminCodes = EgrantsCommon.LoadAdminCodes();

            //load appls
            if (appl_id != 0)
            {
                //load one appls by appl_id   
                ViewBag.Appls = Egrants.Models.EgrantsAppl.LoadUploadableAppls_by_applid(appl_id);
            }
            else
            {
                //load all appls by serial_num and admin_code 
                ViewBag.Appls = Egrants.Models.EgrantsAppl.LoadUploadableAppls_by_serialnum(admin_code, serial_num);
            }

            return View("~/Egrants_Funding/Views/FundingDocCreate.cshtml");
        }

        public ActionResult doc_add(int appl_id, string file)
        {
            //return variable
            ViewBag.Applid = appl_id;
            ViewBag.FileLocation = file;

            //load Profiles 
            ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();

            //load one appl by appl_id      
            ViewBag.Appls = Egrants.Models.EgrantsAppl.LoadUploadableAppls_by_applid(appl_id);

            return View("~/Egrants_Funding/Views/FundingDocCreate.cshtml");
        }
   
        public ActionResult load_doc_appls(int serial_num, string admin_code, int doc_id, int fy)
        {
            //return fy
            ViewBag.FY = fy;
            ViewBag.Docid = doc_id;
            ViewBag.admincode = admin_code;
            ViewBag.SerialNum = serial_num;

            //load Profiles 
            ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();

            //load appls
            ViewBag.GrantYearList = EgrantsFunding.LoadFullGrantNumbers(serial_num, admin_code, doc_id);
           
            //load all appls by doc_id
            ViewBag.DocAppls = EgrantsFunding.LoadDocAppls(doc_id);
      
            return View("~/Egrants_Funding/Views/FundingApplEdit.cshtml");      
        }

        //load appl edit page for this doc_id to add or delete
        public ActionResult appl_edit_default(int doc_id, int fy)
        {
            //return fy
            ViewBag.FY = fy;
            ViewBag.Docid = doc_id;
            //ViewBag.Applid = appl_id;

            //load Profiles 
            ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();

            //load all appls by doc_id
            ViewBag.DocAppls = EgrantsFunding.LoadDocAppls(doc_id);

            return View("~/Egrants_Funding/Views/FundingApplEdit.cshtml");
        }

        //to add appl or delete appl for this doc_id
        public ActionResult appl_edit(string act, int appl_id, int doc_id, int fy)
        {
            ViewBag.FY = fy;
            ViewBag.Docid = doc_id;

            //edit appl
            EgrantsFunding.EditFundingAppl(act, appl_id, doc_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load Profiles
            ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();

            //load all appls by doc_id
            ViewBag.DocAppls = EgrantsFunding.LoadDocAppls(doc_id);

            return View("~/Egrants_Funding/Views/FundingApplEdit.cshtml");
        }
    }
}