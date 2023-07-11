#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  HomeController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2023-07-11
// Contributors:
//      - Briggs, Robin (NIH/NCI) [C] - briggsr2
//      -
// Copyright (c) National Institute of Health
// 
// <Description of the file>
// 
// This source is subject to the NIH Softwre License.
// See https://ncihub.org/resources/899/download/Guidelines_for_Releasing_Research_Software_04062015.pdf
// All other rights reserved.
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT ARE DISCLAIMED. IN NO EVENT SHALL THE NATIONAL
// CANCER INSTITUTE (THE PROVIDER), THE NATIONAL INSTITUTES OF HEALTH, THE
// U.S. GOVERNMENT OR THE INDIVIDUAL DEVELOPERS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
// \***************************************************************************/

#endregion

#region

using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Web.Mvc;

using egrants_new.Functions;
using egrants_new.Models;

using Newtonsoft.Json;

#endregion

namespace egrants_new.Controllers
{
    /// <summary>
    ///     The egrants controller.
    /// </summary>
    public class HomeController : Controller
    {

        // go to default 
        /// <summary>
        ///     The go_to_default.
        /// </summary>
        /// <returns>
        ///     The <see cref="ActionResult" />.CIS
        /// </returns>
        public ActionResult Go_to_default()
        {
            return View("~/Views/Shared/Go_to_Default.cshtml");
        }

        // GET: Egrants
        /// <summary>
        ///     The index.
        /// </summary>
        /// <returns>
        ///     The <see cref="ActionResult" />.
        /// </returns>
        public ActionResult Index()
        {
            // return IC list
            ViewBag.ICList = EgrantsCommon.LoadAdminCodes();
            ViewBag.CurrentView = "StandardForm";

            return View("~/Views/Egrants/Index.cshtml");
        }

        /// <summary>
        /// The session timeout.
        /// </summary>
        /// <param name="time">
        /// The time.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult SessionTimeout()
        {
            // update the timeout variable 
            return Json("Winner", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Download the files to the temp directory from the links checked on the page. Then return the stream of bytes to the
        ///     calling method.
        /// </summary>
        /// <param name="appl">
        /// </param>
        /// <param name="fullGrantNumber">
        /// The full Grant Number.
        /// </param>
        /// <param name="listOfUrl">
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult IsDownloadForm(string appl, string fullGrantNumber, IList<string> listOfUrl)
        {
            // update the timeout variable 
            return Json(string.Empty, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Override the JSON Result with Max integer JSON lenght
        /// </summary>
        /// <param name="data">
        /// Data
        /// </param>
        /// <param name="contentType">
        /// Content Type
        /// </param>
        /// <param name="contentEncoding">
        /// Content Encoding
        /// </param>
        /// <param name="behavior">
        /// Behavior
        /// </param>
        /// <returns>
        /// As JsonResult
        /// </returns>
        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult
                   {
                       Data = data,
                       ContentType = contentType,
                       ContentEncoding = contentEncoding,
                       JsonRequestBehavior = behavior,
                       MaxJsonLength = int.MaxValue
                   };
        }

        // [HttpGet]
        /// <summary>
        /// The download.
        /// </summary>
        /// <param name="fileGuid">
        /// The file guid.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public virtual ActionResult Download(string fileGuid, string fileName)
        {
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];

                ContentDisposition cd = new ContentDisposition
                                        {
                                            // for example foo.bak
                                            FileName = fileName,

                                            // always prompt the user for downloading, set to true if you want 
                                            // the browser to try to show the file inline
                                            Inline = false
                                        };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(data, "application/zip");
            }

            // Problem - Log the error, generate a blank file,
            // redirect to another controller action - whatever fits with your application
            return new EmptyResult();
        }

        /// <summary>
        /// The replace invalid chars.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <param name="replacementCharacter">
        /// The replacement character.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ReplaceInvalidChars(string filename, string replacementCharacter)
        {
            return string.Join(replacementCharacter, filename.Split(Path.GetInvalidFileNameChars()));
        }

        /// <summary>
        /// Get all appls list for appls toggle by grant_id
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string LoadAllApplsHome(int grant_id)
        {
            List<string> list = EgrantsAppl.GetAllAppls(grant_id);

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }

        /// <summary>
        /// Load 12 appls list for appls toggle by grant_id
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string LoadDefaultApplsHome(int grant_id)
        {
            List<string> list = EgrantsAppl.GetDefaultAppls(grant_id);

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }

        // get appls list with documents by (admin_code and serial_num) commented out by Leon at 3/15/2019
        // public string LoadYears(string admin_code, string serial_num)   //string fy, string mechan, s
        // {
        // List<string> yearlist = Egrants.Models.Egrants.GetYearList(admin_code, serial_num);
        // JavaScriptSerializer js = new JavaScriptSerializer();
        // return js.Serialize(yearlist);           
        // }

        // get appls list with documents by (admin_code and serial_num) added by Ayu at 3/15/2019
        /// <summary>
        /// The load years.
        /// </summary>
        /// <param name="fiscalYear">
        /// The fiscal Year.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism.
        /// </param>
        /// <param name="adminCode">
        /// The admin Code.
        /// </param>
        /// <param name="serialNumber">
        /// The serial Number.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string LoadYearsHome(string fiscalYear = null, string mechanism = null, string adminCode = null, string serialNumber = null)
        {
            // string fy, string mechan, s
            List<string> list = Dashboard.Functions.Egrants.GetYearList(fiscalYear, mechanism, adminCode, serialNumber);

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }

        // load all appls list with or without documents
        /// <summary>
        /// The get all appls list.
        /// </summary>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetAllApplsListHome(string admin_code, string serial_num)
        {
            // string fy, string mechan, s
            List<string> list = EgrantsAppl.GetAllApplsList(admin_code, serial_num);

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }

        // get category list by grant_id and years
        /// <summary>
        /// The load categories.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <param name="years">
        /// The years.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string LoadCategoriesHome(int grant_id, string years)
        {
            List<string> list = Dashboard.Functions.Egrants.GetCategoryList(grant_id, years);

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }
    }
}