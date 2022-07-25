#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  Egrants.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-03-31
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

#endregion

namespace egrants_new.Egrants.Models
{
    /// <summary>
    /// The egrants.
    /// </summary>
    public class Egrants
    {

        /// <summary>
        /// The load pagination.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Pagination> LoadPagination(string str, string ic, string userid, string package = null)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("dbo.sp_web_egrants_pagination", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@str", SqlDbType.NVarChar).Value = str;
            cmd.Parameters.Add("@package", SqlDbType.VarChar).Value = package;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var EgrantsPagination = new List<Pagination>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                EgrantsPagination.Add(
                    new Pagination
                        {
                            tag = rdr["tag"]?.ToString(),
                            parent = rdr["parent"]?.ToString(),
                            total_grants = rdr["total_grants"]?.ToString(),
                            total_tabs = rdr["total_tabs"]?.ToString(),
                            total_pages = rdr["total_pages"]?.ToString(),
                            tab_number = rdr["tab_number"]?.ToString(),
                            page_number = rdr["page_number"]?.ToString()
                        });

            conn.Close();

            return EgrantsPagination;
        }

        /// <summary>
        /// The load stop notice.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<StopNotice> LoadStopNotice(int grant_id, string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_stop_notice ", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@GrantID", SqlDbType.Int).Value = grant_id;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;

            conn.Open();

