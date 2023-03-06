#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EgrantsController.cs
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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using egrants_new.Egrants.Models;
using egrants_new.Models;

using Newtonsoft.Json;

#endregion

namespace egrants_new.Controllers
{
    /// <summary>
    /// The egrants controller.
    /// </summary>
    public class EgrantsController : Controller
    {
        // go to default 
        /// <summary>
        /// The go_to_default.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Go_to_default()
        {
            return this.View("~/Shared/Views/Go_to_Default.cshtml");
        }

        // GET: Egrants
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            // go to dashboard tab if user is a specialist
            // if (Convert.ToInt16(Session["position_id"]) == 2 && Convert.ToInt16(Session["dashboard"]) == 0)
            // {
            // return RedirectToAction("Index", "Dashboard", new { area = "Dashboard" });
            // }else return View("~/Egrants/Views/Index.cshtml");

            // go to ICCoordinator tab if user is a Coordinator
            // else if (Convert.ToInt16(Session["position_id"]) == 0)
            // {   
            // return RedirectToAction("Index", "ICCoordinator", new { area = "IC_Coordinator" });
            // }

            // retuen IC list
            this.ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

            return this.View("~/Egrants/Views/Index.cshtml");
        }

        // get all appls list for appls toggle by grant_id
        /// <summary>
        /// The load all appls.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string LoadAllAppls(int grant_id)
        {
            var list = EgrantsAppl.GetAllAppls(grant_id);

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list); // Serialize(applslist);
        }

        // get 12 appls list for appls toggle by grant_id
        /// <summary>
        /// The load default appls.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string LoadDefaultAppls(int grant_id)
        {
            var list = EgrantsAppl.GetDefaultAppls(grant_id);

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
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string LoadYears(
            string fy = null,
            string mechanism = null,
            string admin_code = null,
            string serial_num = null)
        {
            // string fy, string mechan, s
            var list = Egrants.Models.Egrants.GetYearList(fy, mechanism, admin_code, serial_num);

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
        public string GetAllApplsList(string admin_code, string serial_num)
        {
            // string fy, string mechan, s
            var list = EgrantsAppl.GetAllApplsList(admin_code, serial_num);

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
        public string LoadCategories(int grant_id, string years)
        {
            var list = Egrants.Models.Egrants.GetCategoryList(grant_id, years);

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }

        /// <summary>
        /// The by_str.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult by_str(string str, string mode = null)
        {
            this.ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

            if (string.IsNullOrEmpty(str))
            {
                this.ViewBag.Message = "No data found for the search";
                this.ViewBag.grantlayer = null;
            }
            else
            {
                this.ViewBag.Str = str;
                this.ViewBag.Mode = mode;
                this.ViewBag.CurrentTab = 1;
                this.ViewBag.CurrentPage = 1;
                this.ViewBag.SearchStyle = "by_str";

                // load data           
                Egrants.Models.Egrants.Search.egrants_search(
                    str,
                    0,
                    string.Empty,
                    0,
                    0,
                    Convert.ToString(this.Session["browser"]),
                    Convert.ToString(this.Session["ic"]),
                    Convert.ToString(this.Session["userid"]));

                if (Egrants.Models.Egrants.Search.grantlayerproperty != null)
                {
                    this.ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
                    this.ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
                    this.ViewBag.ApplCount = this.ViewBag.appllayer.Count;
                    this.ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;
                    this.ViewBag.doclayer = Egrants.Models.Egrants.Search.doclayerproperty;
                    this.ViewBag.DocCount = this.ViewBag.doclayer.Count;

                    // show pagination
                    this.ViewBag.Pagination = Egrants.Models.Egrants.LoadPagination(
                        str,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]),
                        string.Empty);
                }
                else
                {
                    this.ViewBag.Message = "No data found for the search";
                    this.ViewBag.grantlayer = null;
                }
            }

            return this.View("~/Egrants/Views/Index.cshtml");
        }

        /// <summary>
        /// The by_grant.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <param name="categories">
        /// The categories.
        /// </param>
        /// <param name="appls_list">
        /// The appls_list.
        /// </param>
        /// <param name="years">
        /// The years.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult by_grant(
            int grant_id = 0,
            string package = null,
            string categories = null,
            string appls_list = null,
            string years = null,
            string mode = null)
        {
            this.ViewBag.ICList = EgrantsCommon.LoadAdminCodes();
            var isexisting = Egrants.Models.Egrants.CheckGrantID(grant_id);

            if (grant_id == 0 || isexisting == 0)
            {
                this.ViewBag.Message = "No data found for the search";
                this.ViewBag.grantlayer = null;
            }
            else
            {
                this.ViewBag.bygrant = 1;
                this.ViewBag.GrantID = grant_id;
                this.ViewBag.Package = package;
                this.ViewBag.Mode = mode;
                this.ViewBag.SearchStyle = "by_grant";
                this.ViewBag.SelectedYears = years;
                this.ViewBag.SelectedCats = categories;

                if (categories == string.Empty || categories == "All" || categories == "all")
                    this.ViewBag.SelectedCategories = "All";
                else if (categories != string.Empty && categories != "All" && categories != "all")
                    this.ViewBag.SelectedCategories = Egrants.Models.Egrants.Get_CategoryName_by_id(categories);

                // load data from DB
                Egrants.Models.Egrants.Search.egrants_search(
                    string.Empty,
                    grant_id,
                    package,
                    0,
                    0,
                    Convert.ToString(this.Session["browser"]),
                    Convert.ToString(this.Session["ic"]),
                    Convert.ToString(this.Session["userid"]));

                this.ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
                this.ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;
                this.ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
                this.ViewBag.ApplCount = this.ViewBag.appllayer.Count;
                this.ViewBag.doclayer = Egrants.Models.Egrants.Search.doclayerproperty;
                this.ViewBag.DocCount = this.ViewBag.doclayer.Count;

                // set appls_lis for searching by flag_type
                if (package != string.Empty && package != "All" && package != "all")
                    appls_list = EgrantsAppl.GetApplsList(grant_id, package);

                // set appls_lis for searching by years
                if (years != string.Empty)
                {
                    if (years == "all" || years == "All")
                        appls_list = "All";
                    else
                        appls_list = EgrantsAppl.GetApplsList(grant_id, null, years);
                }

                this.ViewBag.SelectedAppls = appls_list;

                // reset appllayer and limit show appls if appls_list with search parameters
                if (appls_list != null && appls_list != "All" && appls_list != "all")
                {
                    var appllist = new List<Egrants.Models.Egrants.appllayer>();

                    // for more than one appl
                    if (appls_list.IndexOf(',') > 1)
                    {
                        var app = appls_list.Split(',').ToList();

                        // List<Egrants.Models.Egrants.appllayer> appllist = new List<Egrants.Models.Egrants.appllayer>();
                        foreach (var appl in this.ViewBag.appllayer)
                            if (app.Any(n => n == appl.appl_id))
                                appllist.Add(appl);

                        this.ViewBag.appllayer = appllist;
                    }

                    // for only one appl
                    else
                    {
                        // ViewBag.ApplID = appls_list;
                        var app = appls_list.Split().ToList();

                        // List<Egrants.Models.Egrants.appllayer> appllist = new List<Egrants.Models.Egrants.appllayer>();
                        foreach (var appl in this.ViewBag.appllayer)
                            if (app.Any(n => n == appl.appl_id))
                                appllist.Add(appl);

                        this.ViewBag.appllayer = appllist;
                    }
                }
            }

            return this.View("~/Egrants/Views/Index.cshtml");
        }

        /// <summary>
        /// The by_appl.
        /// </summary>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult by_appl(int appl_id = 0, string mode = null, string str = null)
        {
            this.ViewBag.ICList = EgrantsCommon.LoadAdminCodes();
            var isexisting = EgrantsAppl.CheckApplID(appl_id);

            if (appl_id == 0 || isexisting == 0)
            {
                this.ViewBag.Message = "No data found for the search";
                this.ViewBag.grantlayer = null;
            }
            else
            {
                // ViewBag.YearList = Egrants.Models.Egrants.P2_getYearList();
                if (str != null)
                    this.ViewBag.Str = str;

                this.ViewBag.Mode = mode;
                this.ViewBag.SearchStyle = "by_appl";
                this.ViewBag.ApplID = appl_id;
                this.ViewBag.GrantID = Egrants.Models.Egrants.GetGrantID(appl_id);
                this.ViewBag.SelectedCats = "All";
                this.ViewBag.SelectedCategories = "All";
                this.ViewBag.SelectedAppls = appl_id.ToString();

                // load data from DB
                Egrants.Models.Egrants.Search.egrants_search(
                    string.Empty,
                    0,
                    string.Empty,
                    appl_id,
                    0,
                    Convert.ToString(this.Session["browser"]),
                    Convert.ToString(this.Session["ic"]),
                    Convert.ToString(this.Session["userid"]));

                this.ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
                this.ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
                this.ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;
                this.ViewBag.ApplCount = this.ViewBag.appllayer.Count;
                this.ViewBag.doclayer = Egrants.Models.Egrants.Search.doclayerproperty;
                this.ViewBag.DocCount = this.ViewBag.doclayer.Count;

                // ViewBag.doclayer_All = ViewBag.doclayer;--commented by leon 4/1/2019
            }

            return this.View("~/Egrants/Views/Index.cshtml");
        }

        /// <summary>
        /// The by_qc.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult by_qc(string str = null)
        {
            this.ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

            // if (str == null || str == "")
            // {
            // ViewBag.Message = "No data found for the search";
            // ViewBag.grantlayer = null;
            // }
            // else
            // {
            this.ViewBag.str = "qc";
            this.ViewBag.Mode = "qc";

            // ViewBag.DocSort = "date";
            this.ViewBag.CurrentTab = 1;
            this.ViewBag.CurrentPage = 1;
            this.ViewBag.SearchStyle = "by_qc";

            // load data
            Egrants.Models.Egrants.Search.egrants_search(
                "qc",
                0,
                string.Empty,
                0,
                1,
                Convert.ToString(this.Session["browser"]),
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            this.ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
            this.ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
            this.ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;
            this.ViewBag.ApplCount = this.ViewBag.appllayer.Count;
            this.ViewBag.doclayer = Egrants.Models.Egrants.Search.doclayerproperty;
            this.ViewBag.DocCount = this.ViewBag.doclayer.Count;

            this.ViewBag.Pagination = Egrants.Models.Egrants.LoadPagination(
                "qc",
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]),
                string.Empty);

            this.ViewBag.UnidentifiedDocs = EgrantsDoc.LoadDocsUnidentified(
                Convert.ToString(this.Session["ImageServer"]),
                Convert.ToString(this.Session["userid"]));

            // }
            return this.View("~/Egrants/Views/Index.cshtml");
        }

        /// <summary>
        /// The by_filters.
        /// </summary>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism.
        /// </param>
        /// <param name="admincode">
        /// The admincode.
        /// </param>
        /// <param name="serialnum">
        /// The serialnum.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult by_filters(int fy = 0, string mechanism = null, string admincode = null, int serialnum = 0)
        {
            this.ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

            if (fy == 0 && string.IsNullOrEmpty(mechanism) && serialnum == 0) /*string.IsNullOrEmpty(admincode) &&*/
            {
                this.ViewBag.Message = "No data found for the search";
                this.ViewBag.grantlayer = null;
            }
            else
            {
                var package = "by_filters";
                this.ViewBag.SearchStyle = "by_filters";
                this.ViewBag.CurrentTab = 1;
                this.ViewBag.CurrentPage = 1;

                // create return value
                if (fy != 0)
                    this.ViewBag.FilterFY = fy;
                else
                    this.ViewBag.FilterFY = string.Empty;

                if (serialnum != 0)
                    this.ViewBag.FilterSerialNumber = serialnum;

                this.ViewBag.FilterMechanism = mechanism;
                this.ViewBag.FilterAdminCode = admincode;

                // create filters search sql query
                var FilterSearchQuery = Egrants.Models.Egrants.GetSearchQuery(
                    fy,
                    mechanism,
                    admincode,
                    serialnum,
                    1,
                    Convert.ToString(this.Session["browser"]),
                    Convert.ToString(this.Session["ic"]),
                    Convert.ToString(this.Session["userid"]));

                Egrants.Models.Egrants.Search.egrants_search(
                    FilterSearchQuery,
                    0,
                    package,
                    0,
                    0,
                    Convert.ToString(this.Session["browser"]),
                    Convert.ToString(this.Session["ic"]),
                    Convert.ToString(this.Session["userid"]));

                if (Egrants.Models.Egrants.Search.grantlayerproperty != null)
                {
                    this.ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
                    this.ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
                    this.ViewBag.ApplCount = this.ViewBag.appllayer.Count;
                    this.ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;

                    // show pagination
                    this.ViewBag.Pagination = Egrants.Models.Egrants.LoadPagination(
                        FilterSearchQuery,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]),
                        package);
                }
                else
                {
                    this.ViewBag.Message = "No data found for the search";
                    this.ViewBag.grantlayer = null;
                }
            }

            return this.View("~/Egrants/Views/Index.cshtml");
        }

        /// <summary>
        /// The by_filters_page.
        /// </summary>
        /// <param name="tab_num">
        /// The tab_num.
        /// </param>
        /// <param name="page_num">
        /// The page_num.
        /// </param>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism.
        /// </param>
        /// <param name="admincode">
        /// The admincode.
        /// </param>
        /// <param name="serialnum">
        /// The serialnum.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult by_filters_page(
            int tab_num = 0,
            int page_num = 0,
            string package = null,
            int fy = 0,
            string mechanism = null,
            string admincode = null,
            int serialnum = 0)
        {
            this.ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

            /*string.IsNullOrEmpty(admincode) &&*/
            if (fy == 0 && string.IsNullOrEmpty(mechanism) && serialnum == 0)
            {
                this.ViewBag.Message = "No data found for the search";
                this.ViewBag.grantlayer = null;
            }
            else if (tab_num == 0 || page_num == 0 || string.IsNullOrEmpty(package) || package != "by_filters")
            {
                this.ViewBag.Message = "No data found for the search";
                this.ViewBag.grantlayer = null;
            }
            else
            {
                this.ViewBag.SearchStyle = package;
                this.ViewBag.CurrentTab = tab_num;
                this.ViewBag.CurrentPage = page_num;

                // create return value
                if (fy != 0)
                    this.ViewBag.FilterFY = fy;
                else
                    this.ViewBag.FilterFY = string.Empty;

                this.ViewBag.FilterMechanism = mechanism;
                this.ViewBag.FilterAdminCode = admincode;

                if (serialnum != 0)
                    this.ViewBag.FilterSerialNumber = serialnum;

                // create filters search sql query
                var FilterSearchQuery = Egrants.Models.Egrants.GetSearchQuery(
                    fy,
                    mechanism,
                    admincode,
                    serialnum,
                    page_num,
                    Convert.ToString(this.Session["browser"]),
                    Convert.ToString(this.Session["ic"]),
                    Convert.ToString(this.Session["userid"]));

                // load data
                Egrants.Models.Egrants.Search.egrants_search(
                    FilterSearchQuery,
                    0,
                    package,
                    0,
                    page_num,
                    Convert.ToString(this.Session["browser"]),
                    Convert.ToString(this.Session["ic"]),
                    Convert.ToString(this.Session["userid"]));

                this.ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
                this.ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
                this.ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;
                this.ViewBag.ApplCount = this.ViewBag.appllayer.Count;

                // show Pagination 
                this.ViewBag.Pagination = Egrants.Models.Egrants.LoadPagination(
                    FilterSearchQuery,
                    Convert.ToString(this.Session["ic"]),
                    Convert.ToString(this.Session["userid"]),
                    package);
            }

            return this.View("~/Egrants/Views/Index.cshtml");
        }

        /// <summary>
        /// The by_page.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="tab_num">
        /// The tab_num.
        /// </param>
        /// <param name="page_num">
        /// The page_num.
        /// </param>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult by_page(string str = null, int tab_num = 0, int page_num = 0, string package = null, string mode = null)
        {
            this.ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

            if (string.IsNullOrEmpty(str))
            {
                this.ViewBag.Message = "No data found for the search";
                this.ViewBag.grantlayer = null;
            }
            else if (page_num == 0 || tab_num == 0)
            {
                this.ViewBag.Message = "No data found for the search";
                this.ViewBag.grantlayer = null;
            }
            else
            {
                this.ViewBag.SearchStyle = "by_page";
                this.ViewBag.CurrentTab = tab_num;
                this.ViewBag.CurrentPage = page_num;
                this.ViewBag.Str = str;
                this.ViewBag.Mode = mode;

                Egrants.Models.Egrants.Search.egrants_search(
                    str,
                    0,
                    string.Empty,
                    0,
                    page_num,
                    Convert.ToString(this.Session["browser"]),
                    Convert.ToString(this.Session["ic"]),
                    Convert.ToString(this.Session["userid"]));

                this.ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
                this.ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
                this.ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;
                this.ViewBag.ApplCount = this.ViewBag.appllayer.Count;
                this.ViewBag.doclayer = Egrants.Models.Egrants.Search.doclayerproperty;
                this.ViewBag.DocCount = this.ViewBag.doclayer.Count;

                if (str == "qc")
                    this.ViewBag.Mode = "qc";

                // show Pagination 
                this.ViewBag.Pagination = Egrants.Models.Egrants.LoadPagination(
                    str,
                    Convert.ToString(this.Session["ic"]),
                    Convert.ToString(this.Session["userid"]),
                    package);

                if (str == "qc")
                    this.ViewBag.UnidentifiedDocs = EgrantsDoc.LoadDocsUnidentified(
                        Convert.ToString(this.Session["ImageServer"]),
                        Convert.ToString(this.Session["userid"]));
            }

            return this.View("~/Egrants/Views/Index.cshtml");
        }

        // Autocomplete for fy, activity_code and serial_number
        /// <summary>
        /// The load_data_autocomplete.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="term">
        /// The term.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism.
        /// </param>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <param name="admincode">
        /// The admincode.
        /// </param>
        /// <param name="serialnum">
        /// The serialnum.
        /// </param>
        /// <returns>
        /// The <see cref="JsonResult"/>.
        /// </returns>
        public JsonResult load_data_autocomplete(
            string type,
            string term,
            string mechanism = null,
            string fy = null,
            string admincode = null,
            string serialnum = null)
        {
            var sql_query = string.Empty;

            // List<string> data_list = new List<string>();
            if (admincode != null && admincode != string.Empty)
                this.ViewBag.admincode = admincode;

            if (admincode == "undefined")
                this.ViewBag.admincode = string.Empty;
            else
                this.ViewBag.admincode = string.Empty;

            this.ViewBag.FilterFY = fy;

            this.ViewBag.FilterSerialNumber = serialnum;

            this.ViewBag.FilterMechanism = mechanism;

            this.ViewBag.FilterAdminCode = admincode;
            this.ViewBag.ICList = EgrantsCommon.LoadAdminCodes();
            var data_list = new List<string>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                // if (type == "fy")
                // {
                // sql_query = "sp_web_egrants_load_data_autocomplete";
                // }
                if (type == "mechanism")
                    sql_query = "sp_web_egrants_load_data_autocomplete_mechanism";

                if (type == "serialnum")
                    sql_query = "sp_web_egrants_load_data_autocomplete_serialnum";

                if (type == "fy")
                    sql_query = "sp_web_egrants_load_data_autocomplete_fy";

                var cmd = new SqlCommand(sql_query, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@term", term);
                cmd.Parameters.AddWithValue("@fy", fy);
                cmd.Parameters.AddWithValue("@mechanism", mechanism);
                cmd.Parameters.AddWithValue("@admincode", admincode);
                cmd.Parameters.AddWithValue("@serialnum", serialnum);
                conn.Open();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    data_list.Add(rdr[0].ToString());

                // sql_query = rdr[0].ToString();
            }

            return this.Json(data_list, JsonRequestBehavior.AllowGet);
        }

        // load documents by appl_id
        /// <summary>
        /// The load docs grid.
        /// </summary>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="search_type">
        /// The search_type.
        /// </param>
        /// <param name="category_list">
        /// The category_list.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="JsonResult"/>.
        /// </returns>
        public JsonResult LoadDocsGrid(int appl_id, string search_type = null, string category_list = null, string mode = null)
        {
            Egrants.Models.Egrants.Search_by_appl_id.LoadDocs(
                appl_id,
                search_type,
                category_list,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            this.ViewBag.doclayer = Egrants.Models.Egrants.Search_by_appl_id.doclayerproperty;

            // ViewBag.doclayer = Egrants.Models.Egrants.Search_by_appl_id.doclayerproperty.ToList();
            dynamic res = new { data = this.ViewBag.doclayer };

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The stop_notice.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult stop_notice(int grant_id)
        {
            this.ViewBag.StopNotice = Egrants.Models.Egrants.LoadStopNotice(grant_id, Convert.ToString(this.Session["ic"]));

            return this.View("~/Egrants/Views/_Modal_Stop_Notice.cshtml");
        }

        /// <summary>
        /// The supplement.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult supplement(int grant_id)
        {
            var act = "to_view";

            this.ViewBag.StopNotice = Egrants.Models.Egrants.LoadSupplement(
                act,
                grant_id,
                0,
                string.Empty,
                string.Empty,
                0,
                Convert.ToString(this.Session["ic"]),
                Convert.ToString(this.Session["userid"]));

            return this.View("~/Egrants/Views/_Modal_Supplement.cshtml");
        }

        /// <summary>
        /// Log out the user (clean up cookies, session, etc).
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LogOut() {
            //this.Session.Remove("NIHSMSESSION");

            if (System.Web.HttpContext.Current != null)
            {                
                int requestCookieCount = System.Web.HttpContext.Current.Request.Cookies.Count;
                
                bool found = false;
                for (var i = 0; i < requestCookieCount; i++) {
                    var cookie = System.Web.HttpContext.Current.Request.Cookies[i];
                    if (cookie != null) {
                        if (!cookie.Name.Contains("NIHSMSESSION"))
                            continue;
                        /*var expiredCookie = new HttpCookie(cookie.Name)
                        {
                            Expires = DateTime.Now.AddDays(-1),
                            Domain = cookie.Domain,
                            Value = null
                        };
                        System.Web.HttpContext.Current.Request.Cookies.Add(expiredCookie);*/
                        //cookie.Name = "NIHSMSESSION_LOGGEDOFF";
                        cookie.Value = "LOGGEDOFF";
                        found = true;
                    }
                }

                int responseCookieCount = System.Web.HttpContext.Current.Response.Cookies.Count;
                for (var i = 0; i < responseCookieCount; i++)
                {
                    var cookie = System.Web.HttpContext.Current.Response.Cookies[i];
                    if (cookie != null)
                    {
                        if (!cookie.Name.Contains("NIHSMSESSION"))
                            continue;
                        /*var expiredCookie = new HttpCookie(cookie.Name)
                        {
                            Expires = DateTime.Now.AddDays(-1),
                            Domain = cookie.Domain,
                            Value = null
                        };
                        System.Web.HttpContext.Current.Request.Cookies.Add(expiredCookie);*/
                        //cookie.Name = "NIHSMSESSION_LOGGEDOFF";
                        cookie.Value = "LOGGEDOFF";
                        found = true;
                    }
                }
                if (!found)
                    System.Web.HttpContext.Current.Request.Cookies.Remove("NIHSMSESSION");
            }

            Session["NIHSESSION"] = "LOGGEDOFF";

            /*var key = Session.Contents.;
            if (Session.Keys["NIHSMSESSION"] != null)
            {
                var sessionKey = Session.Keys["NI"]
            }*/

            // session lets you remove but not rename
//            this.Session.Remove("NIHSMSESSION");
//            this.Session.Re

            MvcApplication.GetMvcApplication().LogOut();

            /*this.Session.RemoveAll();
            var response = HttpContext.Response;
            if (response.Cookies.Count > 0)
            {
                response.Cookies.Clear();
                response.Clear();
            }*/

            return Json(@"{ ""message"": ""logout complete."" }");
        }

        /// <summary>
        /// ASP.NET session timeouts are based on time between requests,
        /// so send an additional request to this to explicitly keep session alive
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RenewSession()
        {
            return Json(@"{ ""message"": ""session renewed."" }");
        }
    }
}