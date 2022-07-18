#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  ApplDestructedController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-05-05
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

using System;
using System.Web.Mvc;

using egrants_new.Egrants_Admin.Models;
using egrants_new.Models;

#endregion

namespace egrants_new.Controllers
{
    /// <summary>
    /// The appl destructed controller.
    /// </summary>
    public class ApplDestructedController : Controller
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load appl destructed years list
            this.ViewBag.Years = ApplDestructed.LoadYears();

            // load descrip codes list
            this.ViewBag.DescripCodes = ApplDestructed.LoadDescripCodes();

            // load exception codes list
            this.ViewBag.ExceptionCodes = ApplDestructed.LoadExceptionCodes();

            return this.View("~/Egrants_Admin/Views/ApplDestructedIndex.cshtml");
        }

        /// <summary>
        /// The search.
        /// </summary>
        /// <param name="year">
        /// The year.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Search(int year, string status, string exception, string str)
        {
            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load appl destructed years list
            this.ViewBag.Years = ApplDestructed.LoadYears();

            // load descrip codes list
            this.ViewBag.DescripCodes = ApplDestructed.LoadDescripCodes();

            // load exception codes list
            this.ViewBag.ExceptionCodes = ApplDestructed.LoadExceptionCodes();

            // get searching variable
            this.ViewBag.SearchYear = year;

            if (status != string.Empty)
                this.ViewBag.StatusCode = status;

            if (exception != string.Empty)
                this.ViewBag.ExceptionCode = exception;

            if (str != string.Empty)
                this.ViewBag.Str = str;

            // check access permission
            this.ViewBag.Processable = ApplDestructed.CheckPermission(year, Convert.ToString(this.Session["userid"]));

            // load search info
            this.ViewBag.SearchInfo = ApplDestructed.LoadSearchInfo(year, status, exception, str);

            // load appls
            this.ViewBag.Appls = ApplDestructed.LoadAppls(
                string.Empty,
                year,
                status,
                exception,
                str,
                string.Empty,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Admin/Views/ApplDestructedIndex.cshtml");
        }

        /// <summary>
        /// The modify.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="year">
        /// The year.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="id_string">
        /// The id_string.
        /// </param>
        /// <param name="exception_type">
        /// The exception_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Modify(string act, int year, string status, string exception, string str, string id_string, string exception_type)
        {
            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load appl destructed years list
            this.ViewBag.Years = ApplDestructed.LoadYears();

            // get searching variable
            this.ViewBag.SearchYear = year;

            if (status != string.Empty)
            {
                this.ViewBag.StatusCode = status;
            }

            if (exception != string.Empty)
            {
                this.ViewBag.ExceptionCode = exception;
            }

            if (str != string.Empty)
            {
                this.ViewBag.Str = str;
            }

            // modify data and load appls
            this.ViewBag.Appls = ApplDestructed.LoadAppls(
                act,
                year,
                status,
                exception,
                str,
                id_string,
                exception_type,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            // load search info
            this.ViewBag.SearchInfo = ApplDestructed.LoadSearchInfo(year, status, exception, str);

            // load DescripCodes list
            this.ViewBag.DescripCodes = ApplDestructed.LoadDescripCodes();

            // load exception codes list
            this.ViewBag.ExceptionCodes = ApplDestructed.LoadExceptionCodes();

            // check access permission
            this.ViewBag.Processable = ApplDestructed.CheckPermission(year, Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants_Admin/Views/ApplDestructedIndex.cshtml");
        }

        /// <summary>
        /// The show_ exception_ code.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Show_Exception_Code()
        {
            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load exception codes list
            this.ViewBag.ExceptionCodes = ApplDestructed.LoadExceptionCodes();

            return this.View("~/Egrants_Admin/Views/ApplDestructedEdit.cshtml");
        }

        /// <summary>
        /// The edit_ exception_ code.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="detail">
        /// The detail.
        /// </param>
        /// <param name="code">
        /// The code.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Edit_Exception_Code(string act, int id, string detail, string code)
        {
            // act could be create, edit or delete
            ApplDestructed.EditExceptionCode(act, id, detail, code, Convert.ToString(this.Session["ic"]), Convert.ToString(this.Session["userid"]));

            // load admin menu list
            this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

            // load exception codes list
            this.ViewBag.ExceptionCodes = ApplDestructed.LoadExceptionCodes();

            return this.View("~/Egrants_Admin/Views/ApplDestructedEdit.cshtml");
        }
    }
}