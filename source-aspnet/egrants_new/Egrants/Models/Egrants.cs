using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Net;

namespace egrants_new.Egrants.Models
{
    public class Egrants
    {
        public class grantlayer
        {
            public string grant_id { get; set; }
            public string org_name { get; set; }
            public string admin_phs_org_code { get; set; }
            public string serial_num { get; set; }
            public string grant_num { get; set; }
            public string former_grant_num { get; set; }
            public string latest_full_grant_num { get; set; }
            public string all_activity_code { get; set; }
            public string project_title { get; set; }
            public string pi_name { get; set; }       
            public string current_pi_name { get; set; }
            public string current_pi_email_address { get; set; }
            public string current_pd_name { get; set; }
            public string current_pd_email_address { get; set; }
            public string current_spec_name { get; set; }
            public string current_spec_email_address { get; set; }
            public string current_bo_email_address { get; set; }
            public string prog_class_code { get; set; }
            public string sv_url { get; set; }
            public string arra_flag { get; set; }
            public string fda_flag { get; set; }
            public string stop_flag { get; set; }
            public string ms_flag { get; set; }
            public string od_flag { get; set; }
            public string ds_flag { get; set; }
            public string adm_supp { get; set; }
            public bool institutional_flag1 { get; set; }
            public bool institutional_flag2 { get; set; }
            public string inst_flag1_url { get; set; }
            public bool AnyOrgDoc { get; set; }

        }

        public class appllayer
        {
            public string grant_id { get; set; }
            public string appl_id { get; set; }
            public string full_grant_num { get; set; }
            public string support_year { get; set; }
            public string appl_type_code { get; set; }
            public string deleted_by_impac { get; set; }
            public string doc_count { get; set; }
            public string closeout_notcount { get; set; }
            public string can_add_doc { get; set; }
            public string competing { get; set; }
            public string fsr_count { get; set; }
            public string frc_destroyed { get; set; }        
            public string appl_fda_flag { get; set; }
            public string appl_ms_flag { get; set; }
            public string appl_od_flag { get; set; }
            public string appl_ds_flag { get; set; }
            public string closeout_flag { get; set; }
            public string irppr_id { get; set; }
            public string can_add_funding { get; set; }
        }

        public class doclayer
        {       
            public string appl_id { get; set; }
            public string docs_count { get; set; }
            public string grant_id { get; set; }
            public string full_grant_num { get; set; }
            public string document_id { get; set; }
            public string document_date { get; set; }
            public string document_name { get; set; }
            public DateTime doc_date { get; set; }
            public string category_id { get; set; }
            public string category_name { get; set; }
            public string sub_category_name { get; set; }
            public string created_by { get; set; }
            public string created_date { get; set; }
            public string modified_by { get; set; }
            public string modified_date { get; set; }
            public string file_modified_by { get; set; }
            public string file_modified_date { get; set; }
            public string problem_msg { get; set; }
            public string problem_reported_by { get; set; }
            public string page_count { get; set; }
            public string fsr_count { get; set; }
            public string attachment_count { get; set; }
            public string closeout_notcount { get; set; }
            public string frc_destroyed { get; set; }
            public string url { get; set; }
            public string qc_date { get; set; }
            public string can_qc { get; set; }
            public string can_upload { get; set; }
            public string can_modify_index { get; set; }
            public string can_delete { get; set; }
            public string can_restore { get; set; }
            public string can_store { get; set; }
            public string ic { get; set; }
        }

        public class Search
        {
            public static List<grantlayer> grantlayerproperty { get; set; }
            public static List<appllayer> appllayerproperty { get; set; }
            public static List<doclayer> doclayerproperty { get; set; }
            public static List<doclayer> doclayerproperty_era { get; set; }

