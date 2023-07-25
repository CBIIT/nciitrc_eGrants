#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  ManagementController.cs
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

using egrants_new.Models;

#endregion

namespace egrants_new.Controllers
{
    /// <summary>
    /// The management controller.
    /// </summary>
    public class ManagementController : Controller
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            // load egrants qc reasons
            this.ViewBag.QCReasons = QCAssignment.LoadQCReasons(Convert.ToString(this.Session["ic"]));

            // load egrants specialist list
            this.ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(this.Session["ic"]));

            // load qc persons list
            this.ViewBag.QCPersons = QCAssignment.LoadQCPersons(Convert.ToString(this.Session["ic"]));

            // load qc report
            this.ViewBag.QCReport = QCAssignment.LoadQCReport(Convert.ToString(this.Session["ic"]));

            return this.View("~/Views/Management/Index.cshtml");
        }

        /// <summary>
        /// The to_ assign.
        /// </summary>
        /// <param name="qc_reason">
        /// The qc_reason.
        /// </param>
        /// <param name="qc_person_id">
        /// The qc_person_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Assign(string qc_reason, string qc_person_id)
        {
            var act = "to_assign";
            var qcperson_id = Convert.ToInt32(qc_person_id);
            var person_id = 0;
            var percent = 0;

            QCAssignment.run_db(
                act,
                qcperson_id,
                qc_reason,
                percent,
                person_id,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            // load egrants qc reasons
            this.ViewBag.QCReasons = QCAssignment.LoadQCReasons(Convert.ToString(this.Session["ic"]));

            // load egrants specialist list
            this.ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(this.Session["ic"]));

            // load qc persons list
            this.ViewBag.QCPersons = QCAssignment.LoadQCPersons(Convert.ToString(this.Session["ic"]));

            // load qc report
            this.ViewBag.QCReport = QCAssignment.LoadQCReport(Convert.ToString(this.Session["ic"]));

            // return View("~/Views/Management/Index.cshtml");
            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// The to_ remove.
        /// </summary>
        /// <param name="qc_reason">
        /// The qc_reason.
        /// </param>
        /// <param name="qc_person_id">
        /// The qc_person_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Remove(string qc_reason, string qc_person_id)
        {
            var act = "to_remove";
            var qcperson_id = Convert.ToInt32(qc_person_id);
            var person_id = 0;
            var percent = 0;

            QCAssignment.run_db(
                act,
                qcperson_id,
                qc_reason,
                percent,
                person_id,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            // load egrants qc reasons
            this.ViewBag.QCReasons = QCAssignment.LoadQCReasons(Convert.ToString(this.Session["ic"]));

            // load egrants specialist list
            this.ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(this.Session["ic"]));

            // load qc persons list
            this.ViewBag.QCPersons = QCAssignment.LoadQCPersons(Convert.ToString(this.Session["ic"]));

            // load qc report
            this.ViewBag.QCReport = QCAssignment.LoadQCReport(Convert.ToString(this.Session["ic"]));

            // return View("~/Views/Management/Index.cshtml");
            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// The to_ route.
        /// </summary>
        /// <param name="person_id">
        /// The person_id.
        /// </param>
        /// <param name="percent">
        /// The percent.
        /// </param>
        /// <param name="qc_person_id">
        /// The qc_person_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Route(string person_id, string percent, string qc_person_id)
        {
            var act = "to_route";
            var qcperson_id = Convert.ToInt32(qc_person_id);
            var personid = Convert.ToInt32(person_id);
            var percents = Convert.ToInt32(percent);
            var qc_reason = string.Empty;

            QCAssignment.run_db(
                act,
                qcperson_id,
                qc_reason,
                percents,
                personid,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            // load egrants qc reasons
            this.ViewBag.QCReasons = QCAssignment.LoadQCReasons(Convert.ToString(this.Session["ic"]));

            // load egrants specialist list
            this.ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(this.Session["ic"]));

            // load qc persons list
            this.ViewBag.QCPersons = QCAssignment.LoadQCPersons(Convert.ToString(this.Session["ic"]));

            // load qc report
            this.ViewBag.QCReport = QCAssignment.LoadQCReport(Convert.ToString(this.Session["ic"]));

            // return View("~/Views/Management/Index.cshtml");
            return this.RedirectToAction("Index");
        }
    }
}