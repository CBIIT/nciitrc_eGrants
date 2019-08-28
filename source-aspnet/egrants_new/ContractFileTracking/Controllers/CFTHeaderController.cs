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
    public class CFTHeaderController : Controller
    {
        public ActionResult Index(int accession_id)
        {
            //load specialist list
            ViewBag.Specialists = DocmanCommon.LoadDocmanUsers(Convert.ToString(Session["ic"]));

            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            //load boxes list
            ViewBag.Boxes = CFTTracking.LoadBoxes(accession_id);

            return View("~/ContractFileTracking/Views/CFTHeaderIndex.cshtml");
        }

        public ActionResult Edit_Folders(string act, string folders, string target_id)
        {
            //run db to edit folders
            CFTTracking.FolderEdit(act, folders, target_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load specialist list
            ViewBag.Specialists = DocmanCommon.LoadDocmanUsers(Convert.ToString(Session["ic"]));

            //load accession list
            ViewBag.Accessions = DocmanCommon.LoadAccessions();

            //load boxes list
            ViewBag.Boxes = CFTTracking.LoadBoxes(0);

            return View("~/ContractFileTracking/Views/CFTHeaderIndex.cshtml");
        }
    }
}