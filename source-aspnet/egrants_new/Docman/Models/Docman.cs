using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Docman.Models
{
    public class Docman
    {
        public static List<DocmanCommon.DocmanContract> LoadDocmanContract(string str, int econ_id, int page_index, string browser, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_docman_search", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@str", System.Data.SqlDbType.VarChar).Value = str;
            cmd.Parameters.Add("@econ_id", System.Data.SqlDbType.Int).Value = econ_id;
            cmd.Parameters.Add("@page_index", System.Data.SqlDbType.Int).Value = page_index;
            cmd.Parameters.Add("@browser", System.Data.SqlDbType.VarChar).Value = browser;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            var DocmanContract = new List<DocmanCommon.DocmanContract>();
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DocmanContract.Add(new DocmanCommon.DocmanContract
                {
                    econ_id = rdr["econ_id"]?.ToString(),
                    piid = rdr["piid"]?.ToString(),
                    ref_piid = rdr["ref_piid"]?.ToString(),
                    full_contract_number = rdr["full_contract_number"]?.ToString(),
                    award_mod_num = rdr["award_mod_num"]?.ToString(),
                    rfp_number = rdr["rfp_number"]?.ToString(),
                    institution = rdr["vendor_name"]?.ToString(),
                    project_enddate = rdr["project_enddate"]?.ToString(),
                    close_out = rdr["close_out"]?.ToString(),
                    person_id = rdr["person_id"]?.ToString(),
                    specialist_name = rdr["specialist_name"]?.ToString(),
                    team_leader = rdr["team_leader"]?.ToString(),
                    branch_chief = rdr["branch_chief"]?.ToString(),
                    can_upload = rdr["can_upload"]?.ToString(),
                    document_count = rdr["document_count"]?.ToString(),
                    page_count = rdr["page_count"]?.ToString()
                });
            }
            conn.Close();
            return DocmanContract;
        }

        public class DocmanDocument
        {
            public string econ_id { get; set; }
            public string tag { get; set; }
            public string parent { get; set; }
            public string main_category_id { get; set; }
            public string main_category_name { get; set; }
            public string main_category_label { get; set; }
            public string main_category_order { get; set; }
            public string docs_with_mcat { get; set; }
            public string sub_category_id { get; set; }
            public string sub_category_name { get; set; }
            public string sub_category_label { get; set; }
            public string sub_category_order { get; set; }
            public string docs_with_ccat { get; set; }
            public string document_id { get; set; }
            public string document_date { get; set; }
            public string doc_label { get; set; }
            public string seq_num { get; set; }
            public string url { get; set; }
        }

        public static List<DocmanDocument> LoadDocmanDocument(int econ_id, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_docman_contract", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@econ_id", System.Data.SqlDbType.Int).Value = econ_id;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var EconDocument = new List<DocmanDocument>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                EconDocument.Add(new DocmanDocument
                {
                    tag = rdr["tag"]?.ToString(),
                    parent = rdr["parent"]?.ToString(),
                    main_category_id = rdr["main_category_id"]?.ToString(),
                    main_category_name = rdr["main_category_name"]?.ToString(),
                    main_category_label = rdr["main_category_label"]?.ToString(),
                    main_category_order = rdr["main_category_order"]?.ToString(),
                    docs_with_mcat = rdr["docs_with_mcat"]?.ToString(),
                    sub_category_id = rdr["sub_category_id"]?.ToString(),
                    sub_category_name = rdr["sub_category_name"]?.ToString(),
                    sub_category_label = rdr["sub_category_label"]?.ToString(),
                    sub_category_order = rdr["sub_category_order"]?.ToString(),
                    docs_with_ccat = rdr["docs_with_ccat"]?.ToString(),
                    document_id = rdr["document_id"]?.ToString(),
                    document_date = rdr["document_date"]?.ToString(),
                    doc_label = rdr["doc_label"]?.ToString(),
                    seq_num = rdr["seq_num"]?.ToString(),
                    url = rdr["url"]?.ToString()
                });
            }
            conn.Close();
            return EconDocument;
        }

        //modify doc index or delete doc
        public static string DocmanDocModify(string act, int econ_id, int ccat_id, string seq_num, string doc_date, string description, int doc_id, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_docman_doc_modify", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@econ_id", System.Data.SqlDbType.Int).Value = econ_id;
            cmd.Parameters.Add("@cat_identity", System.Data.SqlDbType.Int).Value = ccat_id;
            cmd.Parameters.Add("@seq_num", System.Data.SqlDbType.VarChar).Value = seq_num;
            cmd.Parameters.Add("@doc_date", System.Data.SqlDbType.VarChar).Value = doc_date;
            cmd.Parameters.Add("@description", System.Data.SqlDbType.VarChar).Value = description;
            cmd.Parameters.Add("@document_id", System.Data.SqlDbType.Int).Value = doc_id;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@return_notice", System.Data.SqlDbType.VarChar, 200);
            cmd.Parameters["@return_notice"].Direction = System.Data.ParameterDirection.Output;

            conn.Open();
            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();

            string return_notice = Convert.ToString(cmd.Parameters["@return_notice"].Value);
            return return_notice;
        }

        public class Pagination
        {
            public string tag { get; set; }
            public string parent { get; set; }
            public string total_contracts { get; set; }
            public string total_tabs { get; set; }
            public string total_pages { get; set; }
            public string per_page { get; set; }
            public string tab_number { get; set; }
            public string page_number { get; set; }
        }

        public static List<Pagination> LoadPagination(string str, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("dbo.sp_web_docman_pagination", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@str", System.Data.SqlDbType.VarChar).Value = str;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var DocmanPagination = new List<Pagination>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DocmanPagination.Add(new Pagination
                {
                    tag = rdr["tag"]?.ToString(),
                    parent = rdr["parent"]?.ToString(),
                    total_contracts = rdr["total_contracts"]?.ToString(),
                    total_tabs = rdr["total_tabs"]?.ToString(),
                    total_pages = rdr["total_pages"]?.ToString(),
                    per_page = rdr["per_page"]?.ToString(),
                    tab_number = rdr["tab_number"]?.ToString(),
                    page_number = rdr["page_number"]?.ToString()      
                });
            }
            conn.Close();
            return DocmanPagination;
        }

        //update doc or create new doc
        public static string GetDocmanDocID(string act, int econ_id, int cat_identity, string seq_num, string doc_date, string file_type, int document_id, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_docman_doc_upload", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@econ_id", System.Data.SqlDbType.Int).Value = econ_id;
            cmd.Parameters.Add("@cat_identity", System.Data.SqlDbType.Int).Value = cat_identity;           
            cmd.Parameters.Add("@seq_num", System.Data.SqlDbType.VarChar).Value = seq_num;
            cmd.Parameters.Add("@doc_date", System.Data.SqlDbType.VarChar).Value = doc_date;
            cmd.Parameters.Add("@file_type", System.Data.SqlDbType.VarChar).Value = file_type;
            cmd.Parameters.Add("@document_id", System.Data.SqlDbType.Int).Value = document_id;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@new_doc_id", System.Data.SqlDbType.VarChar, 100);
            cmd.Parameters["@new_doc_id"].Direction = System.Data.ParameterDirection.Output;
            conn.Open();

            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();

            var doc_id = Convert.ToString(cmd.Parameters["@new_doc_id"].Value);
            return doc_id;
        }

        public static List<DocmanCommon.DocmanDeletedDocuments> LoadDeletedDocs(string start_date, string end_date, string date_range, string ic, string userid) 
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_docman_doc_deleted", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@startdate", System.Data.SqlDbType.VarChar).Value = start_date;
            cmd.Parameters.Add("@enddate", System.Data.SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@date_range", System.Data.SqlDbType.VarChar).Value = date_range;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();
     
            var DocmanDeletedDocs = new List<DocmanCommon.DocmanDeletedDocuments>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DocmanDeletedDocs.Add(new DocmanCommon.DocmanDeletedDocuments
                {
                    econ_id = rdr["econ_id"]?.ToString(),
                    url_loc = rdr["url_loc"]?.ToString(),
                    piid = rdr["piid"]?.ToString(),
                    ref_piid = rdr["ref_piid"]?.ToString(),
                    ccat_name = rdr["ccat_name"]?.ToString(),
                    report_label = rdr["report_label"]?.ToString(),
                    disabled_date = rdr["disabled_date"]?.ToString(),
                    disabled_reason = rdr["disabled_reason"]?.ToString(),
                    disabled_by = rdr["disabled_by"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();

            return DocmanDeletedDocs;
        }
    }
}