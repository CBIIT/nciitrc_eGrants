using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Controllers.Management
{
    public class SystemReportController : Controller
    {    
        public ActionResult Index()
        {
            //load egrants accession list
            ViewBag.Accessions = SystemReport.LoadAccessions(Convert.ToString(Session["ic"]));

            return View("~/Management/Views/SystemReport.cshtml");
        }

        public ActionResult by_Serialnum(int serial_number)
        {
            string act = "by_serialnumber";
            ViewBag.SerialNumber = serial_number;

            //load egrants accession list
            ViewBag.Accessions = SystemReport.LoadAccessions(Convert.ToString(Session["ic"]));

            //load folders by serial number search
            ViewBag.EgrantsFolders = SystemReport.LoadFolders(act, serial_number, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"])).AsEnumerable();

            return View("~/Management/Views/SystemReport.cshtml");
        }

        public ActionResult by_Accessionid(int accession_id)
        {
            string act = "by_accessionid";
            ViewBag.AccessionID = accession_id;
            
            //load egrants accession list
            ViewBag.Accessions = SystemReport.LoadAccessions(Convert.ToString(Session["ic"]));

            //load folders by accession id search
            ViewBag.EgrantsFolders = SystemReport.LoadFolders(act, accession_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"])).AsEnumerable();

            return View("~/Management/Views/SystemReport.cshtml");
        }       
    }
}