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
    public class ManagementController : Controller
    {         
        public ActionResult Index()
        {
            //load egrants qc reasons
            ViewBag.QCReasons = QCAssignment.LoadQCReasons(Convert.ToString(Session["ic"]));

            //load egrants specialist list
            ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(Session["ic"]));

            //load qc persons list
            ViewBag.QCPersons = QCAssignment.LoadQCPersons(Convert.ToString(Session["ic"]));

            //load qc report
            ViewBag.QCReport = QCAssignment.LoadQCReport(Convert.ToString(Session["ic"]));

            return View("~/Management/Views/Index.cshtml");         
        }
       
        public ActionResult To_Assign(string qc_reason, string qc_person_id)
        {
            var act = "to_assign";
            int qcperson_id = Convert.ToInt32(qc_person_id);
            var person_id = 0;
            var percent = 0;
            QCAssignment.run_db(act, qcperson_id, qc_reason, percent, person_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load egrants qc reasons
            ViewBag.QCReasons = QCAssignment.LoadQCReasons(Convert.ToString(Session["ic"]));

            //load egrants specialist list
            ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(Session["ic"]));

            //load qc persons list
            ViewBag.QCPersons = QCAssignment.LoadQCPersons(Convert.ToString(Session["ic"]));

            //load qc report
            ViewBag.QCReport = QCAssignment.LoadQCReport(Convert.ToString(Session["ic"]));

            return View("~/Management/Views/Index.cshtml");
        }

        public ActionResult To_Remove(string qc_reason, string qc_person_id)
        {
            var act = "to_remove";
            int qcperson_id = Convert.ToInt32(qc_person_id);
            var person_id = 0;
            var percent = 0;
            QCAssignment.run_db(act, qcperson_id, qc_reason, percent, person_id, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load egrants qc reasons
            ViewBag.QCReasons = QCAssignment.LoadQCReasons(Convert.ToString(Session["ic"]));

            //load egrants specialist list
            ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(Session["ic"]));

            //load qc persons list
            ViewBag.QCPersons = QCAssignment.LoadQCPersons(Convert.ToString(Session["ic"]));

            //load qc report
            ViewBag.QCReport = QCAssignment.LoadQCReport(Convert.ToString(Session["ic"]));

            return View("~/Management/Views/Index.cshtml");
        }

         public ActionResult To_Route(string person_id, string percent, string qc_person_id)
        {
            var act = "to_route";
            int qcperson_id = Convert.ToInt32(qc_person_id);
            int personid = Convert.ToInt32(person_id);
            int percents = Convert.ToInt32(percent);
            var qc_reason = "";
            QCAssignment.run_db(act, qcperson_id, qc_reason, percents, personid, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

            //load egrants qc reasons
            ViewBag.QCReasons = QCAssignment.LoadQCReasons(Convert.ToString(Session["ic"]));

            //load egrants specialist list
            ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(Session["ic"]));

            //load qc persons list
            ViewBag.QCPersons = QCAssignment.LoadQCPersons(Convert.ToString(Session["ic"]));

            //load qc report
            ViewBag.QCReport = QCAssignment.LoadQCReport(Convert.ToString(Session["ic"]));

            return View("~/Management/Views/Index.cshtml");
        }
    }
}