#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  ManagementController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2025-11-24
// Contributors:
//      - Dehuff, Daryl (NIH/NCI) [C] - dehuffdc
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

using eGrants.Models;
using eGrants.Services;
using eGrants.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

#endregion

namespace eGrants.Controllers.Management
{
    /// <summary>
    /// The management controller.
    /// </summary>
    public class ManagementController : Controller
    {

        // Injected dependencies: database context and product service

        private readonly IManagementService _managementService;
        private readonly ISessionInfoService _sessionInfoService = new SessionInfoService();
        private SessionInfo sessionInfo => _sessionInfoService.GetSessionInfo(HttpContext.Session);

        public ManagementController(IManagementService managementService, ISessionInfoService sessionInfoService)
        {
            _managementService = managementService;
            _sessionInfoService = sessionInfoService;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> Index()
        {
            // load egrants qc reasons
            ViewBag.QCReasons = await _managementService.LoadQCReasons(Convert.ToString(sessionInfo.Ic));

            // load egrants specialist list
            ViewBag.Specialists = await _managementService.LoadSpecialists(Convert.ToString(sessionInfo.Ic));

            // load qc persons list
            ViewBag.QCPersons = await _managementService.LoadQCPersons(Convert.ToString(sessionInfo.Ic));

            // load qc report
            ViewBag.QCReport = await _managementService.LoadQCReport(Convert.ToString(sessionInfo.Ic));

            //return this.View("~Views/Management/Index.cshtml");
            return View("~/Views/Management/Index.cshtml");
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
        public async Task<ActionResult> To_Assign(string qc_reason, string qc_person_id)
        {
            var act = "to_assign";
            var qcperson_id = Convert.ToInt32(qc_person_id);
            var person_id = 0;
            var percent = 0;

            _managementService.run_db(
                act,
                qcperson_id,
                qc_reason,
                percent,
                person_id,
                Convert.ToString(sessionInfo.Ic),
                Convert.ToString(sessionInfo.UserId));

            // load egrants qc reasons
            ViewBag.QCReasons = await _managementService.LoadQCReasons(Convert.ToString(sessionInfo.Ic));

            // load egrants specialist list
            ViewBag.Specialists = await _managementService.LoadSpecialists(Convert.ToString(sessionInfo.Ic));

            // load qc persons list
            ViewBag.QCPersons = await _managementService.LoadQCPersons(Convert.ToString(sessionInfo.Ic));

            // load qc report
            ViewBag.QCReport = await _managementService.LoadQCReport(Convert.ToString(sessionInfo.Ic));

            return RedirectToAction("Index");
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
        public async Task<ActionResult> To_Remove(string qc_reason, string qc_person_id)
        {
            var act = "to_remove";
            var qcperson_id = Convert.ToInt32(qc_person_id);
            var person_id = 0;
            var percent = 0;

            _managementService.run_db(
                act,
                qcperson_id,
                qc_reason,
                percent,
                person_id,
                Convert.ToString(sessionInfo.Ic),
                Convert.ToString(sessionInfo.UserId));

            // load egrants qc reasons
            ViewBag.QCReasons = await _managementService.LoadQCReasons(Convert.ToString(sessionInfo.Ic));

            // load egrants specialist list
            ViewBag.Specialists = await _managementService.LoadSpecialists(Convert.ToString(sessionInfo.Ic));

            // load qc persons list
            ViewBag.QCPersons = await _managementService.LoadQCPersons(Convert.ToString(sessionInfo.Ic));

            // load qc report
            ViewBag.QCReport = await _managementService.LoadQCReport(Convert.ToString(sessionInfo.Ic));

            return RedirectToAction("Index");
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
        public async Task<ActionResult> To_Route(string person_id, string percent, string qc_person_id)
        {
            var act = "to_route";
            var qcperson_id = Convert.ToInt32(qc_person_id);
            var personid = Convert.ToInt32(person_id);
            var percents = Convert.ToInt32(percent);
            var qc_reason = string.Empty;

            _managementService.run_db(
                act,
                qcperson_id,
                qc_reason,
                percents,
                personid,
                Convert.ToString(sessionInfo.Ic),
                Convert.ToString(sessionInfo.UserId));

            // load egrants qc reasons
            ViewBag.QCReasons = await _managementService.LoadQCReasons(Convert.ToString(sessionInfo.Ic));

            // load egrants specialist list
            ViewBag.Specialists = await _managementService.LoadSpecialists(Convert.ToString(sessionInfo.Ic));

            // load qc persons list
            ViewBag.QCPersons = await _managementService.LoadQCPersons(Convert.ToString(sessionInfo.Ic));

            // load qc report
            ViewBag.QCReport = await _managementService.LoadQCReport(Convert.ToString(sessionInfo.Ic));

            return RedirectToAction("Index");
        }
    }
}