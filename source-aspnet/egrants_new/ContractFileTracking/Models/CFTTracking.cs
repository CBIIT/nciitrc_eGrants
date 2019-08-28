using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.ContractFileTracking.Models
{
    public class CFTTracking
    {
        public class CFTContractFolders
        {
            public string location { get; set; }
            public string tag { get; set; }
            public string contract_id { get; set; }
            public string full_contract_number { get; set; }
            public string org_name { get; set; }
            public string url { get; set; }
            public string signed_by { get; set; }
            public string folder_id { get; set; }
            public string bar_code { get; set; }
            public string id_string { get; set; }
            public string status { get; set; }
            public string box_id { get; set; }
            public string latest_move { get; set; }
        }

        public class CFTInstitutions
        {
            public string institution { get; set; }
        }

        public static List<CFTInstitutions> LoadInstitutions()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select distinct institution FROM vw_contracts WHERE contract_id in(select distinct contract_id from contract_folders) order by institution", conn);
            cmd.CommandType = CommandType.Text;

            var CFTInstitutions = new List<CFTInstitutions>();
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                CFTInstitutions.Add(new CFTInstitutions { institution = rdr["institution"]?.ToString() });
            }
            conn.Close();

            return CFTInstitutions;
        }
        public static List<CFTContractFolders> LoadContractFolders(string SearchType, string SearchStr, string combined_piid, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_contract_tracking", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@search_type", System.Data.SqlDbType.VarChar).Value = SearchType;
            cmd.Parameters.Add("@search_str", System.Data.SqlDbType.VarChar).Value = SearchStr;
            cmd.Parameters.Add("@combined_piid", System.Data.SqlDbType.VarChar).Value = combined_piid;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            var ContractFolders = new List<CFTContractFolders>();
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                ContractFolders.Add(new CFTContractFolders
                {
                    location = rdr["location"]?.ToString(),
                    tag = rdr["tag"]?.ToString(),
                    contract_id = rdr["contract_id"]?.ToString(),
                    full_contract_number = rdr["full_contract_number"]?.ToString(),
                    org_name = rdr["org_name"]?.ToString(),
                    url = rdr["url"]?.ToString(),
                    signed_by = rdr["signed_by"]?.ToString(),
                    folder_id = rdr["folder_id"]?.ToString(),
                    bar_code = rdr["bar_code"]?.ToString(),
                    id_string = rdr["id_string"]?.ToString(),
                    status = rdr["status"]?.ToString(),
                    box_id = rdr["box_id"]?.ToString(),
                    latest_move = rdr["latest_move"]?.ToString()
                });
            }
            conn.Close();
            return ContractFolders;
        }

        public static List<DocmanCommon.DocmantBoxes> LoadBoxes(int accession_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            if (accession_id == 0)
            {
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select top 1 a.accession_id as accession_id from accessions a inner join vw_boxes b on a.accession_id = b.accession_id"
                    + " where a.contract = 1 order by a.accession_year desc, a.accession_counter desc", conn);
                cmd.CommandType = CommandType.Text;
                conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    accession_id = Convert.ToInt32(rdr["accession_id"]);
                }
                conn.Close();
            }

                System.Data.SqlClient.SqlCommand cmd2 = new System.Data.SqlClient.SqlCommand("SELECT distinct box_id, box as box_number FROM vw_boxes WHERE accession_id = @accession_id order by box_id desc", conn);
                cmd2.CommandType = CommandType.Text;
                cmd2.Parameters.Add("@accession_id", System.Data.SqlDbType.Int).Value = accession_id;
                conn.Open();

                var DocmanBoxes = new List<DocmanCommon.DocmantBoxes>();
                SqlDataReader rdr2 = cmd2.ExecuteReader();

                while (rdr2.Read())
                {
                    DocmanBoxes.Add(new DocmanCommon.DocmantBoxes { box_id = rdr2["box_id"]?.ToString(), box_number = rdr2["box_number"]?.ToString() });
                }
                conn.Close();

                return DocmanBoxes;        
        }

        public static void FolderEdit(string act, string folders, string target_id, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_contract_folder_edit", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@folders", System.Data.SqlDbType.VarChar).Value = folders;
            cmd.Parameters.Add("@target_id", System.Data.SqlDbType.Int).Value = Convert.ToUInt32(target_id);
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();
            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();
        }
    }
}