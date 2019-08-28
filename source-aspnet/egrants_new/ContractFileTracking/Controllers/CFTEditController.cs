using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;
using egrants_new.ContractFileTracking.Models;

namespace egrants_new.ContractFileTracking.Controllers
{
    public class CFTEditController : Controller
    {     
        public ActionResult Index()
        {
            return View("~/ContractFileTracking/Views/EditAccessionIndex.cshtml");
        }

        public ActionResult Edit_Box()
        {
            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            return View("~/ContractFileTracking/Views/EditBoxIndex.cshtml");
        }

        public ActionResult To_Add_Box(int accession_id, int box_start_num, int box_end_num)
        {
            //run db to create new accession
            string act = "boxes_new";
            ViewBag.Message = CFTEdit.ContractEdit(act, 0, "", 0, 0, "", accession_id, "", box_start_num, box_end_num, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            return View("~/ContractFileTracking/Views/EditBoxIndex.cshtml");
        }

        public ActionResult Edit_Accession()
        {
            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            return View("~/ContractFileTracking/Views/EditAccessionIndex.cshtml");
        }

        public ActionResult To_Add_Accession(string accession_num)
        {
            //run db to create new accession
            string act = "accession_new";
            ViewBag.Message = CFTEdit.ContractEdit(act, 0, "", 0, 0, accession_num, 0, "", 0, 0, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            return View("~/ContractFileTracking/Views/EditAccessionIndex.cshtml");
        }

        public ActionResult To_Delete_Accession(int accession_id, string destroy_date)
        {
            //run db to destroy accession
            string act = "accession_destroy";
            ViewBag.Message = CFTEdit.ContractEdit(act, 0, "", 0, 0, "", accession_id, destroy_date, 0, 0, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            return View("~/ContractFileTracking/Views/EditAccessionIndex.cshtml");
        }

        public ActionResult Edit_Folder()
        {
            //load color list
            ViewBag.FolderColors = CFTEdit.LoadFolderColors();

            return View("~/ContractFileTracking/Views/EditFolderIndex.cshtml");
        }

        public ActionResult To_Load(int serial_num)
        {
            //save search string
            ViewBag.SerialNum = serial_num;
            
            //load color list
            ViewBag.FolderColors = CFTEdit.LoadFolderColors();
            
            //load CFT contract list by serial number
            ViewBag.CFTContracts = CFTEdit.LoadCFTContracts(serial_num);  

            //load Docman contract by serial number
            ViewBag.DocmanContracts = DocmanCommon.LoadAdditionalContract(serial_num);

            return View("~/ContractFileTracking/Views/EditFolderIndex.cshtml");
        }

        public ActionResult To_Select(int contract_id, int serial_num)
        {
            //save search string
            ViewBag.SerialNum = serial_num;
            
            //load color list
            ViewBag.FolderColors = CFTEdit.LoadFolderColors();

            //load CFT contract infor by contract_id
            ViewBag.CFTContract = CFTEdit.LoadCFTContract(contract_id);
      
            return View("~/ContractFileTracking/Views/EditFolderIndex.cshtml");
        }

        public ActionResult To_Add_Contract(string combined_piid)
        {
            //to add docman contract to CFT system
            string act = "docman_contract_add";
            ViewBag.Message = CFTEdit.ContractEdit(act, 0, combined_piid, 0, 0, "", 0, "", 0, 0, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load color list
            ViewBag.FolderColors = CFTEdit.LoadFolderColors();

            return View("~/ContractFileTracking/Views/EditFolderIndex.cshtml");
        }

        public ActionResult To_Add_Folder(int contract_id, int bar_code, int color_id, int serial_num)
        {
         
            //save search string
            ViewBag.SerialNum = serial_num;
             
            //to add docman contract to CFT system
            string act = "contract_folder_new";
            ViewBag.Message = CFTEdit.ContractEdit(act, contract_id, "", bar_code, color_id, "", 0, "", 0, 0, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load color list
            ViewBag.FolderColors = CFTEdit.LoadFolderColors();

            //load CFT contract infor by contract_id
            ViewBag.CFTContract = CFTEdit.LoadCFTContract(contract_id);

            return View("~/ContractFileTracking/Views/EditFolderIndex.cshtml");
        }

        public ActionResult To_Delete_Folder(int contract_id, int bar_code)
        {
            //to add docman contract to CFT system
            string act = "contract_folder_delete";
            ViewBag.Message = CFTEdit.ContractEdit(act, contract_id, "", bar_code, 0, "", 0, "", 0, 0, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load color list
            ViewBag.FolderColors = CFTEdit.LoadFolderColors();

            return View("~/ContractFileTracking/Views/EditFolderIndex.cshtml");
        }
    }
}