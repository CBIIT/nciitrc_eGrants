#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EgrantsFunding.cs
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

using egrants_new.Egrants.Models;

#endregion

namespace egrants_new.Egrants_Funding.Models
{

    public static class EgrantsFunding
    {
        /// <summary>
        ///     to load funding category list by fiscal_year
        /// </summary>
        /// <param name="fiscal_year"></param>
        /// <returns></returns>
        public static List<FundingCategories> LoadFundingCategories(int fiscal_year)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand("SELECT level_id, ISNULL(parent_id,0) as parent_id,category_id,category_name,category_fy," +
                                     " dbo.fn_funding_child_count(category_id,@fy) as child_count, dbo.fn_funding_doc_count(category_id,@fy) as doc_count "
                                    +
                                     " FROM vw_funding_categories WHERE category_fy is null or category_fy = @fy ORDER BY category_name",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@fy", SqlDbType.Int).Value = fiscal_year;
            conn.Open();

            var list = new List<FundingCategories>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                list.Add(new FundingCategories
                             {
                                 level_id = rdr["level_id"]?.ToString(),
                                 parent_id = rdr["parent_id"]?.ToString(),
                                 category_id = rdr["category_id"]?.ToString(),
                                 category_name = rdr["category_name"]?.ToString(),
                                 child_count = rdr["child_count"]?.ToString(),
                                 doc_count = rdr["doc_count"]?.ToString()
                             });

            conn.Close();

            return list;
        }

        public static List<FundingDocuments> LoadFundingDocs(string act, int serial_num, int fiscal_year, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_funding_docs", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@serial_num", SqlDbType.Int).Value = serial_num;
            cmd.Parameters.Add("@fy", SqlDbType.Int).Value = fiscal_year;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@Operator", SqlDbType.VarChar).Value = userid;
            conn.Open();
            var list = new List<FundingDocuments>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                list.Add(new FundingDocuments
                             {
                                 document_id = rdr["document_id"]?.ToString(),
                                 doc_label = rdr["doc_label"]?.ToString(),
                                 category_id = rdr["category_id"]?.ToString(),
                                 category_name = rdr["category_name"]?.ToString(),
                                 document_fy = rdr["document_fy"]?.ToString(),
                                 url = rdr["url"]?.ToString(),
                                 created_date = rdr["created_date"]?.ToString(),
                                 arra_flag = rdr["arra_flag"]?.ToString(),
                                 serial_num = rdr["serial_num"]?.ToString(),
                                 appl_id = rdr["appl_id"]?.ToString(),
                                 full_grant_num = rdr["full_grant_num"]?.ToString()
                             });

            conn.Close();

            return list;
        }

        //to create new funding document and return new document_id
        public static string GetFundingDocID(int appl_id,
                                             int category_id,
                                             string doc_date,
                                             string sub_category,
                                             string file_type,
                                             string ic,
                                             string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_funding_doc_create", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ApplID", SqlDbType.Int).Value = appl_id;
            cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = category_id;
            cmd.Parameters.Add("@DocDate", SqlDbType.DateTime).Value = doc_date;
            cmd.Parameters.Add("@SubCategory", SqlDbType.VarChar).Value = sub_category;
            cmd.Parameters.Add("@FileType", SqlDbType.VarChar).Value = file_type;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@DocumentID", SqlDbType.Int);
            cmd.Parameters["@DocumentID"].Direction = ParameterDirection.Output;
            conn.Open();

            var dataReader = cmd.ExecuteReader();

            dataReader.Close();
            conn.Close();

            var documentId = Convert.ToString(cmd.Parameters["@DocumentID"].Value);

            return documentId;
        }

        //to load funding category list without fiscal_year
        public static List<FundingCategories> LoadFundingCategoryList()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);

            var cmd = new SqlCommand("SELECT distinct category_id,category_name,level_id,parent_id FROM funding_categories " +
                                     "WHERE category_fy is null or category_fy = 2014 Order by level_id, category_name",
                conn);

            cmd.CommandType = CommandType.Text;

            conn.Open();

            var list = new List<FundingCategories>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                list.Add(new FundingCategories
                             {
                                 category_id = rdr["category_id"]?.ToString(),
                                 category_name = rdr["category_name"]?.ToString(),
                                 level_id = rdr["level_id"]?.ToString(),
                                 parent_id = rdr["parent_id"]?.ToString()
                             });

            conn.Close();

            return list;
        }

        //to return Max Category id with fiscal_year
        public static int GetMaxCategoryid(int fiscal_year)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT max(category_id) as max_categoryid FROM funding_categories WHERE category_fy is null or category_fy= @fy",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@fy", SqlDbType.Int).Value = fiscal_year;
            conn.Open();
            var MaxCategoryid = 0;
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                MaxCategoryid = Convert.ToInt16(rdr["max_categoryid"]?.ToString());

            conn.Close();

            return MaxCategoryid;
        }

        //to load all appls by document_id
        public static List<EgrantsAppl.Appl> LoadDocAppls(int doc_id)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand("SELECT distinct appl.appl_id, appl.support_year,appl.full_grant_num " +
                                     " FROM vw_appls as appl, vw_funding f " +
                                     " WHERE f.appl_id = appl.appl_id and f.document_id = @doc_id and f.disabled_date is null",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = doc_id;
            conn.Open();

            var list = new List<EgrantsAppl.Appl>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                list.Add(new EgrantsAppl.Appl
                             {
                                 appl_id = rdr["appl_id"]?.ToString(),
                                 support_year = rdr["support_year"]?.ToString(),
                                 full_grant_num = rdr["full_grant_num"]?.ToString()
                             });

            return list;
        }

        //to load all appls with funding document expect appl with that document
        public static List<EgrantsAppl.Appl> LoadFullGrantNumbers(int serial_num, string admin_code, int doc_id)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT appl_id, support_year, full_grant_num FROM vw_appls WHERE admin_phs_org_code = @admin_code and serial_num = @serial_num and "
               +
                "appl_id not in (SELECT appl_id FROM funding_appls WHERE document_id = @doc_id ) order by support_year desc",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@serial_num", SqlDbType.Int).Value = serial_num;
            cmd.Parameters.Add("@admin_code", SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = doc_id;
            conn.Open();

            var list = new List<EgrantsAppl.Appl>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                list.Add(new EgrantsAppl.Appl
                             {
                                 appl_id = rdr["appl_id"]?.ToString(),
                                 support_year = rdr["support_year"]?.ToString(),
                                 full_grant_num = rdr["full_grant_num"]?.ToString()
                             });

            return list;
        }

        //to edit funding document for delete or store
        public static void EditFundingDoc(string act, int appl_id, int doc_id, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_funding_doc_edit", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = appl_id;
            cmd.Parameters.Add("@document_id", SqlDbType.Int).Value = doc_id;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@Operator", SqlDbType.VarChar).Value = userid;
            conn.Open();
            var dataReader = cmd.ExecuteReader();
            dataReader.Close();
            conn.Close();
        }

        //to edit funding document with remove or add appl
        public static void EditFundingAppl(string act, int appl_id, int doc_id, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_funding_appl_edit", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = appl_id;
            cmd.Parameters.Add("@document_id", SqlDbType.Int).Value = doc_id;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@Operator", SqlDbType.VarChar).Value = userid;
            conn.Open();
            var dataReader = cmd.ExecuteReader();
            dataReader.Close();
            conn.Close();
        }
    }
}