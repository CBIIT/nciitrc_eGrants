using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace egrants_new.ContractFileTracking.Controllers
{
    public class ContractFileTrackingController : Controller
    {
        // GET: CFTMain
        public ActionResult Index()
        {
            return View("~/ContractFileTracking/Views/Index.cshtml");
        }
    }
}