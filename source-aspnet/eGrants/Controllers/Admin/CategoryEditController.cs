#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  CategoryEditController.cs
// Solution: egrants_new
// Project:  egrants
// Created: 2025-12-01
// Contributors:
//      - Feroz, Aalyaan (NIH/NCI) [C] - feroza2
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
using eGrants.Services;
using eGrants.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

#endregion

namespace eGrants.Controllers.Admin
{
    /// <summary>
    /// The category edit controller.
    /// </summary>
    public class CategoryEditController : Controller
    {

        private readonly ICommonRepository _commonRepository;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly ICategoryEditService _categoryEditService;

        public CategoryEditController(ICommonRepository commonRepository, 
            ISessionInfoService sessionInfoService, 
            ICategoryEditService categoryEditService)
        {

            _commonRepository = commonRepository;
            _categoryEditService = categoryEditService;
            _sessionInfoService = sessionInfoService;

        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);
            // load admin menu list
            this.ViewBag.AdminMenu = _commonRepository.LoadAdminMenus(sessionInfo.UserId);

            //// Load Common Categroies list
            this.ViewBag.CommonCategories = _categoryEditService.LoadCommonCategories(sessionInfo.Ic);

            //// Load local Categroies list
            this.ViewBag.LocalCategories = _categoryEditService.LoadLocalCategories(sessionInfo.Ic);

            // return View
            this.ViewBag.Message = string.Empty;

            return this.View("~/Views/Admin/CategoryEditIndex.cshtml");
        }

        ///// <summary>
        ///// The to_ move.
        ///// </summary>
        ///// <param name="category_id">
        ///// The category_id.
        ///// </param>
        ///// <returns>
        ///// The <see cref="ActionResult"/>.
        ///// </returns>
        //public ActionResult To_Move(string category_id)
        //{
        //    var act = "remove_out";
        //    var categoryid = Convert.ToInt32(category_id);
        //    var category_name = string.Empty;

        //    this.ViewBag.Message = CategoryEdit.run_db(
        //        act,
        //        categoryid,
        //        category_name,
        //        Convert.ToString(this.Session["ic"]),
        //        Convert.ToString(this.Session["userid"]));

        //    // load admin menu list
        //    this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

        //    // Load Common Categroies list
        //    this.ViewBag.CommonCategroies = CategoryEdit.LoadCommonCategroies(Convert.ToString(this.Session["ic"]));

        //    // Load local Categroies list
        //    this.ViewBag.LocalCategroies = CategoryEdit.LoadLocalCategroies(Convert.ToString(this.Session["ic"]));

        //    // return View
        //    return this.View("~/Egrants_Admin/Views/CategoryEditIndex.cshtml");
        //}

        ///// <summary>
        ///// The to_ add.
        ///// </summary>
        ///// <param name="category_id">
        ///// The category_id.
        ///// </param>
        ///// <returns>
        ///// The <see cref="ActionResult"/>.
        ///// </returns>
        //public ActionResult To_Add(string category_id)
        //{
        //    var act = "add_in";
        //    var categoryid = Convert.ToInt32(category_id);
        //    var category_name = string.Empty;

        //    this.ViewBag.Message = CategoryEdit.run_db(
        //        act,
        //        categoryid,
        //        category_name,
        //        Convert.ToString(this.Session["ic"]),
        //        Convert.ToString(this.Session["userid"]));

        //    // load admin menu list
        //    this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

        //    // Load Common Categroies list
        //    this.ViewBag.CommonCategroies = CategoryEdit.LoadCommonCategroies(Convert.ToString(this.Session["ic"]));

        //    // Load local Categroies list
        //    this.ViewBag.LocalCategroies = CategoryEdit.LoadLocalCategroies(Convert.ToString(this.Session["ic"]));

        //    // return View
        //    return this.View("~/Egrants_Admin/Views/CategoryEditIndex.cshtml");
        //}

        ///// <summary>
        ///// The to_ create.
        ///// </summary>
        ///// <param name="category_name">
        ///// The category_name.
        ///// </param>
        ///// <returns>
        ///// The <see cref="ActionResult"/>.
        ///// </returns>
        //public ActionResult To_Create(string category_name)
        //{
        //    var act = "create_new";
        //    var category_id = 0;

        //    this.ViewBag.Message = CategoryEdit.run_db(
        //        act,
        //        category_id,
        //        category_name,
        //        Convert.ToString(this.Session["ic"]),
        //        Convert.ToString(this.Session["userid"]));

        //    // load admin menu list
        //    this.ViewBag.AdminMenu = EgrantsCommon.LoadAdminMenu(Convert.ToString(this.Session["userid"]));

        //    // Load Common Categroies list
        //    this.ViewBag.CommonCategroies = CategoryEdit.LoadCommonCategroies(Convert.ToString(this.Session["ic"]));

        //    // Load local Categroies list
        //    this.ViewBag.LocalCategroies = CategoryEdit.LoadLocalCategroies(Convert.ToString(this.Session["ic"]));

        //    // return View
        //    return this.View("~/Egrants_Admin/Views/CategoryEditIndex.cshtml");
        //}
    }
}