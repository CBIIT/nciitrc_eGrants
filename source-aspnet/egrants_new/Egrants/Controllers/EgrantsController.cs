using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using egrants_new.Models;
using egrants_new.Egrants.Models;
//using System.Text.Json;

using Newtonsoft.Json;

namespace egrants_new.Controllers
{
    public class EgrantsController : Controller
    {       
        // go to default 
        public ActionResult Go_to_default()
        {
            return View("~/Shared/Views/Go_to_Default.cshtml");
        }

        // GET: Egrants
        public ActionResult Index()
        {
            //go to dashboard tab if user is a specialist
            //if (Convert.ToInt16(Session["position_id"]) == 2 && Convert.ToInt16(Session["dashboard"]) == 0)
            //{
            //    return RedirectToAction("Index", "Dashboard", new { area = "Dashboard" });
            //}else return View("~/Egrants/Views/Index.cshtml");

            //go to ICCoordinator tab if user is a Coordinator
            //else if (Convert.ToInt16(Session["position_id"]) == 0)
            //{   
            //    return RedirectToAction("Index", "ICCoordinator", new { area = "IC_Coordinator" });
            //}

            //retuen IC list
            ViewBag.ICList = EgrantsCommon.LoadAdminCodes();
            return View("~/Egrants/Views/Index.cshtml");                                           
        }

        //get all appls list for appls toggle by grant_id
        public string LoadAllAppls(int grant_id)
        {
            List<string> list = EgrantsAppl.GetAllAppls(grant_id);
            
            //JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);//Serialize(applslist);
        }

