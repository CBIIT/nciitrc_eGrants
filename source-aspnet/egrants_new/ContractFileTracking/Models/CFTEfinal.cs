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
    public class CFTEfinal
    {
        public class CFTContractEfinal
        {
            public string contract_id { get; set; }
            public string full_contract_number { get; set; }
            public string org_name { get; set; }
            public string total_amount { get; set; }
            public string final_payment { get; set; }
            public string deobligation { get; set; }
            public string refund { get; set; }
            public string expired_date { get; set; }
            public string signed { get; set; }
            public string signed_by { get; set; }
            public string signed_date { get; set; }
        }

        public static List<CFTContractEfinal> LoadContractEfinal(string contract_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT c.contract_id as contract_id, full_contract_number, institution,total_amount, final_payment, deobligation, refund, " +
            " convert(varchar, expired_date, 101) as expired_date, lower(signed_by) as signed,'sign_'+ lower(signed_by) + '.jpg' as signed_by, convert(varchar, signed_date, 101) as signed_date " +
            " FROM vw_contract_final_costs f right outer join vw_contracts c ON c.contract_id = f.contract_id WHERE c.contract_id = @contract_id", conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@contract_id", System.Data.SqlDbType.Int).Value = Convert.ToInt32(contract_id);

            var ContractEfinal = new List<CFTContractEfinal>();
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                ContractEfinal.Add(new CFTContractEfinal
                {
                    contract_id = rdr["contract_id"]?.ToString(),
                    full_contract_number = rdr["full_contract_number"]?.ToString(),
                    org_name = rdr["institution"]?.ToString(),
                    total_amount = rdr["total_amount"]?.ToString(),
                    final_payment = rdr["final_payment"]?.ToString(),
                    deobligation = rdr["deobligation"]?.ToString(),
                    refund = rdr["refund"]?.ToString(),
                    expired_date = rdr["expired_date"]?.ToString(),
                    signed = rdr["signed"]?.ToString(),
                    signed_by = rdr["signed_by"]?.ToString(),
                    signed_date = rdr["signed_date"]?.ToString(),
                });
            }
            conn.Close();
            return ContractEfinal;
        }

        public class Signed
        {
            public string person_name { get; set; }
            public string signed_name { get; set; }
            public string disabled { get; set; }
        }

        public static List<Signed> LoadEfinalSigned()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select person_name, signed_name, CASE when end_date is null Then 1 Else 0 End as disabled from contract_final_costs_sign order by person_name", conn);
            cmd.CommandType = CommandType.Text;

            var EfinalSigned = new List<Signed>();
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                EfinalSigned.Add(new Signed
                {
                    person_name = rdr["person_name"]?.ToString(),
                    signed_name = rdr["signed_name"]?.ToString(),
                    disabled = rdr["disabled"]?.ToString()
                });
            }
            conn.Close();
            return EfinalSigned;
        }

        public static string GetSignedBy(string contract_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select signed_by from contract_final_costs where contract_id = @contract_id", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@contract_id", System.Data.SqlDbType.Int).Value = Convert.ToInt32(contract_id);
            conn.Open();
            string signed_by = "";
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                signed_by = rdr["signed_by"]?.ToString();
            }
            conn.Close();
            return signed_by;
        }

        public static void ContractEfinal(string act, string contract_id, string total_amount, string final_payment, string deobligation, string refund, string expired_date, string signed_by)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("dbo.sp_web_contract_efinal", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@contract_id", System.Data.SqlDbType.Int).Value = Convert.ToInt32(contract_id);
            cmd.Parameters.Add("@total_amount", System.Data.SqlDbType.VarChar).Value = total_amount;
            cmd.Parameters.Add("@final_payment", System.Data.SqlDbType.VarChar).Value = final_payment;
            cmd.Parameters.Add("@deobligation", System.Data.SqlDbType.VarChar).Value = deobligation;
            cmd.Parameters.Add("@refund", System.Data.SqlDbType.VarChar).Value = refund;
            cmd.Parameters.Add("@expired_date", System.Data.SqlDbType.VarChar).Value = expired_date;
            cmd.Parameters.Add("@signed_by", System.Data.SqlDbType.VarChar).Value = signed_by;
            conn.Open();

            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();
            DataReader.Close();
            conn.Close();          
        }
    }
}