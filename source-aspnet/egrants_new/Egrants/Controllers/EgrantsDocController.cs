using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;
using egrants_new.Egrants.Models;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace egrants_new.Controllers
{
    public class EgrantsDocController : Controller
    {

        // GET: Egrants
        public ActionResult ReportErrorIndex(int document_id)
        {
            ViewBag.DocID = document_id;

            return View("~/Egrants/Views/_Modal_Report_Error.cshtml");
        }

        public ActionResult ReportError(string errormsg, int document_id, string currenturl)
        {
            ViewBag.DocID = document_id;
            ViewBag.Errormsg = errormsg;
            EgrantsDoc.report_doc_error(errormsg, document_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return Redirect(currenturl);
        }


        //show era doc
        public ActionResult show_era_doc(string docurl)
        {
            string cert_url = ConfigurationManager.ConnectionStrings["certPath"].ToString();
            string cert_pass = ConfigurationManager.ConnectionStrings["certPass"].ToString();
            X509Certificate2 certificate = new X509Certificate2(cert_url, cert_pass);
            Uri uri = new Uri(docurl);
            HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(uri);
            webreq.KeepAlive = false;
            webreq.Method = "GET";
            //webreq.Accept = "text/xml";
            webreq.AllowAutoRedirect = false;
            webreq.ClientCertificates.Add(certificate);
            HttpWebResponse webresp;
            webresp = (HttpWebResponse)webreq.GetResponse();
            //string responsecode = webresp.StatusCode.ToString(); 
            string tempLink;

            Stream postStream = webresp.GetResponseStream();
            using (StreamReader reader = new StreamReader(postStream))
            {
                tempLink = reader.ReadToEnd();
            }

            bool displayNGAInline = bool.Parse(ConfigurationManager.AppSettings["NGAShowInline"]);

            if (docurl.EndsWith("NGA") && displayNGAInline)
            {
              return RedirectToAction("GetConverteRaRTF","EgrantsDoc", new { url = tempLink });
            }
            else
            {
                return RedirectToAction("show_era_doc2", "EgrantsDoc", new { url = tempLink });
            }
        }


        public RedirectResult show_era_doc2(string url)
        {

            return Redirect(url);
        }

        public ActionResult GetConverteRaRTF(string url)
        {
            Uri uri = new Uri(url);
            HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(uri);
            webreq.KeepAlive = false;
            webreq.Method = "GET";
            //webreq.Accept = "text/xml";
            webreq.AllowAutoRedirect = false;
            HttpWebResponse webresp;
            webresp = (HttpWebResponse)webreq.GetResponse();
            string rtf = string.Empty;
            Stream postStream = webresp.GetResponseStream();
            using (StreamReader reader = new StreamReader(postStream))
            {
               rtf = reader.ReadToEnd();
            }


            MailIntegrationPage page = new MailIntegrationPage()
            {
                Result = rtf.Replace("\n", "<br/>")
            };

            return View("~/Egrants/Views/EgrantsShowNGA.cshtml", page);

        }

        public ActionResult ShowRTFDocument()
        {

            string contents = System.IO.File.ReadAllText(@"C:\testing\rtf\NGA852271.rtf");


            MailIntegrationPage page = new MailIntegrationPage()
            {
                Result =    contents.Replace("\n","<br/>")
            };


            return View("~/Egrants/Views/EgrantsShowNGA.cshtml", page);

        }




        public ActionResult LoadSupplementDoc(string act, int grant_id)
        {
            ViewBag.Act = act;
            ViewBag.GrantID = grant_id;
            ViewBag.FormerAppls = EgrantsDoc.LoadFormerAppls(grant_id);
            ViewBag.Supplement = EgrantsDoc.LoadSupplement(act, grant_id, 0, "", 0, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants/Views/_Modal_Supplement.cshtml");
        }

        public ActionResult ProcessSupplementDoc(string act, int grant_id, int support_year, string suffix_code, int former_applid, string docid_str)
        {
            ViewBag.Status = "Done";
            ViewBag.GrantID = grant_id;
            ViewBag.FormerAppls = EgrantsDoc.LoadFormerAppls(grant_id);
            ViewBag.Supplement = EgrantsDoc.LoadSupplement(act, grant_id, support_year, suffix_code, former_applid, docid_str, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
            return View("~/Egrants/Views/_Modal_Supplement.cshtml");
        }

        public ActionResult LoadSupplement(string act, int grant_id)
        {
            ViewBag.Act = act;
            ViewBag.GrantID = grant_id;
            ViewBag.FormerAppls = EgrantsDoc.LoadFormerAppls(grant_id);
            ViewBag.Supplement = EgrantsDoc.LoadSupplement(act, grant_id, 0, "", 0, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants/Views/Supplement.cshtml");
        }

        public ActionResult ProcessSupplement(string act, int grant_id, int support_year, string suffix_code, int former_applid, string docid_str)
        {
            ViewBag.Status = "Done";
            ViewBag.GrantID = grant_id;
            ViewBag.FormerAppls = EgrantsDoc.LoadFormerAppls(grant_id);
            ViewBag.Supplement = EgrantsDoc.LoadSupplement(act, grant_id, support_year, suffix_code, former_applid, docid_str, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
            return View("~/Egrants/Views/Supplement.cshtml");
        }

        //modify doc for delete, store or modify doc index
        public void doc_modify(string act, string docids)
        {
            ViewBag.Status = "Done";
            Egrants.Models.EgrantsDoc.doc_modify(act, 0, 0, "", "", docids, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //return View("~/Egrants/Views/DocProcess.cshtml");
        }

       
        //to create new doc
        [HttpGet]
        public ActionResult doc_create_with_applid(string act, string admin_code, int serial_num, int appl_id = 0, int document_id = 0, int category_id = 0, string sub_category = null, string document_date = null, string previous_url = null)
        {
            ViewBag.Act = "Add";
            ViewBag.admincode = admin_code;
            ViewBag.serialnum = serial_num;
            ViewBag.applid = appl_id;
            ViewBag.Previousurl = previous_url;
            ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            ViewBag.CategoryList = EgrantsDoc.LoadCategories(Convert.ToString(Session["ic"]));      //load categories that could only be upload
            ViewBag.SubCategoryList = EgrantsDoc.LoadSubCategoryList();
            ViewBag.MaxCategoryid = EgrantsDoc.GetMaxCategoryid(Convert.ToString(Session["ic"]));

            return View("~/Egrants/Views/egrantsDocCreate.cshtml");
        }

        //create new doc without selected appl_id
        [HttpGet]
        public ActionResult doc_create_without_applid(string previous_url = null)
        {
            ViewBag.Act = "Add";
            ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            ViewBag.CategoryList = EgrantsDoc.LoadCategories(Convert.ToString(Session["ic"]));  //load categories that could only be upload
            ViewBag.SubCategoryList = EgrantsDoc.LoadSubCategoryList();
            ViewBag.MaxCategoryid = EgrantsDoc.GetMaxCategoryid(Convert.ToString(Session["ic"]));
            ViewBag.Previousurl = previous_url;
            return View("~/Egrants/Views/egrantsDocCreate.cshtml");
        }

        //to create doc by file input
        [HttpPost]
        public ActionResult doc_create_by_file(HttpPostedFileBase file, int appl_id, int category_id, string sub_category, DateTime doc_date, string admin_code, int serial_num)
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
                    var document_id = EgrantsDoc.GetDocID(appl_id, category_id, sub_category, doc_date, fileExtension, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                    docName = Convert.ToString(document_id) + fileExtension;

                    //upload to local server
                    //var filePath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Images"), docName);
                    //file.SaveAs(filePath);

                    //upload to image server
                    var fileFolder = @"\\" + Convert.ToString(Session["webgrant"]) + "\\egrants\\funded2\\nci\\main\\";
                    var filePath = System.IO.Path.Combine(fileFolder, docName);
                    file.SaveAs(filePath);

                    //create review url
                    ViewBag.FileUrl = Convert.ToString(Session["ImageServer"]) + "data/" + Convert.ToString(Session["egrantsDocNew"]) + Convert.ToString(docName);
                    ViewBag.Message = "Done! New document has been created";

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

        //to create doc by dragdrop
        [HttpPost]
        public ActionResult doc_create_by_ddrop(HttpPostedFileBase dropedfile, int appl_id, int category_id, string sub_category, DateTime doc_date, string admin_code, int serial_num)
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
                    var document_id = EgrantsDoc.GetDocID(appl_id, category_id, sub_category, doc_date, fileExtension, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                    docName = Convert.ToString(document_id) + fileExtension;

                    //upload to local server
                    //var filePath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Images"), docName);
                    //dropedfile.SaveAs(filePath);

                    //upload to image server
                    var fileFolder = @"\\" + Convert.ToString(Session["webgrant"]) + "\\egrants\\funded2\\nci\\main\\";
                    var filePath = System.IO.Path.Combine(fileFolder, docName);
                    dropedfile.SaveAs(filePath);

                    //create review url
                    ViewBag.FileUrl = Convert.ToString(Session["ImageServer"]) + "data/" + Convert.ToString(Session["egrantsDocNew"]) + Convert.ToString(docName);
                    ViewBag.Message = "Done! New document has been created";

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

        //string full_grant_num, int appl_id, string full_grant_num, int appl_id, 
        public ActionResult doc_upload_default(int doc_id)
        {
            //ViewBag.DocId = doc_id;
            //ViewBag.ApplId = appl_id;
            //ViewBag.DocName = doc_name;
            //ViewBag.DocDate = doc_date;
            //ViewBag.FullGrantNum = full_grant_num;

            var DocInfor = EgrantsDoc.GetDocInfo(doc_id);
            foreach(var doc in DocInfor)
            {
                ViewBag.DocId = doc.document_id;
                ViewBag.ApplId = doc.appl_id;
                ViewBag.DocName = doc.document_name;
                ViewBag.DocDate = doc.document_date;
                ViewBag.FullGrantNum = doc.full_grant_num;
            }

            return View("~/Egrants/Views/egrantsDocUpload.cshtml");
        }

        //to show doc upload modal default
        public ActionResult doc_upload_modal(int doc_id)
        {
            ViewBag.DocId = doc_id;
            ViewBag.DocInfo = EgrantsDoc.GetDocInfo(doc_id);

            return View("~/Egrants/Views/_Modal_Doc_Upload.cshtml");
        }

        //to upload doc by file --added at 4/15/2019 FOR REFRESH AFTER UPLOAD
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        [HttpPost]
        public ActionResult doc_upload_by_file(HttpPostedFileBase file, int doc_id)
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

                    //get document id and create new document name       
                    docName = Convert.ToString(doc_id) + fileExtension;

                    //update url for document
                    EgrantsDoc.doc_modify("to_upload", 0, 0, "", "", Convert.ToString(doc_id), fileExtension, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

                    //upload to local server for testing
                    //var filePath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Images"), docName);
                    //file.SaveAs(filePath);

                    //upload to image sever 
                    var fileFolder = @"\\" + Convert.ToString(Session["webgrant"]) + "\\egrants\\funded\\nci\\modify\\";
                    var filePath = System.IO.Path.Combine(fileFolder, docName);
                    file.SaveAs(filePath);

                    //create review url
                    ViewBag.FileUrl = Convert.ToString(Session["ImageServer"]) + "data/" + Convert.ToString(Session["egrantsDocModify"]) + Convert.ToString(docName);
                    ViewBag.Message = "Done! New document has been created";
                    //ViewBag.Message = "please waiting window refresh...";

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

        //to upload doc by dragdrop---added at 4/15/2019 FOR REFRESH AFTER UPLOAD
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        [HttpPost]
        public ActionResult doc_upload_by_ddrop(HttpPostedFileBase dropedfile, int doc_id)
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

                    //get document id and create new document name       
                    docName = Convert.ToString(doc_id) + fileExtension;

                    //update url for document
                    EgrantsDoc.doc_modify("to_upload", 0, 0, "", "", Convert.ToString(doc_id), fileExtension, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

                    //upload to local server for testing
                    //var filePath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Images"), docName);
                    //dropedfile.SaveAs(filePath);

                    //upload to image sever 
                    var fileFolder = @"\\" + Convert.ToString(Session["webgrant"]) + "\\egrants\\funded\\nci\\modify\\";
                    var filePath = System.IO.Path.Combine(fileFolder, docName);
                    dropedfile.SaveAs(filePath);

                    //create review url
                    ViewBag.FileUrl = Convert.ToString(Session["ImageServer"]) + "data/" + Convert.ToString(Session["egrantsDocModify"]) + Convert.ToString(docName);
                    ViewBag.Message = "Done! New document has been created";
                    //ViewBag.Message = "please waiting window refresh...";

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

        //to update document index for normal documents
        [HttpGet]
        public ActionResult doc_index_update_default(int document_id, string previous_url)
        {
            var DocInfor = EgrantsDoc.GetDocInfo(document_id);
            foreach (var doc in DocInfor)
            {
                ViewBag.Act = "Update";
                ViewBag.admincode = doc.admin_phs_org_code;
                ViewBag.serialnum = doc.serial_num;
                ViewBag.applid = doc.appl_id;
                ViewBag.docid = doc.document_id;
                ViewBag.categoryid = doc.category_id;
                ViewBag.subcategory = doc.sub_category_name;
                ViewBag.docdate = doc.document_date;
                ViewBag.Previousurl = previous_url;
                ViewBag.Status = "default";
            }

            int appl_id = Convert.ToInt32(ViewBag.applid);
            ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            ViewBag.CategoryList = EgrantsDoc.LoadCategories(Convert.ToString(Session["ic"]));
            ViewBag.MaxCategoryid = EgrantsDoc.GetMaxCategoryid(Convert.ToString(Session["ic"]));
            ViewBag.SubCategoryList = EgrantsDoc.LoadSubCategoryList();

            ViewBag.GrantYearList = EgrantsAppl.LoadAppls_by_applid(appl_id);
            //ViewBag.UserList = EgrantsDoc.LoadUsers(Convert.ToString(Session["ic"]));

            return View("~/Egrants/Views/egrantsDocUpdate.cshtml");
        }

        public ActionResult doc_index_modify(string act, int appl_id, int document_id, int category_id, string sub_category, string document_date, string previous_url)
        {
            ViewBag.Status = "Done";
            ViewBag.applid = appl_id;
            ViewBag.Previousurl = previous_url;
            string docids = Convert.ToString(document_id);
            EgrantsDoc.doc_modify(act, appl_id, category_id, sub_category, document_date, docids, "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return Redirect(previous_url);
        }

        //to modify document index for unidentified documrnt
        public ActionResult unidentified_doc_modify(int document_id, int category_id, string document_date, string previous_url)
        {
            ViewBag.docid = document_id;
            ViewBag.categoryid = category_id;
            ViewBag.docdate = document_date;
            ViewBag.Previousurl = previous_url;

            ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            ViewBag.CategoryList = EgrantsDoc.LoadCategories(Convert.ToString(Session["ic"]));
            ViewBag.MaxCategoryid = EgrantsDoc.GetMaxCategoryid(Convert.ToString(Session["ic"]));
            ViewBag.SubCategoryList = EgrantsDoc.LoadSubCategoryList();

            return View("~/Egrants/Views/egrantsDocUpdate.cshtml");
        }

        //public ActionResult doc_index_modify(string act, int appl_id, int document_id, int category_id, string sub_category, string document_date, int specialist_id)
        //{
        //    ViewBag.Status = "Done";
        //    ViewBag.applid = appl_id;
        //    string docids = Convert.ToString(document_id);
        //    EgrantsDoc.doc_modify(act, appl_id, category_id, sub_category, document_date, docids, "", specialist_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

        //    return RedirectToAction("by_appl", "Egrants", new { appl_id = ViewBag.applid, mode="qc" });
        //}

        public ActionResult appl_create_default(string admin_code, int serial_num)
        {
            ViewBag.admincode = admin_code;
            ViewBag.serialnum = serial_num;

            ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            ViewBag.ApplTypeList = EgrantsAppl.LoadApplType();
            ViewBag.ActivityCodeList = EgrantsAppl.LoadActivityCode(admin_code);
            ViewBag.GrantYearList = EgrantsAppl.LoadAppls_by_serialnum(admin_code, serial_num);

            return View("~/Egrants/Views/EgrantsApplCreate.cshtml");
        }

        public ActionResult create_new_appl(string admin_code, int serial_num, int appl_type, string activity_code, int support_year, string suffix_code)
        {
            ViewBag.admincode = admin_code;
            ViewBag.serialnum = serial_num;

            ViewBag.AdminCodeList = EgrantsCommon.LoadAdminCodes();
            ViewBag.ApplTypeList = EgrantsAppl.LoadApplType();
            ViewBag.ActivityCodeList = EgrantsAppl.LoadActivityCode(admin_code);

            ViewBag.Message = EgrantsAppl.CreateNewAppl(admin_code, serial_num, appl_type, activity_code, support_year, suffix_code, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
            ViewBag.GrantYearList = EgrantsAppl.LoadAppls_by_serialnum(admin_code, serial_num);

            return View("~/Egrants/Views/EgrantsApplCreate.cshtml");
        }

        //show attachments docs
        public ActionResult doc_attachments(int document_id)
        {
            ViewBag.ImageServer = Convert.ToString(Session["ImageServer"]);
            ViewBag.Attachments = EgrantsDoc.LoadDocAttachments(document_id);

            return View("~/Egrants/Views/_Modal_Doc_Attachments.cshtml");
        }

        //show impac doc FRS or Closeout Notification
        public ActionResult impac_docs(string act, int appl_id)
        {
            ViewBag.ImpacDocs = Egrants.Models.EgrantsDoc.LoadImpacDocs(act, appl_id);
            ViewBag.act = act;
            ViewBag.appl_id = appl_id;
            return View("~/Egrants/Views/_Modal_Impac_Docs.cshtml");
        }

        //show Closeout Notification
        public ActionResult closeout_notif(string applid, string notifName)
        {
            ViewBag.notification = EgrantsDoc.getCloseoutNotif(applid, notifName);
            ViewBag.applid = applid;
            return View("~/Egrants/Views/CloseoutNotif.cshtml");
        }
    }
}