        //get 12 appls list for appls toggle by grant_id
        public string LoadDefaultAppls(int grant_id)
        {
            List<string> list = EgrantsAppl.GetDefaultAppls(grant_id);
            //JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }

        //get appls list with documents by (admin_code and serial_num) commented out by Leon at 3/15/2019
        //public string LoadYears(string admin_code, string serial_num)   //string fy, string mechan, s
        //{
        //    List<string> yearlist = Egrants.Models.Egrants.GetYearList(admin_code, serial_num);
        //    JavaScriptSerializer js = new JavaScriptSerializer();
        //    return js.Serialize(yearlist);           
        //}

        //get appls list with documents by (admin_code and serial_num) added by Ayu at 3/15/2019
        public string LoadYears(string fy = null, string mechanism = null, string admin_code = null, string serial_num = null)   //string fy, string mechan, s
        {
            List<string> list = Egrants.Models.Egrants.GetYearList(fy, mechanism, admin_code, serial_num);
            //JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }

        //load all appls list with or without documents
        public string GetAllApplsList(string admin_code, string serial_num)   //string fy, string mechan, s
        {
            List<string> list = Egrants.Models.EgrantsAppl.GetAllApplsList(admin_code, serial_num);
            //JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }
        
        //get category list by grant_id and years
        public string LoadCategories(int grant_id, string years)
        {
            List<string> list = Egrants.Models.Egrants.GetCategoryList(grant_id, years);
            //JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }

        public ActionResult by_str(string str, string mode = null)
        {
            ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

            if (string.IsNullOrEmpty(str))
            {
                ViewBag.Message = "No data found for the search";
                ViewBag.grantlayer = null;
            }
            else {  
                ViewBag.Str = str;
                ViewBag.Mode = mode;
                ViewBag.CurrentTab = 1;
                ViewBag.CurrentPage = 1;
                ViewBag.SearchStyle = "by_str";
           
                //load data           
                Egrants.Models.Egrants.Search.egrants_search(str, 0, "", 0, 0, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                if (Egrants.Models.Egrants.Search.grantlayerproperty != null)
                {
                    ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
                    ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
                    ViewBag.ApplCount = ViewBag.appllayer.Count;
                    ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;
                    ViewBag.doclayer = Egrants.Models.Egrants.Search.doclayerproperty;
                    ViewBag.DocCount = ViewBag.doclayer.Count;

                    //show pagination
                    ViewBag.Pagination = Egrants.Models.Egrants.LoadPagination(str, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]), "");
                }
                else
                {
                    ViewBag.Message = "No data found for the search";
                    ViewBag.grantlayer = null;
                }
            }
            return View("~/Egrants/Views/Index.cshtml");
        }
   
        public ActionResult by_grant(int grant_id = 0, string package=null, string categories=null, string appls_list=null, string years=null, string mode=null)
        {
            ViewBag.ICList = EgrantsCommon.LoadAdminCodes();
            var isexisting = Egrants.Models.Egrants.CheckGrantID(grant_id);

            if (grant_id == 0 || isexisting == 0)
            {
                ViewBag.Message = "No data found for the search";
                ViewBag.grantlayer = null;
            }
            else
            {
                ViewBag.bygrant = 1;
                ViewBag.GrantID = grant_id;
                ViewBag.Package = package;
                ViewBag.Mode = mode;
                ViewBag.SearchStyle = "by_grant";
                ViewBag.SelectedYears = years;
                ViewBag.SelectedCats = categories;

                if (categories == "" || categories == "All" || categories == "all")
                {
                    ViewBag.SelectedCategories = "All";
                }
                else if (categories != "" && categories != "All" && categories != "all")
                {
                    ViewBag.SelectedCategories = Egrants.Models.Egrants.Get_CategoryName_by_id(categories);
                }

                //load data from DB
                Egrants.Models.Egrants.Search.egrants_search("", grant_id, package, 0, 0, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
                ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;
                ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
                ViewBag.ApplCount = ViewBag.appllayer.Count;
                ViewBag.doclayer = Egrants.Models.Egrants.Search.doclayerproperty;
                ViewBag.DocCount = ViewBag.doclayer.Count;

                //set appls_lis for searching by flag_type
                if (package != "" && package != "All" && package != "all")
                {
                    appls_list = EgrantsAppl.GetApplsList(grant_id, package, null);
                }

                //set appls_lis for searching by years
                if (years != "")
                {
                    if (years == "all" || years == "All")
                    {
                        appls_list = "All";
                    }
                    else
                    {
                        appls_list = EgrantsAppl.GetApplsList(grant_id, null, years);
                    }
                }

                ViewBag.SelectedAppls = appls_list;

                //reset appllayer and limit show appls if appls_list with search parameters
                if (appls_list != null && appls_list != "All" && appls_list != "all")
                {
                    List<Egrants.Models.Egrants.appllayer> appllist = new List<Egrants.Models.Egrants.appllayer>();
                    //for more than one appl
                    if (appls_list.IndexOf(',') > 1)
                    {
                        List<string> app = appls_list.Split(',').ToList();
                        //List<Egrants.Models.Egrants.appllayer> appllist = new List<Egrants.Models.Egrants.appllayer>();
                        foreach (var appl in ViewBag.appllayer)
                        {
                            if (app.Any(n => n == appl.appl_id))
                            {
                                appllist.Add(appl);
                            }
                        }
                        ViewBag.appllayer = appllist;
                    }
                    //for only one appl
                    else
                    {
                        //ViewBag.ApplID = appls_list;
                        List<string> app = appls_list.Split().ToList();
                        //List<Egrants.Models.Egrants.appllayer> appllist = new List<Egrants.Models.Egrants.appllayer>();
                        foreach (var appl in ViewBag.appllayer)
                        {
                            if (app.Any(n => n == appl.appl_id))
                            {
                                appllist.Add(appl);
                            }
                        }
                        ViewBag.appllayer = appllist;
                    }
                }

            }          
            return View("~/Egrants/Views/Index.cshtml");
        }

        public ActionResult by_appl(int appl_id=0, string mode=null, string str=null)    
        {
            ViewBag.ICList = EgrantsCommon.LoadAdminCodes();
            var isexisting = EgrantsAppl.CheckApplID(appl_id);

            if (appl_id == 0 || isexisting == 0)
            {
                ViewBag.Message = "No data found for the search";
                ViewBag.grantlayer = null;
            }
            else
            {
                //ViewBag.YearList = Egrants.Models.Egrants.P2_getYearList();
                if (str != null)
                {
                    ViewBag.Str = str;
                }

                ViewBag.Mode = mode;
                ViewBag.SearchStyle = "by_appl";
                ViewBag.ApplID = appl_id;
                ViewBag.GrantID = Egrants.Models.Egrants.GetGrantID(appl_id);
                ViewBag.SelectedCats = "All";
                ViewBag.SelectedCategories = "All";
                ViewBag.SelectedAppls = appl_id.ToString();


                //load data from DB
                Egrants.Models.Egrants.Search.egrants_search("", 0, "", appl_id, 0, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
                ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
                ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;
                ViewBag.ApplCount = ViewBag.appllayer.Count;
                ViewBag.doclayer = Egrants.Models.Egrants.Search.doclayerproperty;
                ViewBag.DocCount = ViewBag.doclayer.Count;

                //ViewBag.doclayer_All = ViewBag.doclayer;--commented by leon 4/1/2019
            }
            return View("~/Egrants/Views/Index.cshtml");
        }

        public ActionResult by_qc(string str = null)
        {
            ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

            //if (str == null || str == "")
            //{
            //    ViewBag.Message = "No data found for the search";
            //    ViewBag.grantlayer = null;
            //}
            //else
            //{
                ViewBag.str = "qc";
                ViewBag.Mode = "qc";
                //ViewBag.DocSort = "date";
                ViewBag.CurrentTab = 1;
                ViewBag.CurrentPage = 1;
                ViewBag.SearchStyle = "by_qc";

                //load data
                Egrants.Models.Egrants.Search.egrants_search("qc", 0, "", 0, 1, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
                ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
                ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;
                ViewBag.ApplCount = ViewBag.appllayer.Count;
                ViewBag.doclayer = Egrants.Models.Egrants.Search.doclayerproperty;
                ViewBag.DocCount = ViewBag.doclayer.Count;

                ViewBag.Pagination = Egrants.Models.Egrants.LoadPagination("qc", Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]), "");
                ViewBag.UnidentifiedDocs = Egrants.Models.EgrantsDoc.LoadDocsUnidentified(Convert.ToString(Session["ImageServer"]), Convert.ToString(Session["userid"]));
            //}
            return View("~/Egrants/Views/Index.cshtml");
        }

        public ActionResult by_filters(int fy = 0, string mechanism = null, string admincode = null, int serialnum = 0)
        {
            ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

            if (fy == 0 && string.IsNullOrEmpty(mechanism) && serialnum == 0) /*string.IsNullOrEmpty(admincode) &&*/
            {
                ViewBag.Message = "No data found for the search";
                ViewBag.grantlayer = null;
            }
            else
            {
                string package = "by_filters";
                ViewBag.SearchStyle = "by_filters";
                ViewBag.CurrentTab = 1;
                ViewBag.CurrentPage = 1;

                //create return value
                if (fy != 0)
                {
                    ViewBag.FilterFY = fy;
                } else ViewBag.FilterFY = "";
         
                if (serialnum != 0)
                {
                    ViewBag.FilterSerialNumber = serialnum;
                }

                ViewBag.FilterMechanism = mechanism;
                ViewBag.FilterAdminCode = admincode;               

                //create filters search sql query
                string FilterSearchQuery = Egrants.Models.Egrants.GetSearchQuery(fy, mechanism, admincode, serialnum, 1, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

                Egrants.Models.Egrants.Search.egrants_search(FilterSearchQuery, 0, package, 0, 0, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
                if (Egrants.Models.Egrants.Search.grantlayerproperty != null)
                {
                    ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
                    ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
                    ViewBag.ApplCount = ViewBag.appllayer.Count;
                    ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;

                    //show pagination
                    ViewBag.Pagination = Egrants.Models.Egrants.LoadPagination(FilterSearchQuery, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]), package);
                }
                else
                {
                    ViewBag.Message = "No data found for the search";
                    ViewBag.grantlayer = null;
                }
            }
            return View("~/Egrants/Views/Index.cshtml");
        }

        public ActionResult by_filters_page(int tab_num=0, int page_num=0, string package=null, int fy=0, string mechanism=null, string admincode =null, int serialnum=0)
        {
            ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

            /*string.IsNullOrEmpty(admincode) &&*/

            if ((fy==0 && string.IsNullOrEmpty(mechanism) && serialnum==0))
            {
                ViewBag.Message = "No data found for the search";
                ViewBag.grantlayer = null;
            }
            else if ((tab_num == 0 || page_num == 0) || ((string.IsNullOrEmpty(package) || package != "by_filters")))
            {
                ViewBag.Message = "No data found for the search";
                ViewBag.grantlayer = null;
            }
            else
            {
                ViewBag.SearchStyle = package;
                ViewBag.CurrentTab = tab_num;
                ViewBag.CurrentPage = page_num;

                //create return value
                if (fy != 0)
                {
                    ViewBag.FilterFY = fy;
                }
                else ViewBag.FilterFY = "";
                ViewBag.FilterMechanism = mechanism;
                ViewBag.FilterAdminCode = admincode;
                if (serialnum != 0)
                {
                    ViewBag.FilterSerialNumber = serialnum;
                }

                //create filters search sql query
                string FilterSearchQuery = Egrants.Models.Egrants.GetSearchQuery(fy, mechanism, admincode, serialnum, page_num, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

                //load data
                Egrants.Models.Egrants.Search.egrants_search(FilterSearchQuery, 0, package, 0, page_num, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

                ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
                ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
                ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;
                ViewBag.ApplCount = ViewBag.appllayer.Count;

                //show Pagination 
                ViewBag.Pagination = Egrants.Models.Egrants.LoadPagination(FilterSearchQuery, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]), package);
            }
           

            return View("~/Egrants/Views/Index.cshtml");
        }

        public ActionResult by_page(string str=null, int tab_num=0, int page_num=0, string package=null, string mode=null)
        {
            ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

            if (string.IsNullOrEmpty(str))
            {
                ViewBag.Message = "No data found for the search";
                ViewBag.grantlayer = null;
            }
            else if (page_num == 0 || tab_num == 0 )
            {
                ViewBag.Message = "No data found for the search";
                ViewBag.grantlayer = null;
            }
            else
            {
                ViewBag.SearchStyle = "by_page";
                ViewBag.CurrentTab = tab_num;
                ViewBag.CurrentPage = page_num;
                ViewBag.Str = str;
                ViewBag.Mode = mode;

                Egrants.Models.Egrants.Search.egrants_search(str, 0, "", 0, page_num, Convert.ToString(Session["browser"]), Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));

                ViewBag.grantlayer = Egrants.Models.Egrants.Search.grantlayerproperty;
                ViewBag.appllayer = Egrants.Models.Egrants.Search.appllayerproperty;
                ViewBag.appllayer_All = Egrants.Models.Egrants.Search.appllayerproperty;
                ViewBag.ApplCount = ViewBag.appllayer.Count;
                ViewBag.doclayer = Egrants.Models.Egrants.Search.doclayerproperty;
                ViewBag.DocCount = ViewBag.doclayer.Count;

                if (str == "qc")
                {
                    ViewBag.Mode = "qc";
                }

                //show Pagination 
                ViewBag.Pagination = Egrants.Models.Egrants.LoadPagination(str, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]), package);

                if (str == "qc")
                {
                    ViewBag.UnidentifiedDocs = Egrants.Models.EgrantsDoc.LoadDocsUnidentified(Convert.ToString(Session["ImageServer"]), Convert.ToString(Session["userid"]));
                }
            }
            return View("~/Egrants/Views/Index.cshtml");
        }

        //Autocomplete for fy, activity_code and serial_number
        public JsonResult load_data_autocomplete(string type, string term, string mechanism = null, string fy = null, string admincode = null, string serialnum = null)
        {
            string sql_query = "";
            //List<string> data_list = new List<string>();

            if (admincode != null && admincode != "")
            {
                ViewBag.admincode = admincode;
            }
            if (admincode == "undefined")
            {
                ViewBag.admincode = "";
            }

            else ViewBag.admincode = "";

            ViewBag.FilterFY = fy;

            ViewBag.FilterSerialNumber = serialnum;

            ViewBag.FilterMechanism = mechanism;

            ViewBag.FilterAdminCode = admincode;
            ViewBag.ICList = EgrantsCommon.LoadAdminCodes();
            List<string> data_list = new List<string>();
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                //if (type == "fy")
                //{
                //    sql_query = "sp_web_egrants_load_data_autocomplete";
                //}

                if (type == "mechanism")
                {
                    sql_query = "sp_web_egrants_load_data_autocomplete_mechanism";
                }
                if (type == "serialnum")
                {
                    sql_query = "sp_web_egrants_load_data_autocomplete_serialnum";
                }
                if (type == "fy")
                {
                    sql_query = "sp_web_egrants_load_data_autocomplete_fy";
                }

                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(sql_query, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@term", term);
                cmd.Parameters.AddWithValue("@fy", fy);
                cmd.Parameters.AddWithValue("@mechanism", mechanism);
                cmd.Parameters.AddWithValue("@admincode", admincode);
                cmd.Parameters.AddWithValue("@serialnum", serialnum);
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    data_list.Add(rdr[0].ToString());
                    //sql_query = rdr[0].ToString();
                }
            }
            return Json(data_list, JsonRequestBehavior.AllowGet);
        }

        //load documents by appl_id
        public JsonResult LoadDocsGrid(int appl_id, string search_type=null, string category_list=null, string mode = null)
        {
            Egrants.Models.Egrants.Search_by_appl_id.LoadDocs(appl_id, search_type, category_list, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
            ViewBag.doclayer = Egrants.Models.Egrants.Search_by_appl_id.doclayerproperty;
 
            //ViewBag.doclayer = Egrants.Models.Egrants.Search_by_appl_id.doclayerproperty.ToList();

            dynamic res = new { data = ViewBag.doclayer };
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult stop_notice(int grant_id)
        {
            ViewBag.StopNotice = Egrants.Models.Egrants.LoadStopNotice(grant_id, Convert.ToString(Session["ic"]));
            return View("~/Egrants/Views/_Modal_Stop_Notice.cshtml");
        }

        public ActionResult supplement(int grant_id)
        {
            string act = "to_view";
            ViewBag.StopNotice = Egrants.Models.Egrants.LoadSupplement(act, grant_id, 0, "", "", 0, Convert.ToString(Session["ic"]), Convert.ToString(Session["userid"]));
            return View("~/Egrants/Views/_Modal_Supplement.cshtml");
        }
    }
}