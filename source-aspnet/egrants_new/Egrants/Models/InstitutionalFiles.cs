using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Egrants.Models
{
    public class InstitutionalFiles
    {
        public class OrgNameIndex
        {
            public string index_id { get; set; }
            public string character_index { get; set; }
            public string index_seq { get; set; }
        }

        //to load Character index for intitution name 
        public static List<OrgNameIndex> LoadOrgNameCharacterIndex()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT index_id, character_index, index_seq from dbo.character_index order by index_seq", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var OrgNameCharacterIndexs = new List<OrgNameIndex>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                OrgNameCharacterIndexs.Add(new OrgNameIndex
                {
                    index_id = rdr["index_id"]?.ToString(),
                    character_index = rdr["character_index"]?.ToString(),
                    index_seq = rdr["index_seq"]?.ToString()
                });
            }
            conn.Close();
            return OrgNameCharacterIndexs;
        }

        public class OrgFiles
        {
            public string tag { get; set; }
            public string org_id { get; set; }
            public string org_name { get; set; }
            public string created_by { get; set; }
            public string created_date { get; set; }
            public string end_date { get; set; }
            public string sv_url { get; set; }
        }

        //to load all intitution list
        public static List<OrgFiles> LoadOrgList(string act, string str, int index_id, int org_id, int doc_id, int category_id, string file_type, string start_date, string end_date, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_institutional_files", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@str", System.Data.SqlDbType.VarChar).Value = str;
            cmd.Parameters.Add("@index_id", System.Data.SqlDbType.Int).Value = index_id;
            cmd.Parameters.Add("@org_id", System.Data.SqlDbType.Int).Value = org_id;
            cmd.Parameters.Add("@doc_id", System.Data.SqlDbType.Int).Value = doc_id;
            cmd.Parameters.Add("@category_id", System.Data.SqlDbType.Int).Value = category_id;
            cmd.Parameters.Add("@file_type", System.Data.SqlDbType.VarChar).Value = file_type;
            cmd.Parameters.Add("@start_date", System.Data.SqlDbType.VarChar).Value = start_date;
            cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();

            var OrgFilesList = new List<OrgFiles>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                OrgFilesList.Add(new OrgFiles
                {
                    tag = rdr["tag"]?.ToString(),
                    org_id = rdr["org_id"]?.ToString(),
                    org_name = rdr["org_name"]?.ToString(),
                    created_by = rdr["created_by"]?.ToString(),
                    created_date= rdr["created_date"]?.ToString(),
                    end_date = rdr["end_date"]?.ToString(),
                    sv_url = rdr["sv_url"]?.ToString()
                });
            }
            conn.Close();
            return OrgFilesList;
        }

        public class DocFiles
        {
            public string tag { get; set; }
            public string org_id { get; set; }
            public string org_name { get; set; }
            public string document_id { get; set; }
            public string category_name { get; set; }
            public string url { get; set; }
            public string start_date { get; set; }
            public string end_date { get; set; }
            public string created_date { get; set; }
        }

        //to load all intitutional files list
        public static List<DocFiles> LoadOrgDocList(string act, string str, int index_id, int org_id, int doc_id, int category_id, string file_type, string start_date, string end_date, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_institutional_files", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@str", System.Data.SqlDbType.VarChar).Value = str;
            cmd.Parameters.Add("@index_id", System.Data.SqlDbType.Int).Value = index_id;
            cmd.Parameters.Add("@org_id", System.Data.SqlDbType.Int).Value = org_id;
            cmd.Parameters.Add("@doc_id", System.Data.SqlDbType.Int).Value = doc_id;
            cmd.Parameters.Add("@category_id", System.Data.SqlDbType.Int).Value = category_id;
            cmd.Parameters.Add("@file_type", System.Data.SqlDbType.VarChar).Value = file_type;
            cmd.Parameters.Add("@start_date", System.Data.SqlDbType.VarChar).Value = start_date;
            cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();

            var DocFilesList = new List<DocFiles>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DocFilesList.Add(new DocFiles
                {
                    tag = rdr["tag"]?.ToString(),
                    org_id = rdr["org_id"]?.ToString(),
                    org_name = rdr["org_name"]?.ToString().Replace("'","\'"),
                    document_id = rdr["document_id"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    url = rdr["url"]?.ToString(),
                    start_date = rdr["start_date"]?.ToString(),
                    end_date = rdr["end_date"]?.ToString(),
                    created_date = rdr["created_date"]?.ToString()
                });
            }
            conn.Close();
            return DocFilesList;
        }

        public class OrgCategory
        {
            public string category_id { get; set; }
            public string category_name { get; set; }
            public string tobe_flag { get; set; }
            public string flag_period { get; set; }
            public string flag_data { get; set; }
            public string today { get; set; }
        }

        //to turn all categories for intitutional file
        public static List<OrgCategory> LoadOrgCategory()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT doctype_id AS category_id, doctype_name AS category_name, tobe_flagged AS tobe_flag, Flag_period FROM dbo.Org_Categories ORDER BY category_name", conn);
            cmd.CommandType = CommandType.Text;

            conn.Open();

            var OrgCategoryList = new List<OrgCategory>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                OrgCategoryList.Add(new OrgCategory
                {
                    category_id= rdr["category_id"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    tobe_flag = rdr["tobe_flag"]?.ToString(),
                    flag_period = rdr["Flag_period"]?.ToString(),
                    flag_data = rdr["tobe_flag"]?.ToString() + "_" + rdr["Flag_period"]?.ToString()
                });
            }
            conn.Close();
            return OrgCategoryList;
        }

        //to disable an intitutional file
        public static void DisableDoc(string act, string str, int index_id, int org_id, int doc_id, int category_id, string file_type, string start_date, string end_date, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_institutional_files", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@str", System.Data.SqlDbType.VarChar).Value = str;
            cmd.Parameters.Add("@index_id", System.Data.SqlDbType.Int).Value = index_id;
            cmd.Parameters.Add("@org_id", System.Data.SqlDbType.Int).Value = org_id;
            cmd.Parameters.Add("@doc_id", System.Data.SqlDbType.Int).Value = doc_id;
            cmd.Parameters.Add("@category_id", System.Data.SqlDbType.Int).Value = category_id;
            cmd.Parameters.Add("@file_type", System.Data.SqlDbType.VarChar).Value = file_type;
            cmd.Parameters.Add("@start_date", System.Data.SqlDbType.VarChar).Value = start_date;
            cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();

            SqlDataReader rdr = cmd.ExecuteReader();

            conn.Close();
        }

        //to create new intitutional file and return document_id
        public static string GetDocID(int org_id, int category_id, string file_type, string start_date, string end_date, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_institutional_file_create", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.Add("@org_id", System.Data.SqlDbType.Int).Value = org_id;
            cmd.Parameters.Add("@category_id", System.Data.SqlDbType.Int).Value = category_id;
            cmd.Parameters.Add("@file_type", System.Data.SqlDbType.VarChar).Value = file_type;
            cmd.Parameters.Add("@start_date", System.Data.SqlDbType.VarChar).Value = start_date;
            cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@document_id", System.Data.SqlDbType.VarChar, 100);
            cmd.Parameters["@document_id"].Direction = System.Data.ParameterDirection.Output;
            conn.Open();

            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();

            var document_id = Convert.ToString(cmd.Parameters["@document_id"].Value);
            return document_id;
        }
    }
}