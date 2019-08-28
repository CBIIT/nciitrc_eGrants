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
    public class CFTEdit
    {
        public class CFTContractFolderColors
        {
            public string color { get; set; }
            public string color_id { get; set; }
            public string color_name { get; set; }
        }

        public static List<CFTContractFolderColors> LoadFolderColors()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT color_id, color_name, color from contract_colors order by color_name", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var FolderColors = new List<CFTContractFolderColors>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                FolderColors.Add(new CFTContractFolderColors { color = rdr["color"]?.ToString(), color_id = rdr["color_id"]?.ToString(), color_name = rdr["color_name"]?.ToString() });
            }
            conn.Close();

            return FolderColors;
        }

        public class CFTContract
        {
            public string contract_id { get; set; }
            public string contract_number { get; set; }
            public string contract_type { get; set; }
            public string activity_code { get; set; }
            public string full_contract_number { get; set; }
            public string piid { get; set; }
            public string fiscal_year { get; set; }
            public string institution { get; set; }
            public string rfp { get; set; }
            public string specialist_name { get; set; }
            public string close_out { get; set; }
        }

        //load contracts by serial number
        public static List<CFTContract> LoadCFTContracts(int serial_num)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT contract_id, contract_number,fiscal_year,dbo.fn_clean_characters(institution) AS institution, " +
                " contract_type, activity_code, full_contract_number, piid, rfp, specialist_name, upper(close_out) AS close_out " +
                " FROM vw_contracts WHERE contract_number = @serial_num or control_number = @serial_num order by full_contract_number", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@serial_num", System.Data.SqlDbType.Int).Value = serial_num;

            var CFTContracts = new List<CFTContract>();
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                CFTContracts.Add(new CFTContract
                {
                    contract_id = rdr["contract_id"]?.ToString(),
                    contract_number = rdr["contract_number"]?.ToString(),
                    contract_type = rdr["contract_type"]?.ToString(),
                    activity_code = rdr["activity_code"]?.ToString(),
                    full_contract_number = rdr["full_contract_number"]?.ToString(),
                    institution = rdr["institution"]?.ToString(),
                    rfp = rdr["rfp"]?.ToString(),
                    piid = rdr["piid"]?.ToString(),
                    specialist_name = rdr["specialist_name"]?.ToString(),
                    fiscal_year = rdr["fiscal_year"]?.ToString(),
                    close_out = rdr["close_out"]?.ToString()
                });
            }
            conn.Close();
            return CFTContracts;
        }

        public static List<CFTContract> LoadCFTContract(int contract_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT contract_id, contract_number, fiscal_year,dbo.fn_clean_characters(institution) AS institution, " +
                " contract_type, activity_code, full_contract_number, piid, rfp, specialist_name, upper(close_out) AS close_out " +
                " FROM vw_contracts WHERE contract_id = @contract_id", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@contract_id", System.Data.SqlDbType.Int).Value = contract_id;

            var CFTContract = new List<CFTContract>();
            conn.Open();

            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                CFTContract.Add(new CFTContract
                {
                    contract_id = rdr["contract_id"]?.ToString(),
                    contract_number = rdr["contract_number"]?.ToString(),
                    contract_type = rdr["contract_type"]?.ToString(),
                    activity_code = rdr["activity_code"]?.ToString(),
                    full_contract_number = rdr["full_contract_number"]?.ToString(),
                    institution = rdr["institution"]?.ToString(),
                    rfp = rdr["rfp"]?.ToString(),
                    piid = rdr["piid"]?.ToString(),
                    specialist_name = rdr["specialist_name"]?.ToString(),
                    fiscal_year = rdr["fiscal_year"]?.ToString(),
                    close_out = rdr["close_out"]?.ToString()
                });
            }

            conn.Close();
            return CFTContract;
        }

        public static string ContractEdit(string act, int contract_id, string combined_piid, int bar_code, int color_id, string accession_number, int accession_id, string destroy_date, int box_start_number, int box_end_number, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("dbo.sp_web_contract_edit", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@contract_id", System.Data.SqlDbType.Int).Value = contract_id;
            cmd.Parameters.Add("@combined_piid", System.Data.SqlDbType.VarChar, 100).Value = combined_piid;
            cmd.Parameters.Add("@bar_code", System.Data.SqlDbType.Int).Value = bar_code;
            cmd.Parameters.Add("@color_id", System.Data.SqlDbType.Int).Value = color_id;
            cmd.Parameters.Add("@accession_number", System.Data.SqlDbType.VarChar).Value = accession_number;
            cmd.Parameters.Add("@accession_id", System.Data.SqlDbType.Int).Value = accession_id;
            cmd.Parameters.Add("@destroy_date", System.Data.SqlDbType.VarChar).Value = destroy_date;
            cmd.Parameters.Add("@box_start_number", System.Data.SqlDbType.Int).Value = box_start_number;
            cmd.Parameters.Add("@box_end_number", System.Data.SqlDbType.Int).Value = box_end_number;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@return_notice", System.Data.SqlDbType.VarChar, 200);
            cmd.Parameters["@return_notice"].Direction = System.Data.ParameterDirection.Output;

            conn.Open();
            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();

            string Return_Message = Convert.ToString(cmd.Parameters["@return_notice"].Value);
            return Return_Message;
        }
    }
}