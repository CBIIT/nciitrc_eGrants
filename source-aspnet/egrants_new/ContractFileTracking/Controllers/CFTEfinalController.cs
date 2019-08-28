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
    public class CFTEfinalController : Controller
    {     
        public ActionResult Index(string contract_id)
        {
            //return Contract Efinal data by contract_id
            ViewBag.ContractEfinal = CFTEfinal.LoadContractEfinal(contract_id);

            return View("~/ContractFileTracking/Views/eFinalCostView.cshtml");
        }

        public ActionResult Edit(string contract_id)
        {
            //return Efinal Signed by contract_id
            ViewBag.EfinalSigned = CFTEfinal.LoadEfinalSigned();

            //return Contract Efinal data by contract_id
            ViewBag.ContractEfinal = CFTEfinal.LoadContractEfinal(contract_id);

            //return SignedBy
            ViewBag.SignedBy= CFTEfinal.GetSignedBy(contract_id).ToString();
           
            return View("~/ContractFileTracking/Views/eFinalCostEdit.cshtml");
        }

        public ActionResult Update(string contract_id, string total_amount, string final_payment, string deobligation, string refund, string expired_date, string signed_by)
        {
            //to update final cost
            string act = "update";
            CFTEfinal.ContractEfinal(act, contract_id, total_amount, final_payment, deobligation, refund, expired_date, signed_by);

            //return efinal report
            return Index(contract_id);
        }

        public ActionResult Create(string contract_id, string total_amount, string final_payment, string deobligation, string refund, string expired_date, string signed_by)
        {
            //to add new final cost
            string act = "create";
            CFTEfinal.ContractEfinal(act, contract_id, total_amount, final_payment, deobligation, refund, expired_date, signed_by);

            //return efinal report
            return Index(contract_id);
        }     
    }
}