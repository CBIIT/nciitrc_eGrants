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
    public class CFTTrackingController : Controller
    {      
        public ActionResult Index()
        {
            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            //load institution list
            ViewBag.Institutions = CFTTracking.LoadInstitutions();

            return View("~/ContractFileTracking/Views/CFTTrackingIndex.cshtml");
        }

        public ActionResult By_Charged_Out()
        {
            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            //load institution list
            ViewBag.Institutions = CFTTracking.LoadInstitutions();

            //load contract folder list
            ViewBag.ContractFolders = CFTTracking.LoadContractFolders("charged_out", "null", "null", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //return search type
            ViewBag.SearchType = "By_Charged_Out";
            ViewBag.SearchStr = "";

            return View("~/ContractFileTracking/Views/CFTTrackingIndex.cshtml");
        }

        public ActionResult By_Bar_Code(string bar_code)
        {
            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            //load institution list
            ViewBag.Institutions = CFTTracking.LoadInstitutions();

            //load contract folder list
            ViewBag.ContractFolders = CFTTracking.LoadContractFolders("bar_code", bar_code, "null", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //return search string
            ViewBag.BarCode = bar_code;
            ViewBag.SearchType = "By_Bar_Code";
            ViewBag.SearchStr = bar_code;

            return View("~/ContractFileTracking/Views/CFTTrackingIndex.cshtml");
        }

        public ActionResult By_Accession(string accession_id)
        {
            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            //load institution list
            ViewBag.Institutions = CFTTracking.LoadInstitutions();

            //load contract folder list
            ViewBag.ContractFolders = CFTTracking.LoadContractFolders("accession", accession_id, "null", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //return search string
            ViewBag.AccessionId = accession_id;
            ViewBag.SearchType = "By_Accession";
            ViewBag.SearchStr = accession_id;

            return View("~/ContractFileTracking/Views/CFTTrackingIndex.cshtml");
        }

        public ActionResult By_Institution(string institution)
        {
            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            //load institution list
            ViewBag.Institutions = CFTTracking.LoadInstitutions();

            //load contract folder list
            ViewBag.ContractFolders = CFTTracking.LoadContractFolders("institution", institution, "null", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //return search string
            ViewBag.InstitutionName = institution;
            ViewBag.SearchType = "By_Institution";
            ViewBag.SearchStr = institution;

            return View("~/ContractFileTracking/Views/CFTTrackingIndex.cshtml");
        }

        public ActionResult By_Serial_Num(int serial_num)
        {
            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            //load institution list
            ViewBag.Institutions = CFTTracking.LoadInstitutions();

            //load docman contract list
            ViewBag.docmanContracts = DocmanCommon.LoadAdditionalContract(serial_num);

            //load contract folder list
            string SearchStr = Convert.ToString(serial_num);
            ViewBag.ContractFolders = CFTTracking.LoadContractFolders("serial_num", SearchStr, "null", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            

            //return search string
            ViewBag.SerialNum = serial_num;
            ViewBag.SearchType = "By_Serial_Num";
            ViewBag.SearchStr = serial_num;

            return View("~/ContractFileTracking/Views/CFTTrackingIndex.cshtml");
        }

        public ActionResult By_Add(string serial_num, string combined_piid)
        {
            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            //load institution list
            ViewBag.Institutions = CFTTracking.LoadInstitutions();

            //load contract folder list
            ViewBag.ContractFolders = CFTTracking.LoadContractFolders("add", serial_num, combined_piid, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load docman contract list

            int serial_number = Convert.ToInt32(serial_num);
            ViewBag.docmanContracts = DocmanCommon.LoadAdditionalContract(serial_number);

            return View("~/ContractFileTracking/Views/CFTTrackingIndex.cshtml");
        }       
    }
}