            public static void egrants_search(string str, int grant_id, string package, int appl_id, int current_page, string browser, string ic, string userid)
            {
                System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("dbo.sp_web_egrants", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@str", System.Data.SqlDbType.NVarChar).Value = str;
                cmd.Parameters.Add("@grant_id", System.Data.SqlDbType.Int).Value = grant_id;
                cmd.Parameters.Add("@package", System.Data.SqlDbType.VarChar).Value = package;
                cmd.Parameters.Add("@appl_id", System.Data.SqlDbType.Int).Value = appl_id;
                cmd.Parameters.Add("@current_page", System.Data.SqlDbType.Int).Value = current_page;
                cmd.Parameters.Add("@browser", System.Data.SqlDbType.VarChar).Value = browser;
                cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

                //cmd.CommandTimeout = 120;
                conn.Open();

                grantlayerproperty = null;
                appllayerproperty = null;
                doclayerproperty = null;

                var grantList = new List<grantlayer>();
                var applList = new List<appllayer>();
                var docList = new List<doclayer>();

                SqlDataReader rdr = cmd.ExecuteReader();

                int tag = 0;

                while (rdr.Read())
                {
                    tag = Convert.ToInt32(rdr["tag"]);
                    if (tag == 1)
                    {
                        grantlayer grant = new grantlayer();
                        grant.grant_id = rdr["grant_id"]?.ToString();
                        grant.org_name = rdr["org_name"]?.ToString();
                        grant.serial_num = rdr["serial_num"]?.ToString();
                        grant.grant_num = String.Concat(rdr["admin_phs_org_code"].ToString() + Convert.ToInt32(rdr["serial_num"]).ToString("000000"));
                        grant.former_grant_num = rdr["former_grant_num"]?.ToString();
                        grant.latest_full_grant_num = rdr["latest_full_grant_num"]?.ToString();
                        grant.admin_phs_org_code = rdr["admin_phs_org_code"]?.ToString();
                        grant.project_title = rdr["project_title"]?.ToString();
                        grant.pi_name = rdr["pi_name"]?.ToString();
                        grant.prog_class_code = rdr["prog_class_code"]?.ToString();
                        grant.all_activity_code = rdr["all_activity_code"]?.ToString();  
                        grant.current_pi_name = rdr["current_pi_name"]?.ToString();
                        grant.current_pi_email_address = rdr["current_pi_email_address"]?.ToString();
                        grant.current_pd_name = rdr["current_pd_name"]?.ToString();
                        grant.current_pd_email_address = rdr["current_pd_email_address"]?.ToString();
                        grant.current_spec_name = rdr["current_spec_name"]?.ToString();
                        grant.current_spec_email_address = rdr["current_spec_email_address"]?.ToString();
                        grant.current_bo_email_address = rdr["current_bo_email_address"]?.ToString();
                        grant.sv_url = rdr["sv_url"]?.ToString();
                        grant.arra_flag = rdr["arra_flag"]?.ToString();
                        grant.fda_flag = rdr["fda_flag"]?.ToString();                    
                        grant.stop_flag = rdr["stop_flag"]?.ToString();
                        grant.ms_flag = rdr["ms_flag"]?.ToString();
                        grant.od_flag = rdr["od_flag"]?.ToString();
                        grant.ds_flag = rdr["ds_flag"]?.ToString();
                        grant.adm_supp = rdr["adm_supp"]?.ToString();
                        grant.institutional_flag1 = rdr["institutional_flag1"].ToString() == "1" ? true:false;
                        grant.AnyOrgDoc = (bool) rdr["institutional_flag2"];
                        grant.inst_flag1_url = rdr["inst_flag1_url"].ToString();
//                        grant.inst_flag2_url = rdr["inst_flag2_url"].ToString();

                        grantList.Add(grant);
                    }

                    else if (tag == 2)
                    {
                        appllayer appl = new appllayer();
                        appl.grant_id = rdr["grant_id"]?.ToString();
                        appl.appl_id = rdr["appl_id"]?.ToString();
                        appl.appl_type_code = rdr["appl_type_code"]?.ToString();
                        appl.full_grant_num = rdr["full_grant_num"]?.ToString();
                        appl.support_year = rdr["support_year"]?.ToString();
                        appl.deleted_by_impac = rdr["deleted_by_impac"]?.ToString();
                        appl.doc_count = rdr["doc_count"]?.ToString();
                        appl.closeout_notcount = rdr["closeout_notcount"]?.ToString();
                        appl.can_add_doc = rdr["can_add_doc"]?.ToString();
                        appl.competing = rdr["competing"]?.ToString();
                        appl.fsr_count = rdr["fsr_count"]?.ToString();
                        appl.frc_destroyed = rdr["frc_destroyed"]?.ToString();
                        appl.appl_fda_flag = rdr["appl_fda_flag"]?.ToString();
                        appl.appl_ms_flag = rdr["appl_ms_flag"]?.ToString();
                        appl.appl_od_flag = rdr["appl_od_flag"]?.ToString();
                        appl.appl_ds_flag = rdr["appl_ds_flag"]?.ToString();
                        appl.closeout_flag = rdr["closeout_flag"]?.ToString();
                        appl.irppr_id = rdr["irppr_id"]?.ToString();
                        appl.can_add_funding = rdr["can_add_funding"]?.ToString();

                        applList.Add(appl);
                    }

                    else if (tag == 3)
                    {
                        doclayer doc = new doclayer();
                        doc.appl_id = rdr["appl_id"]?.ToString();
                        doc.docs_count = rdr["docs_count"]?.ToString();
                       
                        docList.Add(doc);
                    }
                }
                //added by Leon 5/11/2019
                conn.Close();

                grantlayerproperty = grantList;
                appllayerproperty = applList;
                doclayerproperty = docList;
            }
        }
     