            var list = new List<StopNotice>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(
                    new StopNotice
                        {
                            appl_id = rdr["appl_id"]?.ToString(),
                            full_grant_num = rdr["full_grant_num"]?.ToString(),
                            closeout_fsr_code = rdr["closeout_fsr_code"]?.ToString(),
                            final_invention_stmnt_code = rdr["final_invention_stmnt_code"]?.ToString(),
                            final_report_date = rdr["final_report_date"]?.ToString()
                        });
            }

            conn.Close();

            return list;
        }

        /// <summary>
        /// The load supplement.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <param name="support_year">
        /// The support_year.
        /// </param>
        /// <param name="suffix_code">
        /// The suffix_code.
        /// </param>
        /// <param name="docid_str">
        /// The docid_str.
        /// </param>
        /// <param name="former_applid">
        /// The former_applid.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<supplement> LoadSupplement(
            string act,
            int grant_id,
            int support_year,
            string suffix_code,
            string docid_str,
            int former_applid,
            string ic,
            string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_supplement", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@grant_id", SqlDbType.Int).Value = grant_id;
            cmd.Parameters.Add("@support_year", SqlDbType.Int).Value = support_year;
            cmd.Parameters.Add("@suffix_code", SqlDbType.VarChar).Value = suffix_code;
            cmd.Parameters.Add("@docid_str", SqlDbType.VarChar).Value = docid_str;
            cmd.Parameters.Add("@former_applid", SqlDbType.VarChar).Value = former_applid;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@Operator", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var list = new List<supplement>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(
                    new supplement
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
                            moved_by = rdr["moved_by"]?.ToString()
                        });
            }

            conn.Close();

            return list;
        }

        // Phase2
        /// <summary>
        /// The get ic list.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<string> GetICList()
        {
            var list = new List<string>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                // System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select admin_phs_org_code as IC from profiles", conn);
                var cmd = new SqlCommand("select distinct admin_phs_org_code as IC from grants", conn);
                cmd.CommandType = CommandType.Text;
                conn.Open();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    list.Add(rdr[0].ToString());

                // added by Leon 5/11/2019
                conn.Close();
            }

            return list;
        }

        // search by filters
        /// <summary>
        /// The get search query.
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
        /// <param name="page_num">
        /// The page_num.
        /// </param>
        /// <param name="browser">
        /// The browser.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetSearchQuery(
            int fy,
            string mechanism,
            string admincode,
            int serialnum,
            int page_num,
            string browser,
            string ic,
            string userid)
        {
            var FilterSearchQuery = string.Empty;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("sp_web_egrants_search_by_filters", conn);
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
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    FilterSearchQuery = rdr[0].ToString();

                // added by Leon 5/11/2019
                conn.Close();
            }

            return FilterSearchQuery;
        }

        // load full grant number list by filters data---added by Ayu 3/15
        /// <summary>
        /// The get year list.
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
        /// The <see cref="List"/>.
        /// </returns>
        public static List<string> GetYearList(string fy = null, string mechanism = null, string admin_code = null, string serial_num = null)
        {
            var yearList = new List<string>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("sp_web_egrants_load_data_years", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fy", fy);
                cmd.Parameters.AddWithValue("@mechanism", mechanism);
                cmd.Parameters.AddWithValue("@admincode", admin_code);
                cmd.Parameters.AddWithValue("@serialnum", serial_num);

                conn.Open();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var applData = rdr[0] + ":" + rdr[1];
                    yearList.Add(applData);
                }

                // added by Leon 5/11/2019
                conn.Close();
            }

            return yearList;
        }

        // load category list string 
        /// <summary>
        /// The get category list.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <param name="years">
        /// The years.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<string> GetCategoryList(int grant_id, string years)
        {
            var CategoryList = new List<string>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("sp_web_egrants_load_category_list", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@grant_id", grant_id);
                cmd.Parameters.AddWithValue("@years", years);

                conn.Open();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var category = rdr[0] + ":" + rdr[1];
                    CategoryList.Add(category);
                }

                // added by Leon 5/11/2019
                conn.Close();
            }

            return CategoryList;
        }

        // load category name string by year
        // public static string Get_CategoryName_by_year(int grant_id, string years)
        // {
        // string CategoryNameList = "";
        // using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
        // {
        // SqlCommand cmd = new SqlCommand("sp_web_egrants_load_category_list", conn);
        // cmd.CommandType = CommandType.StoredProcedure;
        // cmd.Parameters.AddWithValue("@grant_id", grant_id);
        // cmd.Parameters.AddWithValue("@years", years);

        // conn.Open();
        // SqlDataReader rdr = cmd.ExecuteReader();

        // while (rdr.Read())
        // {
        // string category = rdr[1].ToString() + ", ";
        // CategoryNameList = CategoryNameList + category;
        // }
        // }

        // if (CategoryNameList !="" && CategoryNameList.IndexOf(",") > 0)
        // {
        // CategoryNameList = CategoryNameList.Substring(0, (CategoryNameList.Length - 2));
        // }

        // return CategoryNameList;
        // }

        // load category name string by year
        /// <summary>
        /// The get_ category name_by_id.
        /// </summary>
        /// <param name="categories">
        /// The categories.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string Get_CategoryName_by_id(string categories)
        {
            var CategoryNameList = string.Empty;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand(
                    "select category_name from categories where category_id in (" + categories + ") order by category_name",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@categories", categories);

                // cmd.Parameters.AddWithValue("@years", years);
                conn.Open();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var category = rdr[0] + ", ";
                    CategoryNameList = CategoryNameList + category;
                }

                // added by Leon 5/11/2019
                conn.Close();
            }

            if (CategoryNameList != string.Empty && CategoryNameList.IndexOf(",") > 0)
                CategoryNameList = CategoryNameList.Substring(0, CategoryNameList.Length - 2);

            return CategoryNameList;
        }

        /// <summary>
        /// The get grant id.
        /// </summary>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int GetGrantID(int appl_id)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("select grant_id from appls where appl_id = @appl_id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@appl_id", SqlDbType.VarChar).Value = appl_id.ToString();

                conn.Open();
                var grant_id = 0;
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    grant_id = Convert.ToInt32(rdr["grant_id"]);

                conn.Close();

                return grant_id;

                // conn.Open();
                // int grant_id = (int)cmd.ExecuteScalar();
                // conn.Close();
                // return grant_id;
            }
        }

        /// <summary>
        /// Check if this grant_id is existing in grants table, added by Leon 7/10/2019
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int CheckGrantID(int grant_id)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("select count(*) as count_id from grants where grant_id = @grant_id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@grant_id", SqlDbType.Int).Value = grant_id;

                conn.Open();
                var exists = 0;
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    exists = Convert.ToInt32(rdr["count_id"]);
                }

                conn.Close();

                return exists;
            }
        }

        /// <summary>
        /// The grantlayer.
        /// </summary>
        public class grantlayer
        {
            /// <summary>
            /// Gets or sets the grant_id.
            /// </summary>
            public string grant_id { get; set; }

            /// <summary>
            /// Gets or sets the org_name.
            /// </summary>
            public string org_name { get; set; }

            /// <summary>
            /// Gets or sets the admin_phs_org_code.
            /// </summary>
            public string admin_phs_org_code { get; set; }

            /// <summary>
            /// Gets or sets the serial_num.
            /// </summary>
            public string serial_num { get; set; }

            /// <summary>
            /// Gets or sets the grant_num.
            /// </summary>
            public string grant_num { get; set; }

            /// <summary>
            /// Gets or sets the former_grant_num.
            /// </summary>
            public string former_grant_num { get; set; }

            /// <summary>
            /// Gets or sets the latest_full_grant_num.
            /// </summary>
            public string latest_full_grant_num { get; set; }

            /// <summary>
            /// Gets or sets the all_activity_code.
            /// </summary>
            public string all_activity_code { get; set; }

            /// <summary>
            /// Gets or sets the project_title.
            /// </summary>
            public string project_title { get; set; }

            /// <summary>
            /// Gets or sets the pi_name.
            /// </summary>
            public string pi_name { get; set; }

            /// <summary>
            /// Gets or sets the current_pi_name.
            /// </summary>
            public string current_pi_name { get; set; }

            /// <summary>
            /// Gets or sets the current_pi_email_address.
            /// </summary>
            public string current_pi_email_address { get; set; }

            /// <summary>
            /// Gets or sets the current_pd_name.
            /// </summary>
            public string current_pd_name { get; set; }

            /// <summary>
            /// Gets or sets the current_pd_email_address.
            /// </summary>
            public string current_pd_email_address { get; set; }

            /// <summary>
            /// Gets or sets the current_spec_name.
            /// </summary>
            public string current_spec_name { get; set; }

            /// <summary>
            /// Gets or sets the current_spec_email_address.
            /// </summary>
            public string current_spec_email_address { get; set; }

            /// <summary>
            /// Gets or sets the current_bo_email_address.
            /// </summary>
            public string current_bo_email_address { get; set; }

            /// <summary>
            /// Gets or sets the prog_class_code.
            /// </summary>
            public string prog_class_code { get; set; }

            /// <summary>
            /// Gets or sets the sv_url.
            /// </summary>
            public string sv_url { get; set; }

            /// <summary>
            /// Gets or sets the arra_flag.
            /// </summary>
            public string arra_flag { get; set; }

            /// <summary>
            /// Gets or sets the fda_flag.
            /// </summary>
            public string fda_flag { get; set; }

            /// <summary>
            /// Gets or sets the stop_flag.
            /// </summary>
            public string stop_flag { get; set; }

            /// <summary>
            /// Gets or sets the ms_flag.
            /// </summary>
            public string ms_flag { get; set; }

            /// <summary>
            /// Gets or sets the od_flag.
            /// </summary>
            public string od_flag { get; set; }

            /// <summary>
            /// Gets or sets the ds_flag.
            /// </summary>
            public string ds_flag { get; set; }

            /// <summary>
            /// Gets or sets the adm_supp.
            /// </summary>
            public string adm_supp { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether institutional_flag 1.
            /// </summary>
            public bool institutional_flag1 { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether institutional_flag 2.
            /// </summary>
            public bool institutional_flag2 { get; set; }

            /// <summary>
            /// Gets or sets the inst_flag 1_url.
            /// </summary>
            public string inst_flag1_url { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether any org doc.
            /// </summary>
            public bool AnyOrgDoc { get; set; }
        }

        /// <summary>
        /// The appllayer.
        /// </summary>
        public class appllayer
        {
            /// <summary>
            /// Gets or sets the grant_id.
            /// </summary>
            public string grant_id { get; set; }

            /// <summary>
            /// Gets or sets the appl_id.
            /// </summary>
            public string appl_id { get; set; }

            /// <summary>
            /// Gets or sets the full_grant_num.
            /// </summary>
            public string full_grant_num { get; set; }

            /// <summary>
            /// Gets or sets the support_year.
            /// </summary>
            public string support_year { get; set; }

            /// <summary>
            /// Gets or sets the appl_type_code.
            /// </summary>
            public string appl_type_code { get; set; }

            /// <summary>
            /// Gets or sets the deleted_by_impac.
            /// </summary>
            public string deleted_by_impac { get; set; }

            /// <summary>
            /// Gets or sets the doc_count.
            /// </summary>
            public string doc_count { get; set; }

            /// <summary>
            /// Gets or sets the closeout_notcount.
            /// </summary>
            public string closeout_notcount { get; set; }

            /// <summary>
            /// Gets or sets the can_add_doc.
            /// </summary>
            public string can_add_doc { get; set; }

            /// <summary>
            /// Gets or sets the competing.
            /// </summary>
            public string competing { get; set; }

            /// <summary>
            /// Gets or sets the fsr_count.
            /// </summary>
            public string fsr_count { get; set; }

            /// <summary>
            /// Gets or sets the frc_destroyed.
            /// </summary>
            public string frc_destroyed { get; set; }

            /// <summary>
            /// Gets or sets the appl_fda_flag.
            /// </summary>
            public string appl_fda_flag { get; set; }

            /// <summary>
            /// Gets or sets the appl_ms_flag.
            /// </summary>
            public string appl_ms_flag { get; set; }

            /// <summary>
            /// Gets or sets the appl_od_flag.
            /// </summary>
            public string appl_od_flag { get; set; }

            /// <summary>
            /// Gets or sets the appl_ds_flag.
            /// </summary>
            public string appl_ds_flag { get; set; }

            /// <summary>
            /// Gets or sets the closeout_flag.
            /// </summary>
            public string closeout_flag { get; set; }

            /// <summary>
            /// Gets or sets the irppr_id.
            /// </summary>
            public string irppr_id { get; set; }

            /// <summary>
            /// Gets or sets the can_add_funding.
            /// </summary>
            public string can_add_funding { get; set; }
        }

        /// <summary>
        /// The doclayer.
        /// </summary>
        public class doclayer
        {
            /// <summary>
            /// Gets or sets the appl_id.
            /// </summary>
            public string appl_id { get; set; }

            /// <summary>
            /// Gets or sets the docs_count.
            /// </summary>
            public string docs_count { get; set; }

            /// <summary>
            /// Gets or sets the grant_id.
            /// </summary>
            public string grant_id { get; set; }

            /// <summary>
            /// Gets or sets the full_grant_num.
            /// </summary>
            public string full_grant_num { get; set; }

            /// <summary>
            /// Gets or sets the document_id.
            /// </summary>
            public string document_id { get; set; }

            /// <summary>
            /// Gets or sets the document_date.
            /// </summary>
            public string document_date { get; set; }

            /// <summary>
            /// Gets or sets the document_name.
            /// </summary>
            public string document_name { get; set; }

            /// <summary>
            /// Gets or sets the doc_date.
            /// </summary>
            public DateTime doc_date { get; set; }

            /// <summary>
            /// Gets or sets the category_id.
            /// </summary>
            public string category_id { get; set; }

            /// <summary>
            /// Gets or sets the category_name.
            /// </summary>
            public string category_name { get; set; }

            /// <summary>
            /// Gets or sets the sub_category_name.
            /// </summary>
            public string sub_category_name { get; set; }

            /// <summary>
            /// Gets or sets the created_by.
            /// </summary>
            public string created_by { get; set; }

            /// <summary>
            /// Gets or sets the created_date.
            /// </summary>
            public string created_date { get; set; }

            /// <summary>
            /// Gets or sets the modified_by.
            /// </summary>
            public string modified_by { get; set; }

            /// <summary>
            /// Gets or sets the modified_date.
            /// </summary>
            public string modified_date { get; set; }

            /// <summary>
            /// Gets or sets the file_modified_by.
            /// </summary>
            public string file_modified_by { get; set; }

            /// <summary>
            /// Gets or sets the file_modified_date.
            /// </summary>
            public string file_modified_date { get; set; }

            /// <summary>
            /// Gets or sets the problem_msg.
            /// </summary>
            public string problem_msg { get; set; }

            /// <summary>
            /// Gets or sets the problem_reported_by.
            /// </summary>
            public string problem_reported_by { get; set; }

            /// <summary>
            /// Gets or sets the page_count.
            /// </summary>
            public string page_count { get; set; }

            /// <summary>
            /// Gets or sets the fsr_count.
            /// </summary>
            public string fsr_count { get; set; }

            /// <summary>
            /// Gets or sets the attachment_count.
            /// </summary>
            public string attachment_count { get; set; }

            /// <summary>
            /// Gets or sets the closeout_notcount.
            /// </summary>
            public string closeout_notcount { get; set; }

            /// <summary>
            /// Gets or sets the frc_destroyed.
            /// </summary>
            public string frc_destroyed { get; set; }

            /// <summary>
            /// Gets or sets the url.
            /// </summary>
            public string url { get; set; }

            /// <summary>
            /// Gets or sets the qc_date.
            /// </summary>
            public string qc_date { get; set; }

            /// <summary>
            /// Gets or sets the can_qc.
            /// </summary>
            public string can_qc { get; set; }

            /// <summary>
            /// Gets or sets the can_upload.
            /// </summary>
            public string can_upload { get; set; }

            /// <summary>
            /// Gets or sets the can_modify_index.
            /// </summary>
            public string can_modify_index { get; set; }

            /// <summary>
            /// Gets or sets the can_delete.
            /// </summary>
            public string can_delete { get; set; }

            /// <summary>
            /// Gets or sets the can_restore.
            /// </summary>
            public string can_restore { get; set; }

            /// <summary>
            /// Gets or sets the can_store.
            /// </summary>
            public string can_store { get; set; }

            /// <summary>
            /// Gets or sets the ic.
            /// </summary>
            public string ic { get; set; }
        }

        /// <summary>
        /// The search.
        /// </summary>
        public class Search
        {
            /// <summary>
            /// Gets or sets the grantlayerproperty.
            /// </summary>
            public static List<grantlayer> grantlayerproperty { get; set; }

            /// <summary>
            /// Gets or sets the appllayerproperty.
            /// </summary>
            public static List<appllayer> appllayerproperty { get; set; }

            /// <summary>
            /// Gets or sets the doclayerproperty.
            /// </summary>
            public static List<doclayer> doclayerproperty { get; set; }

            /// <summary>
            /// Gets or sets the doclayerproperty_era.
            /// </summary>
            public static List<doclayer> doclayerproperty_era { get; set; }

            /// <summary>
            /// The egrants_search.
            /// </summary>
            /// <param name="str">
            /// The str.
            /// </param>
            /// <param name="grant_id">
            /// The grant_id.
            /// </param>
            /// <param name="package">
            /// The package.
            /// </param>
            /// <param name="appl_id">
            /// The appl_id.
            /// </param>
            /// <param name="current_page">
            /// The current_page.
            /// </param>
            /// <param name="browser">
            /// The browser.
            /// </param>
            /// <param name="ic">
            /// The ic.
            /// </param>
            /// <param name="userid">
            /// The userid.
            /// </param>
            public static void egrants_search(
                string str,
                int grant_id,
                string package,
                int appl_id,
                int current_page,
                string browser,
                string ic,
                string userid)
            {
                var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
                var cmd = new SqlCommand("dbo.sp_web_egrants", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@str", SqlDbType.NVarChar).Value = str;
                cmd.Parameters.Add("@grant_id", SqlDbType.Int).Value = grant_id;
                cmd.Parameters.Add("@package", SqlDbType.VarChar).Value = package;
                cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = appl_id;
                cmd.Parameters.Add("@current_page", SqlDbType.Int).Value = current_page;
                cmd.Parameters.Add("@browser", SqlDbType.VarChar).Value = browser;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

                conn.Open();

                grantlayerproperty = null;
                appllayerproperty = null;
                doclayerproperty = null;

                var grantList = new List<grantlayer>();
                var applList = new List<appllayer>();
                var docList = new List<doclayer>();

                var rdr = cmd.ExecuteReader();

                var tag = 0;

                while (rdr.Read())
                {
                    tag = Convert.ToInt32(rdr["tag"]);

                    if (tag == 1)
                    {
                        var grant = new grantlayer();
                        grant.grant_id = rdr["grant_id"]?.ToString();
                        grant.org_name = rdr["org_name"]?.ToString();
                        grant.serial_num = rdr["serial_num"]?.ToString();
                        grant.grant_num = string.Concat(rdr["admin_phs_org_code"] + Convert.ToInt32(rdr["serial_num"]).ToString("000000"));
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
                        grant.institutional_flag1 = rdr["institutional_flag1"].ToString() == "1" ? true : false;
                        grant.AnyOrgDoc = rdr["institutional_flag2"].ToString() == "1" ? true : false;
                        
                        grant.inst_flag1_url = rdr["inst_flag1_url"].ToString();

                        // grant.inst_flag2_url = rdr["inst_flag2_url"].ToString();
                        grantList.Add(grant);
                    }
                    else if (tag == 2)
                    {
                        var appl = new appllayer();
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
                        var doc = new doclayer();
                        doc.appl_id = rdr["appl_id"]?.ToString();
                        doc.docs_count = rdr["docs_count"]?.ToString();

                        docList.Add(doc);
                    }
                }

                // added by Leon 5/11/2019
                conn.Close();

                grantlayerproperty = grantList;
                appllayerproperty = applList;
                doclayerproperty = docList;
            }
        }

        /// <summary>
        /// The pagination.
        /// </summary>
        public class Pagination
        {
            /// <summary>
            /// Gets or sets the tag.
            /// </summary>
            public string tag { get; set; }

            /// <summary>
            /// Gets or sets the parent.
            /// </summary>
            public string parent { get; set; }

            /// <summary>
            /// Gets or sets the total_grants.
            /// </summary>
            public string total_grants { get; set; }

            /// <summary>
            /// Gets or sets the total_tabs.
            /// </summary>
            public string total_tabs { get; set; }

            /// <summary>
            /// Gets or sets the total_pages.
            /// </summary>
            public string total_pages { get; set; }

            /// <summary>
            /// Gets or sets the tab_number.
            /// </summary>
            public string tab_number { get; set; }

            /// <summary>
            /// Gets or sets the page_number.
            /// </summary>
            public string page_number { get; set; }
        }

        /// <summary>
        /// The stop notice.
        /// </summary>
        public class StopNotice
        {
            /// <summary>
            /// Gets or sets the appl_id.
            /// </summary>
            public string appl_id { get; set; }

            /// <summary>
            /// Gets or sets the full_grant_num.
            /// </summary>
            public string full_grant_num { get; set; }

            /// <summary>
            /// Gets or sets the closeout_fsr_code.
            /// </summary>
            public string closeout_fsr_code { get; set; }

            /// <summary>
            /// Gets or sets the final_invention_stmnt_code.
            /// </summary>
            public string final_invention_stmnt_code { get; set; }

            /// <summary>
            /// Gets or sets the final_report_date.
            /// </summary>
            public string final_report_date { get; set; }
        }

        /// <summary>
        /// The supplement.
        /// </summary>
        public class supplement
        {
            /// <summary>
            /// Gets or sets the tag.
            /// </summary>
            public string tag { get; set; }

            /// <summary>
            /// Gets or sets the grant_id.
            /// </summary>
            public string grant_id { get; set; }

            /// <summary>
            /// Gets or sets the admin_phs_org_code.
            /// </summary>
            public string admin_phs_org_code { get; set; }

            /// <summary>
            /// Gets or sets the serial_num.
            /// </summary>
            public string serial_num { get; set; }

            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// Gets or sets the full_grant_num.
            /// </summary>
            public string full_grant_num { get; set; }

            /// <summary>
            /// Gets or sets the supp_appl_id.
            /// </summary>
            public string supp_appl_id { get; set; }

            /// <summary>
            /// Gets or sets the support_year.
            /// </summary>
            public string support_year { get; set; }

            /// <summary>
            /// Gets or sets the suffix_code.
            /// </summary>
            public string suffix_code { get; set; }

            /// <summary>
            /// Gets or sets the former_num.
            /// </summary>
            public string former_num { get; set; }

            /// <summary>
            /// Gets or sets the submitted_date.
            /// </summary>
            public string submitted_date { get; set; }

            /// <summary>
            /// Gets or sets the category_name.
            /// </summary>
            public string category_name { get; set; }

            /// <summary>
            /// Gets or sets the status.
            /// </summary>
            public string status { get; set; }

            /// <summary>
            /// Gets or sets the url.
            /// </summary>
            public string url { get; set; }

            /// <summary>
            /// Gets or sets the moved_date.
            /// </summary>
            public string moved_date { get; set; }

            /// <summary>
            /// Gets or sets the moved_by.
            /// </summary>
            public string moved_by { get; set; }
        }

        // load documents by appl_id
        /// <summary>
        /// The search_by_appl_id.
        /// </summary>
        public class Search_by_appl_id
        {
            /// <summary>
            /// Gets or sets the doclayerproperty.
            /// </summary>
            public static List<doclayer> doclayerproperty { get; set; }

            /// <summary>
            /// Gets or sets the doclayerproperty_era.
            /// </summary>
            public static List<doclayer> doclayerproperty_era { get; set; }

            /// <summary>
            /// The load docs.
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
            /// <param name="ic">
            /// The ic.
            /// </param>
            /// <param name="userid">
            /// The userid.
            /// </param>
            public static void LoadDocs(int appl_id, string search_type, string category_list, string ic, string userid)
            {
                var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
                var cmd = new SqlCommand("sp_web_egrants_search_by_appl_id", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = appl_id;
                cmd.Parameters.Add("@search_type", SqlDbType.VarChar).Value = search_type;
                cmd.Parameters.Add("@category_list", SqlDbType.VarChar).Value = category_list;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

                // cmd.CommandTimeout = 120;
                conn.Open();

                doclayerproperty = null;
                var docList = new List<doclayer>();

                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var doc = new doclayer();
                    doc.grant_id = rdr["grant_id"]?.ToString();
                    doc.appl_id = rdr["appl_id"]?.ToString();

                    // doc.full_grant_num = rdr["full_grant_num"]?.ToString();
                    doc.document_id = rdr["document_id"]?.ToString();
                    doc.document_date = rdr["document_date"]?.ToString();
                    doc.document_name = rdr["document_name"]?.ToString();

                    if (rdr["doc_date"] != DBNull.Value)
                        doc.doc_date = Convert.ToDateTime(rdr["doc_date"]);

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

                    // doc.ic = rdr["ic"]?.ToString();
                    docList.Add(doc);
                }

                // added by Leon 5/11/2019
                conn.Close();
                doclayerproperty = docList;
            }
        }
    }
}