#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  DocTransactionReportController.cs
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
using System.Linq;
using System.Web.Mvc;

using egrants_new.Functions;
using egrants_new.Models;

#endregion

namespace egrants_new.Controllers.Management
{
    /// <summary>
    /// The doc transaction report controller.
    /// </summary>
    public class DocTransactionReportController : Controller
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            // load egrants specialist list
            this.ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(this.Session["ic"]));

            return this.View("~/Views/Management/DocTransactionReport.cshtml");
        }

        // search by click button
        /// <summary>
        /// The to_ show_ report.
        /// </summary>
        /// <param name="transaction_type">
        /// The transaction_type.
        /// </param>
        /// <param name="person_id">
        /// The person_id.
        /// </param>
        /// <param name="date_range">
        /// The date_range.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Show_Report(string transaction_type, int person_id, string date_range)
        {
            // load egrants specialist list
            this.ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(this.Session["ic"]));

            this.ViewBag.PersonID = person_id;
            this.ViewBag.TransactionType = transaction_type;
            this.ViewBag.DateRange = date_range;

            var start_date = string.Empty;
            var end_date = string.Empty;

            // load docs Transaction history
            this.ViewBag.EgrantsDocs = EgrantsDoc.LoadDocTransactionHistory(
                transaction_type,
                person_id,
                start_date,
                end_date,
                date_range,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"])).AsEnumerable();

            return this.View("~/Views/Management/DocTransactionReport.cshtml");
        }

        /// <summary>
        /// The to_ create_ report.
        /// </summary>
        /// <param name="transaction_type">
        /// The transaction_type.
        /// </param>
        /// <param name="person_id">
        /// The person_id.
        /// </param>
        /// <param name="start_date">
        /// The start_date.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult To_Create_Report(string transaction_type, int person_id, string start_date, string end_date)
        {
            // load egrants specialist list
            this.ViewBag.Specialists = EgrantsCommon.LoadSpecialists(Convert.ToString(this.Session["ic"]));

            this.ViewBag.PersonID = person_id;
            this.ViewBag.TransactionType = transaction_type;
            this.ViewBag.StartDate = start_date;
            this.ViewBag.EndDate = end_date;

            // load docs Transaction history
            this.ViewBag.EgrantsDocs = EgrantsDoc.LoadDocTransactionHistory(
                transaction_type,
                person_id,
                start_date,
                end_date,
                string.Empty,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"])).AsEnumerable();

            return this.View("~/Views/Management/DocTransactionReport.cshtml");
        }
    }
}