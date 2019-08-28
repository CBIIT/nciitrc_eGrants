using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;
using egrants_new.Docman.Models;

namespace egrants_new.Controllers
{
    public class DocmanController : Controller
    {
        public ActionResult Index()
        {
            //set tab_id and page_num
            ViewBag.CurrentTab = 1;
            ViewBag.PageNum = 1;

            //load pagenation
            ViewBag.DocmanPagenation = Docman.Models.Docman.LoadPagination("", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load docman contracts 
            ViewBag.DocmanContract = Docman.Models.Docman.LoadDocmanContract("", 0, 1, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Docman/Views/DocmanIndex.cshtml");
        }

        public ActionResult by_str(string str)
        {
            //set tab_id and page_num
            ViewBag.CurrentTab = 1;
            ViewBag.PageNum = 1;

            //load pagenation
            ViewBag.DocmanPagenation = Docman.Models.Docman.LoadPagination(str, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load docman contracts 
            ViewBag.DocmanContract = Docman.Models.Docman.LoadDocmanContract(str, 0, 1, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //get search string
            ViewBag.SearchStr = str;
            //ViewBag.PageNum = page_num;

            return View("~/Docman/Views/DocmanIndex.cshtml");
        }

        public ActionResult by_page(string str, int current_tab, int page_num)
        {
            //get tab_id and page_num
            ViewBag.CurrentTab = current_tab;
            ViewBag.PageNum = page_num;

            //load pagenation
            ViewBag.DocmanPagenation = Docman.Models.Docman.LoadPagination(str, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load docman contracts 
            ViewBag.DocmanContract = Docman.Models.Docman.LoadDocmanContract(str, 0, page_num, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //get search string
            ViewBag.SearchStr = str;
            //ViewBag.PageNum = page_num;

            return View("~/Docman/Views/DocmanIndex.cshtml");
        }

        public ActionResult by_id(int econ_id)
        {
            //set econ_id, mcattotal, ccatotal
            ViewBag.EconID = econ_id;
            ViewBag.Mcat_total = DocmanCommon.GetMCatTotal();
            ViewBag.Ccat_total = DocmanCommon.GetdCCatTotal();

            //load docman MainCategories list
            ViewBag.MainCategories = DocmanCommon.LoadMainCategories();

            //load docman SubCategories list
            ViewBag.SubCategories = DocmanCommon.LoadSubCategories();

            //load docman contracts 
            ViewBag.DocmanContract = Docman.Models.Docman.LoadDocmanContract("", econ_id, 0, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load documents
            ViewBag.DocmanDocument = Docman.Models.Docman.LoadDocmanDocument(econ_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Docman/Views/DocmanContractDocs.cshtml");
        }

        public ActionResult Show_Disable(int econ_id, int doc_id)
        {
            //set econ_id, doc id, mcattotal, ccatotal
            ViewBag.EconID = econ_id;
            ViewBag.DocID = doc_id;

            //load docman contracts 
            ViewBag.DocmanContract = Docman.Models.Docman.LoadDocmanContract("", econ_id, 0, Convert.ToString(Session["browser"]), Convert.ToString(Session["id"]), Convert.ToString(Session["userid"]));

            //load documents
            ViewBag.DocmanDocument = Docman.Models.Docman.LoadDocmanDocument(econ_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Docman/Views/DocmanDocDelete.cshtml");
        }

        public ActionResult To_Disable(string act, int econ_id, int doc_id, string reason)
        {
            //run DB to disable and get return info
            ViewBag.ReturnInfo = Docman.Models.Docman.DocmanDocModify(act, econ_id, 0, "", "", reason, doc_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return by_id(econ_id);
        }

        public ActionResult Show_Modify(int econ_id, int doc_id, int mcat_id, int ccat_id)
        {
            //set econ_id, doc id, mcattotal, ccatotal
            ViewBag.EconID = econ_id;
            ViewBag.DocID = doc_id;
            ViewBag.McatID = mcat_id;
            ViewBag.CcatID = ccat_id;
            ViewBag.Mcat_total = DocmanCommon.GetMCatTotal();
            ViewBag.Ccat_total = DocmanCommon.GetdCCatTotal();

            //load docman MainCategories list
            ViewBag.MainCategories = DocmanCommon.LoadMainCategories();

            //load docman SubCategories list
            ViewBag.SubCategories = DocmanCommon.LoadSubCategories();

            //load docman contracts 
            ViewBag.DocmanContract = Docman.Models.Docman.LoadDocmanContract("", econ_id, 0, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load documents
            ViewBag.DocmanDocument = Docman.Models.Docman.LoadDocmanDocument(econ_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Docman/Views/DocmanDocModify.cshtml");
        }

        public ActionResult To_Modify(string act, int econ_id, int doc_id, int ccat_id, string seq_num, string doc_date)
        {
            //run DB to modify document's index and get return info
            ViewBag.ReturnInfo = Docman.Models.Docman.DocmanDocModify(act, econ_id, ccat_id, seq_num, doc_date, "", doc_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return by_id(econ_id);
        }

        public ActionResult Show_Upload(int econ_id, int doc_id)
        {
            //set econ_id, doc id, mcattotal, ccatotal
            ViewBag.EconID = econ_id;
            ViewBag.DocID = doc_id;
            ViewBag.DocmanEmail = Convert.ToString(Session["docman_email"]);

            //load docman contracts 
            ViewBag.DocmanContract = Docman.Models.Docman.LoadDocmanContract("", econ_id, 0, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load documents
            ViewBag.DocmanDocument = Docman.Models.Docman.LoadDocmanDocument(econ_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Docman/Views/DocmanDocUpload.cshtml");
        }

        public ActionResult Show_Create(int econ_id)
        {
            //set econ_id, doc id, mcattotal, ccatotal
            ViewBag.EconID = econ_id;
            ViewBag.Mcat_total = DocmanCommon.GetMCatTotal();
            ViewBag.Ccat_total = DocmanCommon.GetdCCatTotal();
            ViewBag.DocmanEmail = Convert.ToString(Session["docman_email"]);

            //load docman MainCategories list
            ViewBag.MainCategories = DocmanCommon.LoadMainCategories();

            //load docman SubCategories list
            ViewBag.SubCategories = DocmanCommon.LoadSubCategories();

            //load docman contracts 
            ViewBag.DocmanContract = Docman.Models.Docman.LoadDocmanContract("", econ_id, 0, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Docman/Views/DocmanDocCreate.cshtml");
        }

        //to create doc by dragdrop
        [HttpPost]
        public void Create_Doc_by_DDrop(HttpPostedFileBase dropedfile, int econ_id, string doc_date, string seq_num, int cat_identity)
        {
            //get file name and file Extension
            var act = "to_create";
            var fileName = System.IO.Path.GetFileName(dropedfile.FileName);
            var fileExtension = System.IO.Path.GetExtension(fileName);

            if (dropedfile != null && dropedfile.ContentLength > 0)
                try
                {
                    //create document name 
                    var docID = Docman.Models.Docman.GetDocmanDocID(act, econ_id, cat_identity, seq_num, doc_date, fileExtension, 0, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                    var docName = "c" + Convert.ToString(docID) + fileExtension;

                    //upload file to local server
                    //var filePath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Images"), docName);
                    //dropedfile.SaveAs(filePath);

                    //upload file to image server
                    var fileFolder = @"\\" + Convert.ToString(Session["webgrant"]) + "\\egrants\\funded\\nci\\econ\\main\\";
                    var filePath = System.IO.Path.Combine(fileFolder, docName);
                    dropedfile.SaveAs(filePath);

                    //create review url
                    ViewBag.FileUrl = Convert.ToString(Session["ImageServer"]) + "data/" + Convert.ToString(Session["docmanDoc"]) + Convert.ToString(docName);
                    ViewBag.Message = "Upload successful";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
        }

        //to create doc by file
        [HttpPost]
        public void Create_Doc_by_File(HttpPostedFileBase file, int econ_id, string doc_date, string seq_num, int cat_identity)
        {               
            //get file name and file Extension
            var act = "to_create";
            var fileName = System.IO.Path.GetFileName(file.FileName);
            var fileExtension = System.IO.Path.GetExtension(fileName);

            if (file != null && file.ContentLength > 0)
                try
                {
                    //create document name 
                    var docID = Docman.Models.Docman.GetDocmanDocID(act, econ_id, cat_identity,seq_num, doc_date, fileExtension, 0, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                    var docName = "c"+ Convert.ToString(docID) + fileExtension;

                    //upload file to local server
                    //var filePath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Images"), docName);
                    //file.SaveAs(filePath);

                    //upload file to image server
                    var fileFolder = @"\\" + Convert.ToString(Session["webgrant"]) + "\\egrants\\funded\\nci\\econ\\main\\";
                    var filePath = System.IO.Path.Combine(fileFolder, docName);
                    file.SaveAs(filePath);

                    //create review url
                    ViewBag.FileUrl = Convert.ToString(Session["ImageServer"]) + "data/" + Convert.ToString(Session["docmanDoc"]) + Convert.ToString(docName);
                    ViewBag.Message = "Upload successful";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
        }

        //to upload doc by dragdrop
        [HttpPost]
        public void Upload_Doc_by_DDrop(HttpPostedFileBase dropedfile, int econ_id, string doc_date, string seq_num, int doc_id)
        {
            //get file name and file Extension
            var act = "to_upload";
            var fileName = System.IO.Path.GetFileName(dropedfile.FileName);
            var fileExtension = System.IO.Path.GetExtension(fileName);

            if (dropedfile != null && dropedfile.ContentLength > 0)
                try
                {
                    //create document name 
                    var docID = Docman.Models.Docman.GetDocmanDocID(act, econ_id, 0, seq_num, doc_date, fileExtension, doc_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                    var docName = "c" + Convert.ToString(docID) + fileExtension;

                    //upload file to local server
                    //var filePath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Images"), docName);
                    //dropedfile.SaveAs(filePath);

                    //upload file to image server
                    var fileFolder = @"\\" + Convert.ToString(Session["webgrant"]) + "\\egrants\\funded\\nci\\econ\\main\\";
                    var filePath = System.IO.Path.Combine(fileFolder, docName);
                    dropedfile.SaveAs(filePath);

                    //create review url
                    ViewBag.FileUrl = Convert.ToString(Session["ImageServer"]) + "data/" + Convert.ToString(Session["docmanDoc"]) + Convert.ToString(docName);
                    ViewBag.Message = "Upload successful";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
        }

        //to upload doc by file
        [HttpPost]
        public void Upload_Doc_by_File(HttpPostedFileBase file, int econ_id, string doc_date, string seq_num, int doc_id)
        {
            //get file name and file Extension
            var act = "to_upload";
            var fileName = System.IO.Path.GetFileName(file.FileName);
            var fileExtension = System.IO.Path.GetExtension(fileName);

            if (file != null && file.ContentLength > 0)
                try
                {
                    //create document name 
                    var docID = Docman.Models.Docman.GetDocmanDocID(act, econ_id, 0, seq_num, doc_date, fileExtension, doc_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                    var docName = "c" + Convert.ToString(docID) + fileExtension;

                    //upload file to local server
                    //var filePath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Images"), docName);
                    //file.SaveAs(filePath);

                    //upload file to image server
                    var fileFolder = @"\\" + Convert.ToString(Session["webgrant"]) + "\\egrants\\funded\\nci\\econ\\main\\";
                    var filePath = System.IO.Path.Combine(fileFolder, docName);
                    file.SaveAs(filePath);

                    //create review url
                    ViewBag.FileUrl = Convert.ToString(Session["ImageServer"]) + "data/" + Convert.ToString(Session["docmanDoc"]) + Convert.ToString(docName);
                    ViewBag.Message = "Upload successful";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
        }

        public ActionResult DocmanDeletedDocsIndex()
        {
            return View("~/Docman/Views/DocmanDeletedDocs.cshtml");
        }

        public ActionResult To_Create_Report(string start_date, string end_date)
        {
            ViewBag.StartDate = start_date;
            ViewBag.EndDate = end_date;
            ViewBag.DateRange = null;
            var date_range = "";
            ViewBag.DocDeletedDocs = Docman.Models.Docman.LoadDeletedDocs(start_date, end_date, date_range, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));  //.AsEnumerable();

            return View("~/Docman/Views/DocmanDeletedDocs.cshtml");
        }

        public ActionResult To_Show_Report(string date_range)
        {
            var start_date = "";
            var end_date = "";
            ViewBag.StartDate = null;
            ViewBag.EndDate = null;
            ViewBag.DateRange = date_range;
            ViewBag.DocDeletedDocs = Docman.Models.Docman.LoadDeletedDocs(start_date, end_date, date_range, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"])); //.AsEnumerable();

            return View("~/Docman/Views/DocmanDeletedDocs.cshtml");
        }
    }
}