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

namespace egrants_new.Controllers
{
    public class InstitutionalFilesController : Controller
    {
        // GET: InstitutionalFiles      
        public ActionResult Index()
        {          
            //set act
            string act = "show_orgs";
            ViewBag.Act = act;

            //load Character Index
            ViewBag.CharacterIndex = InstitutionalFiles.LoadOrgNameCharacterIndex();

            //load default org
            ViewBag.OrgFiles = InstitutionalFiles.LoadOrgList(act, "", 2, 0, 0, 0, "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants/Views/InstitutionalFilesIndex.cshtml");         
        }

        public ActionResult Show_Orgs(int index_id)
        {
            //set act
            string act = "show_orgs";
            ViewBag.Act = act;

            //load Character Index
            ViewBag.CharacterIndex = InstitutionalFiles.LoadOrgNameCharacterIndex();

            //load default org
            ViewBag.OrgFiles = InstitutionalFiles.LoadOrgList(act, "", index_id, 0, 0, 0, "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants/Views/InstitutionalFilesIndex.cshtml");
        }

        public ActionResult Search_Orgs(string act, string str)
        {
            //set act
            ViewBag.Act = act;
            ViewBag.Str = str;

            //load Character Index
            ViewBag.CharacterIndex = InstitutionalFiles.LoadOrgNameCharacterIndex();

            //load default org
            ViewBag.OrgFiles = InstitutionalFiles.LoadOrgList(act, str, 0, 0, 0, 0, "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants/Views/InstitutionalFilesIndex.cshtml");
        }

        public ActionResult Show_Docs(int org_id)
        {
            //set act
            string act= "show_docs";
            ViewBag.Act = act;
            ViewBag.OrgID = org_id;
            //ViewBag.OrgName = org_name;

            //load Character Index
            ViewBag.CharacterIndex = InstitutionalFiles.LoadOrgNameCharacterIndex();

            //load default org
            ViewBag.DocFiles = InstitutionalFiles.LoadOrgDocList(act, "", 0, org_id, 0, 0, "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            return View("~/Egrants/Views/InstitutionalFilesIndex.cshtml");
        }

        public ActionResult Delete_Doc(string act, int doc_id, int org_id, string org_name)
        {
            //disable_doc
            InstitutionalFiles.DisableDoc(act, "", 0, org_id, doc_id, 0, "", "", "", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            ViewBag.Act = act;
            ViewBag.OrgID = org_id;
            ViewBag.OrgName = org_name;
            return Show_Docs(org_id);
        }

        public ActionResult Show_Create_Doc(int org_id, string org_name)
        {
            //set act
            ViewBag.Act = "create new";
            ViewBag.OrgID = org_id;
            ViewBag.OrgName = org_name; 
            ViewBag.Today = System.DateTime.Now.ToShortDateString();

            //load Character Index
            ViewBag.CharacterIndex = InstitutionalFiles.LoadOrgNameCharacterIndex();

            //load default org
            ViewBag.OrgCategory = InstitutionalFiles.LoadOrgCategory();

            return View("~/Egrants/Views/InstitutionalFilesIndex.cshtml");
        }

        [HttpPost]
        public void Create_Doc_by_DDrop(HttpPostedFileBase dropedfile, int category_id, string org_name, string start_date, string end_date, int org_id)
        {
            //ViewBag.Act = "create new";
            if (dropedfile != null && dropedfile.ContentLength > 0)
                try
                {
                    //get file name and file Extension
                    var fileName = System.IO.Path.GetFileName(dropedfile.FileName);
                    var fileExtension = System.IO.Path.GetExtension(fileName);

                    //get document id and create new document name 
                    var docID = InstitutionalFiles.GetDocID(org_id, category_id, fileExtension, start_date, end_date, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                    var docName = Convert.ToString(docID) + fileExtension;

                    //upload to local server
                    //var filePath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Images"), docName);
                    //dropedfile.SaveAs(filePath);

                    //upload to image sever 
                    var fileFolder = @"\\" + Convert.ToString(Session["webgrant"]) + "\\egrants\\funded\\nci\\institutional\\";
                    var filePath = System.IO.Path.Combine(fileFolder, docName);
                    dropedfile.SaveAs(filePath);
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

        [HttpPost]
        public void Create_Doc_by_File(HttpPostedFileBase file, int category_id, string org_name, string start_date, string end_date, int org_id)
        {
            //ViewBag.Act = "create new";
            if (file != null && file.ContentLength > 0)
                try
                {
                    //get file name and file Extension
                    var fileName = System.IO.Path.GetFileName(file.FileName);
                    var fileExtension = System.IO.Path.GetExtension(fileName);

                    //get document id and create new document name 
                    var docID = InstitutionalFiles.GetDocID(org_id, category_id, fileExtension, start_date, end_date, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                    var docName = Convert.ToString(docID) + fileExtension;

                    //upload to local server
                    //var filePath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Images"), docName);
                    //file.SaveAs(filePath);

                    //upload to image sever 
                    var fileFolder = @"\\" + Convert.ToString(Session["webgrant"]) + "\\egrants\\funded\\nci\\institutional\\";
                    var filePath = System.IO.Path.Combine(fileFolder, docName);
                    file.SaveAs(filePath);
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
    }
}