        public class Pagination
        {
            public string tag { get; set; }
            public string parent { get; set; }
            public string total_grants { get; set; }
            public string total_tabs { get; set; }
            public string total_pages { get; set; }
            public string tab_number { get; set; }
            public string page_number { get; set; }
        }

        public static List<Pagination> LoadPagination(string str, string ic, string userid, string package=null)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("dbo.sp_web_egrants_pagination", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@str", System.Data.SqlDbType.NVarChar).Value = str;
            cmd.Parameters.Add("@package", System.Data.SqlDbType.VarChar).Value = package;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var EgrantsPagination = new List<Pagination>();
            SqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                EgrantsPagination.Add(new Pagination
                {
                    tag = rdr["tag"]?.ToString(),
                    parent = rdr["parent"]?.ToString(),
                    total_grants = rdr["total_grants"]?.ToString(),
                    total_tabs = rdr["total_tabs"]?.ToString(),
                    total_pages = rdr["total_pages"]?.ToString(),
                    tab_number = rdr["tab_number"]?.ToString(),
                    page_number = rdr["page_number"]?.ToString()
                });
            }
            conn.Close();
            return EgrantsPagination;
        }

        public class StopNotice
        {
            public string appl_id { get; set; }
            public string full_grant_num { get; set; }
            public string closeout_fsr_code { get; set; }
            public string final_invention_stmnt_code { get; set; }
            public string final_report_date { get; set; }
        }

