#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  SystemReportController.cs
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

using egrants_new.Models;

#endregion

namespace egrants_new.Controllers
{
    /// <summary>
    /// The system report controller.
    /// </summary>
    public class SystemReportController : Controller
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            // load egrants accession list
            this.ViewBag.Accessions = SystemReport.LoadAccessions(Convert.ToString(this.Session["ic"]));

            return this.View("~/Views/Management/SystemReport.cshtml");
        }

        /// <summary>
        /// The by_ serialnum.
        /// </summary>
        /// <param name="serial_number">
        /// The serial_number.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult by_Serialnum(int serial_number)
        {
            var act = "by_serialnumber";
            this.ViewBag.SerialNumber = serial_number;

            // load egrants accession list
            this.ViewBag.Accessions = SystemReport.LoadAccessions(Convert.ToString(this.Session["ic"]));

            // load folders by serial number search
            this.ViewBag.EgrantsFolders = SystemReport.LoadFolders(
                act,
                serial_number,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"])).AsEnumerable();

            return this.View("~/Views/Management/SystemReport.cshtml");
        }

        /// <summary>
        /// The by_ accessionid.
        /// </summary>
        /// <param name="accession_id">
        /// The accession_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult by_Accessionid(int accession_id)
        {
            var act = "by_accessionid";
            this.ViewBag.AccessionID = accession_id;

            // load egrants accession list
            this.ViewBag.Accessions = SystemReport.LoadAccessions(Convert.ToString(this.Session["ic"]));

            // load folders by accession id search
            this.ViewBag.EgrantsFolders = SystemReport.LoadFolders(
                act,
                accession_id,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"])).AsEnumerable();

            return this.View("~/Views/Management/SystemReport.cshtml");
        }
    }
}