using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Models
{
    public class QCAssignment
    {
        public class QCReasons
        {
            public string qc_reason { get; set; }
        }

        public static List<QCReasons> LoadQCReasons(string ic)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT distinct qc_reason FROM vw_quality_control WHERE profile = @ic ORDER BY qc_reason", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic; 
                conn.Open();

                var QCReasons = new List<QCReasons>();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    QCReasons.Add(new QCReasons { qc_reason = rdr["qc_reason"].ToString() });
                }
                return QCReasons; 
        }

        public class QCPersons
        {
            public string qc_reason { get; set; }
            public string userid { get; set; }
            public string person_id { get; set; }
            public string person_name { get; set; }
        }

        public static List<QCPersons> LoadQCPersons(string ic)
        {
                System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT qc_reason, userid, person_id, person_name FROM vw_quality_control WHERE profile=@ic order by qc_reason", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;  
                conn.Open();

                var QCPeasons = new List<QCPersons>();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    QCPeasons.Add(new QCPersons { qc_reason = rdr["qc_reason"].ToString(), userid = rdr["userid"].ToString(), person_id = rdr["person_id"].ToString(), person_name = rdr["person_name"].ToString() });
                }
                return QCPeasons;
        }

        public class QCReports
        {
            public string qc_days { get; set; }
            public string files_to_qc { get; set; }
            public string qc_person_id { get; set; }
            public string qc_person_name { get; set; }
        }

        public static List<QCReports> LoadQCReport(string ic)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(
                @"	with qc as (Select count(*) as files_to_qc, 
            AVG(datediff(d, qc_date, getdate())) as qc_days,
            qc_person_id from egrants where
            qc_date is not null
            and qc_person_id is not null
            and qc_reason is not null
            and disabled_date is null
            and ic = @ic
            AND parent_id IS null
            and not(grant_id is null) 
            group by qc_person_id)

            Select qc.files_to_qc, qc.qc_days,
            qc.qc_person_id, COALESCE(vp.person_name, CAST(qc.qc_person_id as varchar(10))) as qc_person_name
            from qc inner join vw_people vp on qc.qc_person_id = vp.person_id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;    
                conn.Open();

                var QCReport = new List<QCReports>();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    QCReport.Add(new QCReports { files_to_qc = rdr["files_to_qc"].ToString(), qc_person_id = rdr["qc_person_id"].ToString(), qc_person_name = rdr["qc_person_name"].ToString(), qc_days = rdr["qc_days"].ToString() });
                }
                return QCReport;    
        }

        public static void run_db(string act, int qc_person_id, string qc_reason, int percent, int person_id, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_management_qc_assign", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@person_id", System.Data.SqlDbType.Int).Value = person_id;
            cmd.Parameters.Add("@qc_person_id", System.Data.SqlDbType.Int).Value = qc_person_id;
            cmd.Parameters.Add("@qc_reason", System.Data.SqlDbType.VarChar).Value = qc_reason;
            cmd.Parameters.Add("@percent", System.Data.SqlDbType.Int).Value = percent;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();
            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();       
        }
    }
}