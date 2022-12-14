#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  Egrants.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-11-21
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
    ///     The egrants.
    /// </summary>
    public static class Egrants
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
        /// The <see cref="System.Collections.Generic.List`1"/> .
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

            var list = new List<Pagination>();

            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                list.Add(
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

            return list;
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
        /// The <see cref="System.Collections.Generic.List`1"/> .
        /// </returns>
        public static List<StopNoticeObject> LoadStopNotice(int grant_id, string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_stop_notice ", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@GrantID", SqlDbType.Int).Value = grant_id;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;

            conn.Open();

            var list = new List<StopNoticeObject>();

            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                list.Add(
                    new StopNoticeObject
                        {
                            appl_id = rdr["appl_id"]?.ToString(),
                            full_grant_num = rdr["full_grant_num"]?.ToString(),
                            closeout_fsr_code = rdr["closeout_fsr_code"]?.ToString(),
                            final_invention_stmnt_code = rdr["final_invention_stmnt_code"]?.ToString(),
                            final_report_date = rdr["final_report_date"]?.ToString()
                        });

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
        /// The <see cref="System.Collections.Generic.List`1"/> .
        /// </returns>
        public static List<SupplementObject> LoadSupplement(
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

            var list = new List<SupplementObject>();

            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                list.Add(
                    new SupplementObject
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

            conn.Close();

            return list;
        }

        // Phase2
        /// <summary>
        ///     The get ic list.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
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

        /// <summary>
        /// The get search query. search by filters
        /// </summary>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism
        /// </param>
        /// <param name="adminCode">
        /// The adminCode.
        /// </param>
        /// <param name="serialNumber">
        /// The serialNumber.
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
        /// The <see cref="string"/> .
        /// </returns>
        public static string GetSearchQuery(
            int fy,
            string mechanism,
            string adminCode,
            int serialNumber,
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
                cmd.Parameters.AddWithValue("@adminCode", adminCode);
                cmd.Parameters.AddWithValue("@serialNumber", serialNumber);
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

        /// <summary>
        /// The get year list.
        /// </summary>
        /// <param name="fiscalYear">
        /// The fiscal year.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism.
        /// </param>
        /// <param name="adminCode">
        /// The admin code.
        /// </param>
        /// <param name="serialNumber">
        /// The serial number.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<string> GetYearList(string fiscalYear = null, string mechanism = null, string adminCode = null, string serialNumber = null)
        {
            var yearList = new List<string>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("sp_web_egrants_load_data_years", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fy", fiscalYear);
                cmd.Parameters.AddWithValue("@mechanism", mechanism);
                cmd.Parameters.AddWithValue("@admincode", adminCode);
                cmd.Parameters.AddWithValue("@serialnum", serialNumber);

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
        /// The <see cref="System.Collections.Generic.List`1"/> .
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
        /// The <see cref="string"/> .
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
            {
                CategoryNameList = CategoryNameList.Substring(0, CategoryNameList.Length - 2);
            }

            return CategoryNameList;
        }

        /// <summary>
        /// The get grant id.
        /// </summary>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <returns>
        /// The <see cref="int"/> .
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
                {
                    grant_id = Convert.ToInt32(rdr["grant_id"]);
                }


                return grant_id;

                // conn.Open();
                // int grant_id = (int)cmd.ExecuteScalar();
                // conn.Close();
                // return grant_id;
            }
        }

        /// <summary>
        /// Check if this <paramref name="grant_id"/> is existing in grants
        ///     table, added by Leon 7/10/2019
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="int"/> .
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

                //conn.Close();

                return exists;
            }
        }
    }
}