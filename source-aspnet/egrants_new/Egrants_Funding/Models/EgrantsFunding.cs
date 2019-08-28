using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;


namespace egrants_new.Egrants_Funding.Models
{
    public class EgrantsFunding
    {
        public class FundingCategories
        {
            public string level_id { get; set; }
            public string parent_id { get; set; }
            public string category_id { get; set; }
            public string category_name { get; set; }
            public string category_fy { get; set; }
            public string child_count { get; set; }
            public string doc_count { get; set; }
        }

        //to load funding category list by fiscal_year
        public static List<FundingCategories> LoadFundingCategories(int fiscal_year)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT level_id, ISNULL(parent_id,0) as parent_id,category_id,category_name,category_fy," +
            " dbo.fn_funding_child_count(category_id,@fy) as child_count, dbo.fn_funding_doc_count(category_id,@fy) as doc_count " +
            " FROM vw_funding_categories WHERE category_fy is null or category_fy = @fy ORDER BY category_name", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@fy", System.Data.SqlDbType.Int).Value = fiscal_year;
            conn.Open();

            var FundingCategories = new List<FundingCategories>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                FundingCategories.Add(new FundingCategories
                {
                    level_id = rdr["level_id"]?.ToString(),
                    parent_id = rdr["parent_id"]?.ToString(),
                    category_id = rdr["category_id"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    child_count = rdr["child_count"]?.ToString(),
                    doc_count = rdr["doc_count"]?.ToString(),
                });
            }
            conn.Close();
            return FundingCategories;
        }

        public class FundingDocuments
        {
            public string serial_num { get; set; }
            public string appl_id { get; set; }
            public string full_grant_num { get; set; }
            public string document_id { get; set; }
            public string doc_label { get; set; }
            public string url { get; set; }
            public string category_id { get; set; }
            public string category_name { get; set; }
            public string document_fy { get; set; }
            public string created_date { get; set; }
            public string arra_flag { get; set; }                 
        }

        //to load funding documents
        public static List<FundingDocuments> LoadFundingDocs(string act, int serial_num, int fiscal_year, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_funding_docs", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@serial_num", System.Data.SqlDbType.Int).Value = serial_num;
            cmd.Parameters.Add("@fy", System.Data.SqlDbType.Int).Value = fiscal_year;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@Operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();
            var FundingDocuments = new List<FundingDocuments>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                FundingDocuments.Add(new FundingDocuments
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
                    full_grant_num = rdr["full_grant_num"]?.ToString(),
                });
            }
            conn.Close();
            return FundingDocuments;
        }

        //to create new funding document and return new document_id
        public static string GetFundingDocID(int appl_id, int category_id, string doc_date, string sub_category, string file_type, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_funding_doc_create", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ApplID", System.Data.SqlDbType.Int).Value = appl_id;
            cmd.Parameters.Add("@CategoryID", System.Data.SqlDbType.Int).Value = category_id;
            cmd.Parameters.Add("@DocDate", System.Data.SqlDbType.DateTime).Value = doc_date;
            cmd.Parameters.Add("@SubCategory", System.Data.SqlDbType.VarChar).Value = sub_category;
            cmd.Parameters.Add("@FileType", System.Data.SqlDbType.VarChar).Value = file_type;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@DocumentID", System.Data.SqlDbType.Int);
            cmd.Parameters["@DocumentID"].Direction = System.Data.ParameterDirection.Output;
            conn.Open();

            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();

            var document_id = Convert.ToString(cmd.Parameters["@DocumentID"].Value);
            return document_id;
        }

        //to load funding category list without fiscal_year
        public static List<FundingCategories> LoadFundingCategoryList()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT distinct category_id,category_name,level_id,parent_id FROM funding_categories " +
            "WHERE category_fy is null or category_fy = 2014 Order by level_id, category_name", conn);
            cmd.CommandType = CommandType.Text;

            conn.Open();

            var FundingCategoryList = new List<FundingCategories>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                FundingCategoryList.Add(new FundingCategories
                {
                    category_id = rdr["category_id"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    level_id = rdr["level_id"]?.ToString(),
                    parent_id = rdr["parent_id"]?.ToString(),
                });
            }
            conn.Close();
            return FundingCategoryList;
        }

        //to return Max Category id with fiscal_year
        public static int GetMaxCategoryid(int fiscal_year)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT max(category_id) as max_categoryid FROM funding_categories WHERE category_fy is null or category_fy= @fy", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@fy", System.Data.SqlDbType.Int).Value = fiscal_year;
            conn.Open();
            int MaxCategoryid = 0;
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                MaxCategoryid = Convert.ToInt16(rdr["max_categoryid"]?.ToString());
            }
            conn.Close();
            return MaxCategoryid;
        }
   
        //to load all appls by document_id
        public static List<Egrants.Models.EgrantsAppl.Appl> LoadDocAppls(int doc_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT distinct appl.appl_id, appl.support_year,appl.full_grant_num " +
            " FROM vw_appls as appl, vw_funding f " +
            " WHERE f.appl_id = appl.appl_id and f.document_id = @doc_id and f.disabled_date is null", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@doc_id", System.Data.SqlDbType.Int).Value = doc_id;
            conn.Open();

            var Appls = new List<Egrants.Models.EgrantsAppl.Appl>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Appls.Add(new Egrants.Models.EgrantsAppl.Appl
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    support_year = rdr["support_year"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString()
                });
            }
            return Appls;
        }

        //to load all appls with funding document expect appl with that document
        public static List<Egrants.Models.EgrantsAppl.Appl> LoadFullGrantNumbers(int serial_num, string admin_code, int doc_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT appl_id, support_year, full_grant_num FROM vw_appls WHERE admin_phs_org_code = @admin_code and serial_num = @serial_num and " +
            "appl_id not in (SELECT appl_id FROM funding_appls WHERE document_id = @doc_id ) order by support_year desc", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@serial_num", System.Data.SqlDbType.Int).Value = serial_num;
            cmd.Parameters.Add("@admin_code", System.Data.SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@doc_id", System.Data.SqlDbType.Int).Value = doc_id;
            conn.Open();

            var FullGrantNums = new List<Egrants.Models.EgrantsAppl.Appl>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                FullGrantNums.Add(new Egrants.Models.EgrantsAppl.Appl
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    support_year = rdr["support_year"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString()
                });
            }

            return FullGrantNums;
        }

        //to edit funding document for delete or store
        public static void EditFundingDoc(string act, int appl_id, int doc_id, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_funding_doc_edit", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@appl_id", System.Data.SqlDbType.Int).Value = appl_id;
            cmd.Parameters.Add("@document_id", System.Data.SqlDbType.Int).Value = doc_id;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@Operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();
            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();
            DataReader.Close();
            conn.Close();
        }

        //to edit funding document with remove or add appl
        public static void EditFundingAppl(string act, int appl_id, int doc_id, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_funding_appl_edit", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@appl_id", System.Data.SqlDbType.Int).Value = appl_id;
            cmd.Parameters.Add("@document_id", System.Data.SqlDbType.Int).Value = doc_id;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@Operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();
            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();
            DataReader.Close();
            conn.Close();        
        }
    }
}