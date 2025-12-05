#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  ApplDestructedController.cs
// Solution: egrants_new
// Project:  egrants
// Created: 2025-12-05
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

using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

#endregion

namespace egrants.Controllers
{
    /// <summary>
    /// The appl destructed controller.
    /// </summary>
    public class ApplDestructedController : Controller
    {
        //
        private readonly ICommonRepository _commonRepository;
        private readonly ICommonService _commonService;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly IApplDestructedService _applDestructedService;

        public ApplDestructedController(ICommonRepository commonRepository, ICommonService commonService,
            ISessionInfoService sessionInfoService, IApplDestructedService applDestructedService)
        {
            _commonRepository = commonRepository;
            _commonService = commonService;
            _sessionInfoService = sessionInfoService;
            _applDestructedService = applDestructedService;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="page">The current page number for pagination (default: 1).</param>
        /// <param name="sortColumn">Optional. The column name to sort by.</param>
        /// <param name="sortDirection">Sort direction: 'asc' or 'desc' (default: 'asc').</param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load appl destructed years list
            ViewBag.Years = _applDestructedService.LoadYears();

            // load descrip codes list
            ViewBag.DescripCodes = _applDestructedService.LoadDescripCodes();

            // load exception codes list
            ViewBag.ExceptionCodes = _applDestructedService.LoadExceptionCodes();

            return View("~/Views/Admin/ApplDestructedIndex.cshtml");
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
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load appl destructed years list
            ViewBag.Years = _applDestructedService.LoadYears();

            // load descrip codes list
            ViewBag.DescripCodes = _applDestructedService.LoadDescripCodes();

            // load exception codes list
            ViewBag.ExceptionCodes = _applDestructedService.LoadExceptionCodes();

            // get searching variable
            ViewBag.SearchYear = year;

            if (status != string.Empty)
            {
                ViewBag.StatusCode = status;
            }

            if (exception != string.Empty)
            {
                ViewBag.ExceptionCode = exception;
            }

            if (str != string.Empty)
            {
                ViewBag.Str = str;
            }

            // check access permission
            ViewBag.Processable = _applDestructedService.CheckPermission(year, sessionInfo.UserId);

            // load search info
            ViewBag.SearchInfo = _applDestructedService.LoadSearchInfo(year, status, exception, str);

            // load appls
            ViewBag.Appls = _applDestructedService.LoadAppls(
                string.Empty,
                year,
                status,
                exception,
                str,
                string.Empty,
                string.Empty,
                sessionInfo.Ic,
                sessionInfo.UserId);

            return View("~/Views/Admin/ApplDestructedIndex.cshtml");
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
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load appl destructed years list
            ViewBag.Years = _applDestructedService.LoadYears();

            // get searching variable
            ViewBag.SearchYear = year;

            if (status != string.Empty)
            {
                ViewBag.StatusCode = status;
            }

            if (exception != string.Empty)
            {
                ViewBag.ExceptionCode = exception;
            }

            if (str != string.Empty)
            {
                ViewBag.Str = str;
            }

            // modify data and load appls
            ViewBag.Appls = _applDestructedService.LoadAppls(
                act,
                year,
                status,
                exception,
                str,
                id_string,
                exception_type,
                sessionInfo.Ic,
                sessionInfo.UserId);

            // load search info
            ViewBag.SearchInfo = _applDestructedService.LoadSearchInfo(year, status, exception, str);

            // load DescripCodes list
            ViewBag.DescripCodes = _applDestructedService.LoadDescripCodes();

            // load exception codes list
            ViewBag.ExceptionCodes = _applDestructedService.LoadExceptionCodes();

            // check access permission
            ViewBag.Processable = _applDestructedService.CheckPermission(year, sessionInfo.UserId);

            return View("~/Views/Admin/ApplDestructedIndex.cshtml");
        }

        /// <summary>
        /// The show_ exception_ code.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Show_Exception_Code()
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load exception codes list
            ViewBag.ExceptionCodes = _applDestructedService.LoadExceptionCodes();

            return View("~/Views/Admin/ApplDestructedEdit.cshtml");
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
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            // act could be create, edit or delete
            _applDestructedService.EditExceptionCode(act, id, detail, code, sessionInfo.Ic, sessionInfo.UserId);

            // load admin menu list
            ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            // load exception codes list
            ViewBag.ExceptionCodes = _applDestructedService.LoadExceptionCodes();

            return View("~/Views/Admin/ApplDestructedEdit.cshtml");
        }
    }
}