        public static List<StopNotice> LoadStopNotice(int grant_id, string ic)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            SqlCommand cmd = new SqlCommand("sp_web_egrants_stop_notice ", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@GrantID", System.Data.SqlDbType.Int).Value = grant_id;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;

            conn.Open();

            var StopNotice = new List<StopNotice>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                StopNotice.Add(new StopNotice
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString(),
                    closeout_fsr_code = rdr["closeout_fsr_code"]?.ToString(),
                    final_invention_stmnt_code = rdr["final_invention_stmnt_code"]?.ToString(),
                    final_report_date = rdr["final_report_date"]?.ToString(),
                });
            }
            conn.Close();
            return StopNotice;
        }

        public class supplement
        {
            public string tag { get; set; }
            public string grant_id { get; set; }
            public string admin_phs_org_code { get; set; }
            public string serial_num { get; set; }
            public string id { get; set; }
            public string full_grant_num { get; set; }
            public string supp_appl_id { get; set; }
            public string support_year { get; set; }
            public string suffix_code { get; set; }
            public string former_num { get; set; }
            public string submitted_date { get; set; }
            public string category_name { get; set; }
            public string status { get; set; }
            public string url { get; set; }
            public string moved_date { get; set; }
            public string moved_by { get; set; }
        }

        public static List<supplement> LoadSupplement(string act, int grant_id, int support_year, string suffix_code, string docid_str, int former_applid, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_supplement", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@grant_id", System.Data.SqlDbType.Int).Value = grant_id;
            cmd.Parameters.Add("@support_year", System.Data.SqlDbType.Int).Value = support_year;
            cmd.Parameters.Add("@suffix_code", System.Data.SqlDbType.VarChar).Value = suffix_code;
            cmd.Parameters.Add("@docid_str", System.Data.SqlDbType.VarChar).Value = docid_str;
            cmd.Parameters.Add("@former_applid", System.Data.SqlDbType.VarChar).Value = former_applid;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@Operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var Supplement = new List<supplement>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Supplement.Add(new supplement
                {
                    tag = rdr["tag"]?.ToString(),
                    grant_id = rdr["grant_id "]?.ToString(),
                    admin_phs_org_code = rdr["admin_phs_org_code "]?.ToString(),
                    serial_num = rdr["serial_num"]?.ToString(),
                    id = rdr["id"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString(),
                    supp_appl_id = rdr["supp_appl_id"]?.ToString(),
                    support_year = rdr["support_year"]?.ToString(),
                    suffix_code = rdr["suffix_code"]?.ToString(),
                    former_num = rdr["former_num"]?.ToString(),
                    submitted_date = rdr["submitted_date"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    url = rdr["url"]?.ToString(),
                    moved_date = rdr["moved_date"]?.ToString(),
                    moved_by = rdr["moved_by"]?.ToString(),
                });
            }
            conn.Close();
            return Supplement;
        }

        //Phase2
        public static List<string> GetICList()
        {
            List<string> IC_list = new List<string>();
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                //System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select admin_phs_org_code as IC from profiles", conn);
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select distinct admin_phs_org_code as IC from grants", conn);
                cmd.CommandType = CommandType.Text;
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    IC_list.Add(rdr[0].ToString());
                }

                //added by Leon 5/11/2019
                conn.Close();
            }
         
            return IC_list;
        }

        //search by filters
        public static string GetSearchQuery(int fy, string mechanism, string admincode, int serialnum, int page_num, string browser, string ic, string userid)
        {
            string FilterSearchQuery = "";
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {              
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_search_by_filters", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fy", fy);
                cmd.Parameters.AddWithValue("@mechanism", mechanism);
                cmd.Parameters.AddWithValue("@admincode", admincode);
                cmd.Parameters.AddWithValue("@serialnum", serialnum);
                cmd.Parameters.AddWithValue("@page_num", page_num);
                cmd.Parameters.AddWithValue("@browser", browser);
                cmd.Parameters.AddWithValue("@ic", ic);
                cmd.Parameters.AddWithValue("@operator", userid);

                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    FilterSearchQuery = rdr[0].ToString();
                }

                //added by Leon 5/11/2019
                conn.Close();
            }
            return FilterSearchQuery;
        }
         
        //load full grant number list by filters data---added by Ayu 3/15
        public static List<string> GetYearList(string fy = null, string mechanism = null, string admin_code = null, string serial_num = null)
        {
            List<string> yearList = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_web_egrants_load_data_years", conn);            
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fy", fy);
                cmd.Parameters.AddWithValue("@mechanism", mechanism);
                cmd.Parameters.AddWithValue("@admincode", admin_code);
                cmd.Parameters.AddWithValue("@serialnum", serial_num);

                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    string applData = rdr[0].ToString() + ":" + rdr[1].ToString();
                    yearList.Add(applData);
                }

                //added by Leon 5/11/2019
                conn.Close();
            }
            return yearList;
        }

        //load category list string 
        public static List<string> GetCategoryList(int grant_id, string years)
        {
            List<string> CategoryList = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_web_egrants_load_category_list", conn);           
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@grant_id", grant_id);
                cmd.Parameters.AddWithValue("@years", years);

                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    string category = rdr[0].ToString() + ":" + rdr[1].ToString();
                    CategoryList.Add(category);
                }

                //added by Leon 5/11/2019
                conn.Close();
            }
            return CategoryList;
        }

        //load category name string by year
        //public static string Get_CategoryName_by_year(int grant_id, string years)
        //{
        //    string CategoryNameList = "";
        //    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
        //    {
        //        SqlCommand cmd = new SqlCommand("sp_web_egrants_load_category_list", conn);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@grant_id", grant_id);
        //        cmd.Parameters.AddWithValue("@years", years);

        //        conn.Open();
        //        SqlDataReader rdr = cmd.ExecuteReader();

        //        while (rdr.Read())
        //        {
        //            string category = rdr[1].ToString() + ", ";
        //            CategoryNameList = CategoryNameList + category;
        //        }
        //    }

        //    if (CategoryNameList !="" && CategoryNameList.IndexOf(",") > 0)
        //    {
        //        CategoryNameList = CategoryNameList.Substring(0, (CategoryNameList.Length - 2));
        //    }

        //    return CategoryNameList;
        //}

        //load category name string by year
        public static string Get_CategoryName_by_id(string categories)
        {
            string CategoryNameList = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("select category_name from categories where category_id in ("+ @categories+ ") order by category_name", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@categories", categories);
                //cmd.Parameters.AddWithValue("@years", years);

                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    string category = rdr[0].ToString() + ", ";
                    CategoryNameList = CategoryNameList + category;
                }

                //added by Leon 5/11/2019
                conn.Close();
            }

            if (CategoryNameList != "" && CategoryNameList.IndexOf(",") > 0)
            {
                CategoryNameList = CategoryNameList.Substring(0, (CategoryNameList.Length - 2));
            }

            return CategoryNameList;
        }

        public static int GetGrantID(int appl_id)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("select grant_id from appls where appl_id = @appl_id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@appl_id", System.Data.SqlDbType.VarChar).Value = appl_id.ToString();

                conn.Open();
                int grant_id = 0;
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    grant_id = Convert.ToInt32(rdr["grant_id"]);
                }
                conn.Close();
                return grant_id;

                //conn.Open();
                //int grant_id = (int)cmd.ExecuteScalar();
                //conn.Close();
                //return grant_id;
            }
        }

        //load documents by appl_id
        public class Search_by_appl_id
        {          
            public static List<doclayer> doclayerproperty { get; set; }
            public static List<doclayer> doclayerproperty_era { get; set; }
       
            public static void LoadDocs(int appl_id, string search_type, string category_list, string ic, string userid)
            {
                System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_search_by_appl_id", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@appl_id", System.Data.SqlDbType.Int).Value = appl_id;
                cmd.Parameters.Add("@search_type", System.Data.SqlDbType.VarChar).Value = search_type;
                cmd.Parameters.Add("@category_list", System.Data.SqlDbType.VarChar).Value = category_list;
                cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

                //cmd.CommandTimeout = 120;
                conn.Open();

                doclayerproperty = null;
                var docList = new List<doclayer>();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    doclayer doc = new doclayer();
                    doc.grant_id = rdr["grant_id"]?.ToString();
                    doc.appl_id = rdr["appl_id"]?.ToString();
                    //doc.full_grant_num = rdr["full_grant_num"]?.ToString();
                    doc.document_id = rdr["document_id"]?.ToString();
                    doc.document_date = rdr["document_date"]?.ToString();
                    doc.document_name = rdr["document_name"]?.ToString();
                    if (rdr["doc_date"] != DBNull.Value)
                    {
                        doc.doc_date = Convert.ToDateTime(rdr["doc_date"]);
                    }
                    doc.category_id = rdr["category_id"]?.ToString();
                    doc.category_name = rdr["category_name"]?.ToString();
                    doc.sub_category_name = rdr["sub_category_name"]?.ToString();
                    doc.created_by = rdr["created_by"]?.ToString();
                    doc.created_date = rdr["created_date"]?.ToString();
                    doc.modified_by = rdr["modified_by"]?.ToString();
                    doc.modified_date = rdr["modified_date"]?.ToString();
                    doc.file_modified_by = rdr["file_modified_by"]?.ToString();
                    doc.file_modified_date = rdr["file_modified_date"]?.ToString();
                    doc.problem_msg = rdr["problem_msg"]?.ToString();
                    doc.problem_reported_by = rdr["problem_reported_by"]?.ToString();
                    doc.page_count = rdr["page_count"]?.ToString();
                    doc.fsr_count = rdr["fsr_count"]?.ToString();
                    doc.attachment_count = rdr["attachment_count"]?.ToString();
                    doc.url = rdr["url"]?.ToString();
                    doc.frc_destroyed = rdr["frc_destroyed"]?.ToString();
                    doc.qc_date = rdr["qc_date"]?.ToString();
                    doc.can_qc = rdr["can_qc"]?.ToString();
                    doc.can_upload = rdr["can_upload"]?.ToString();
                    doc.can_modify_index = rdr["can_modify_index"]?.ToString();
                    doc.can_delete = rdr["can_delete"]?.ToString();
                    doc.can_restore = rdr["can_restore"]?.ToString();
                    doc.can_store = rdr["can_store"]?.ToString();
                    //doc.ic = rdr["ic"]?.ToString();

                    docList.Add(doc);
                }

                //added by Leon 5/11/2019
                conn.Close();
                doclayerproperty = docList;
            }
        }

        //check if this grant_id is existing in grants table, added by Leon 7/10/2019
        public static int CheckGrantID(int grant_id)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("select count(*) as count_id from grants where grant_id = @grant_id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@grant_id", System.Data.SqlDbType.Int).Value = grant_id;

                conn.Open();
                int isexisting = 0;
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    isexisting = Convert.ToInt32(rdr["count_id"]);
                }
                conn.Close();
                return isexisting;
            }
        }
    }
}