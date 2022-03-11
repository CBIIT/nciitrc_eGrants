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
        [HttpGet]
        public ActionResult Index()
        {
            var _repo = new InstitutionalFilesRepo();
            //Create new Page Model to adhere to MVC practices
            //Should have a Builder but... This will do for now
            var page = new InstitutionallFilesPage()
            {
                SelectedInstitutionalOrg = new InstitutionalOrg(),
                Action = InstitutionalFilesPageAction.ShowOrgs,
                CharacterIndices = _repo.LoadOrgNameCharacterIndices(),
                OrgList = _repo.LoadOrgList( 2)

            };

            return View("~/Egrants/Views/InstitutionalFilesIndex.cshtml", page);
        }

        [HttpGet]
        public ActionResult Show_Orgs(int index_id)
        {
            var _repo = new InstitutionalFilesRepo();

            var page = new InstitutionallFilesPage()
            {
                SelectedInstitutionalOrg = new InstitutionalOrg(),
                Action = InstitutionalFilesPageAction.ShowOrgs,
                CharacterIndices = _repo.LoadOrgNameCharacterIndices(),
                OrgList = _repo.LoadOrgList( index_id)

            };

            return View("~/Egrants/Views/InstitutionalFilesIndex.cshtml",page);
        }

        [HttpGet]
        public ActionResult Search_Orgs(string str)
        {
            var _repo = new InstitutionalFilesRepo();
            var page = new InstitutionallFilesPage()
            {
                SelectedInstitutionalOrg = new InstitutionalOrg(),
                Action = InstitutionalFilesPageAction.ShowOrgs,
                CharacterIndices = _repo.LoadOrgNameCharacterIndices(),
                OrgList = _repo.SearchOrgList(str)
            };

            return View("~/Egrants/Views/InstitutionalFilesIndex.cshtml",page);
        }

        [HttpGet]
        public ActionResult Show_Docs(int org_id = 0, string org_name = "")
        {
            var _repo = new InstitutionalFilesRepo();

            var selectedInstitutionalOrg = _repo.FindOrg(org_id,org_name);

            var page = new InstitutionallFilesPage()
            {
                SelectedInstitutionalOrg = selectedInstitutionalOrg,
                Action = InstitutionalFilesPageAction.ShowDocs,
                CharacterIndices = _repo.LoadOrgNameCharacterIndices(),
                DocFiles = _repo.LoadOrgDocList( selectedInstitutionalOrg.OrgId)

            };

            return View("~/Egrants/Views/InstitutionalFilesIndex.cshtml", page);
        }

        [HttpGet]
        public ActionResult Delete_Doc(string act, int doc_id, int org_id, string org_name)
        {
            var _repo = new InstitutionalFilesRepo();
            //disable_doc
            _repo.DisableDoc(doc_id,  Convert.ToString(Session["userid"]));

            ViewBag.Act = act;
            ViewBag.OrgID = org_id;
            ViewBag.OrgName = org_name;
            return Show_Docs(org_id, org_name);
        }

        [HttpGet]
        public ActionResult Show_Create_Doc(int org_id, string org_name)
        {
            var _repo = new InstitutionalFilesRepo();
            //set act

            var selectedInstitutionalOrg = _repo.FindOrg(org_id);

            var page = new InstitutionallFilesPage()
            {
                SelectedInstitutionalOrg = selectedInstitutionalOrg,
                Action = InstitutionalFilesPageAction.CreateNew,
                CharacterIndices = _repo.LoadOrgNameCharacterIndices(),
                DocFiles = _repo.LoadOrgDocList(org_id),
                OrgCategories = _repo.LoadOrgCategory(true),
                TodayText = System.DateTime.Now.ToShortDateString()
            };

            return View("~/Egrants/Views/InstitutionalFilesIndex.cshtml", page);
        }

        [HttpGet]
        public ActionResult Show_Update_Doc(int doc_id, int org_id )
        {
            var _repo = new InstitutionalFilesRepo();
            var selectedInstitutionalOrg = _repo.FindOrg(org_id);

            var page = new InstitutionallFilesPage()
            {
                SelectedInstitutionalOrg = selectedInstitutionalOrg,
                Action = InstitutionalFilesPageAction.UpdateDoc,
                CharacterIndices = _repo.LoadOrgNameCharacterIndices(),
                SelectedDocFile = _repo.LoadOrgDocList(org_id).Where(d => d.DocumentId == doc_id).FirstOrDefault(),
//                DocFiles = _repo.LoadOrgDocList(org_id).Where(d => d.DocumentId == doc_id).ToList(),
                OrgCategories = _repo.LoadOrgCategory(false),
                TodayText = System.DateTime.Now.ToShortDateString()
            };

            return View("~/Egrants/Views/InstitutionalFilesIndex.cshtml", page);
        }

        [HttpPost]
        public void Create_Doc_by_DDrop(HttpPostedFileBase dropedfile, int category_id, string org_name, string start_date, string end_date, int org_id, string comments)
        {
            var _repo = new InstitutionalFilesRepo();
            //ViewBag.Act = "create new";
            if (dropedfile != null && dropedfile.ContentLength > 0)
                try
                {
                    //get file name and file Extension
                    var fileName = System.IO.Path.GetFileName(dropedfile.FileName);
                    var fileExtension = System.IO.Path.GetExtension(fileName);

                    //get document id and create new document name 
                    var docID = _repo.GetDocID(org_id, category_id, fileExtension, start_date, end_date, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]),comments);
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
        public void Create_Doc_by_File(HttpPostedFileBase file, int category_id, string org_name, string start_date, string end_date, int org_id, string comments)
        {
            var _repo = new InstitutionalFilesRepo();
            //ViewBag.Act = "create new";
            if (file != null && file.ContentLength > 0)
                try
                {
                    //get file name and file Extension
                    var fileName = System.IO.Path.GetFileName(file.FileName);
                    var fileExtension = System.IO.Path.GetExtension(fileName);

                    //get document id and create new document name 
                    var docID = _repo.GetDocID(org_id, category_id, fileExtension, start_date, end_date, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]),comments);
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


        [HttpPost]
        public void Update_Doc(int category_id, string start_date, string end_date, string comments, int doc_id)
        {
            var _repo = new InstitutionalFilesRepo();
            try
            {
                _repo.UpdateDocument(doc_id,category_id, start_date, end_date, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]), comments);

            }
            catch (Exception ex)
            {

            }
        }

    }
}