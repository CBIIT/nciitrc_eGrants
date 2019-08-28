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
    public class QCController : Controller
    {
        // GET: Egrants
        public ActionResult Index()
        {
            return RedirectToAction("by_qc", "Egrants");          
        }
    }
}

      