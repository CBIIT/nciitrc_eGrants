using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using egrants_new.Models;

namespace egrants_new.Egrants.Models
{
    public class InstitutionalFilesRepo
    {
        private System.Data.SqlClient.SqlConnection conn;
       // private System.Data.SqlClient.SqlCommand cmd;

        public InstitutionalFilesRepo()
        {
            conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
        }



        //to load Character index for intitution name 
        public List<InsitutionalOrgNameIndex> LoadOrgNameCharacterIndices()
        {
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT index_id, character_index, index_seq from dbo.character_index order by index_seq", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var OrgNameCharacterIndexs = new List<InsitutionalOrgNameIndex>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                OrgNameCharacterIndexs.Add(new InsitutionalOrgNameIndex()
                {
                    IndexId = rdr["index_id"]?.ToString(),
                    CharacterIndex = rdr["character_index"]?.ToString(),
                    IndexSeq = rdr["index_seq"]?.ToString()
                });
            }
            conn.Close();
            return OrgNameCharacterIndexs;
        }

        //to load all intitution list
        //public List<InstitutionalOrg> LoadOrgList(InstitutionalFilesPageAction action, string str, int index_id, int org_id, int doc_id, int category_id, string file_type, string start_date, string end_date, string ic, string userid)
        public List<InstitutionalOrg> LoadOrgList( int index_id)

        {
            //string act = "";
            //switch (action)
            //{
            //    case InstitutionalFilesPageAction.ShowDocs:
            //        act = "show_docs";
            //        break;
            //    case InstitutionalFilesPageAction.ShowOrgs:
            //        act = "show_orgs";
            //        break;
            //    case InstitutionalFilesPageAction.CreateNew:
            //        act = "create_new";
            //        break;
            //    default:
            //        break;
            //}

            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_inst_files_show_orgs", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            //TODO:  This should branched to different Stored procedures based on the revision MAdhu does
            //cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            //cmd.Parameters.Add("@str", System.Data.SqlDbType.VarChar).Value = str;
            cmd.Parameters.Add("@index_id", System.Data.SqlDbType.Int).Value = index_id;
            //cmd.Parameters.Add("@org_id", System.Data.SqlDbType.Int).Value = org_id;
            //cmd.Parameters.Add("@doc_id", System.Data.SqlDbType.Int).Value = doc_id;
            //cmd.Parameters.Add("@category_id", System.Data.SqlDbType.Int).Value = category_id;
            //cmd.Parameters.Add("@file_type", System.Data.SqlDbType.VarChar).Value = file_type;
            //cmd.Parameters.Add("@start_date", System.Data.SqlDbType.VarChar).Value = start_date;
            //cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = end_date;
            //cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            //cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();

            var OrgList = new List<InstitutionalOrg>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                OrgList.Add(new InstitutionalOrg()
                {
                    OrgId = (int)rdr["org_id"],
                    OrgName = rdr["org_name"]?.ToString(),
                    CreatedBy = rdr["created_by"]?.ToString(),
                    CreatedDate= rdr["created_date"]?.ToString(),
                    EndDate = rdr["end_date"]?.ToString(),
                    SvUrl = rdr["sv_url"]?.ToString()
                });
            }
            conn.Close();
            return OrgList;
        }
        //sp_web_egrants_inst_files_search_orgs

        public List<InstitutionalOrg> SearchOrgList(string search_str)

        {

            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_inst_files_search_orgs", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@str", System.Data.SqlDbType.VarChar).Value = search_str;

            conn.Open();

            var OrgList = new List<InstitutionalOrg>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                OrgList.Add(new InstitutionalOrg()
                {
                    OrgId = (int)rdr["org_id"],
                    OrgName = rdr["org_name"]?.ToString(),
                    CreatedBy = rdr["created_by"]?.ToString(),
                    CreatedDate = rdr["created_date"]?.ToString(),
                    EndDate = rdr["end_date"]?.ToString(),
                    SvUrl = rdr["sv_url"]?.ToString()
                });
            }
            conn.Close();
            return OrgList;
        }

        //
        public InstitutionalOrg FindOrg( int org_id)
        {

            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_institutional_file_find_org", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            //TODO:  This should branched to different Stored procedures based on the revision MAdhu does

            cmd.Parameters.Add("@org_id", System.Data.SqlDbType.Int).Value = org_id;

            conn.Open();

            var Org = new InstitutionalOrg();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Org = new InstitutionalOrg()
                {
                    OrgId = (int)rdr["org_id"],
                    OrgName = rdr["org_name"]?.ToString(),
                    CreatedBy = rdr["created_by"]?.ToString(),
                    CreatedDate = rdr["created_date"]?.ToString(),
                    EndDate = rdr["end_date"]?.ToString(),
                    SvUrl = rdr["sv_url"]?.ToString()
                };
            }
            conn.Close();
            return Org;
        }



        //to load all intitutional files list   -   [sp_web_egrants_institutional_file_find_org]
        public List<InstitutionalDocFiles> LoadOrgDocList( int org_id)
        {

            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_inst_files_show_docs", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            //cmd.Parameters.Add("@str", System.Data.SqlDbType.VarChar).Value = str;
            //cmd.Parameters.Add("@index_id", System.Data.SqlDbType.Int).Value = index_id;
            cmd.Parameters.Add("@org_id", System.Data.SqlDbType.Int).Value = org_id;
            //cmd.Parameters.Add("@doc_id", System.Data.SqlDbType.Int).Value = doc_id;
            //cmd.Parameters.Add("@category_id", System.Data.SqlDbType.Int).Value = category_id;
            //cmd.Parameters.Add("@file_type", System.Data.SqlDbType.VarChar).Value = file_type;
            //cmd.Parameters.Add("@start_date", System.Data.SqlDbType.VarChar).Value = start_date;
            //cmd.Parameters.Add("@end_date", System.Data.SqlDbType.VarChar).Value = end_date;
            //cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            //cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();

            var DocFilesList = new List<InstitutionalDocFiles>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DocFilesList.Add(new InstitutionalDocFiles
                {
                    org_id = rdr["org_id"]?.ToString(),
                    org_name = rdr["org_name"]?.ToString(),
                    document_id = rdr["document_id"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    url = rdr["url"]?.ToString(),
                    start_date = rdr["start_date"]?.ToString(),
                    end_date = rdr["end_date"]?.ToString(),
                    created_date = rdr["created_date"]?.ToString(),
                    comments = rdr["comments"]?.ToString()
                });
            }
            conn.Close();
            return DocFilesList;
        }



        //to turn all categories for institutional file
        public List<InstitutionalOrgCategory> LoadOrgCategory()
        {
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT doctype_id AS category_id, doctype_name AS category_name, tobe_flagged AS tobe_flag, Flag_period, isnull(comments_required,0) as comments_required, active FROM dbo.Org_Categories where active=1  ORDER BY category_name ", conn);
            cmd.CommandType = CommandType.Text;

            conn.Open();

            var OrgCategoryList = new List<InstitutionalOrgCategory>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                OrgCategoryList.Add(new InstitutionalOrgCategory
                {
                    category_id= rdr["category_id"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    tobe_flag = rdr["tobe_flag"]?.ToString(),
                    flag_period = rdr["Flag_period"]?.ToString(),
                    flag_data = rdr["tobe_flag"]?.ToString() + "_" + rdr["Flag_period"]?.ToString(),
                    require_comments =(bool) rdr["comments_required"],
                    active = (bool)rdr["active"]

                });
            }
            conn.Close();
            return OrgCategoryList;
        }

        //to disable an institutional file
        public void DisableDoc(string act, string str, int index_id, int org_id, int doc_id, int category_id, string file_type, string start_date, string end_date, string ic, string userid)
        {

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

        //to create new institutional file and return document_id
        public string GetDocID(int org_id, int category_id, string file_type, string start_date, string end_date, string ic, string userid, string comments)
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
            cmd.Parameters.Add("@comments", System.Data.SqlDbType.VarChar).Value = comments;
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