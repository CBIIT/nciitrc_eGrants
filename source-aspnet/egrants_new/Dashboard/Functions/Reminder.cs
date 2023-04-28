using egrants_new.Dashboard.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace egrants_new.Dashboard.Functions
{
    public static class Reminder
    {

        public static List<Appls> LoadAppls(int serial_num)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT appl.appl_id, appl.serial_num, appl.full_grant_num "+
            " FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS d, vw_appls as appl WHERE appl.appl_id = d.APPL_ID and appl.serial_num =@serial_num", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@serial_num", System.Data.SqlDbType.VarChar).Value = serial_num;
            conn.Open();

            var appls = new List<Appls>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                appls.Add(new Appls
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    serial_num = rdr["serial_num"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString()
                });
            }
            conn.Close();
            return appls;
        }

        public static List<Appls> LoadSelectedAppl(int appl_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(" SELECT appl_id, fgn as full_grant_num, convert(varchar, GRANT_ASSIGN_DATE, 101) as assign_date, CASE " +
            " WHEN APPL_TYPE_CODE=5 THEN convert(varchar, DATEADD(day, 45, convert(varchar(10), GRANT_ASSIGN_DATE, 101)), 101)"+
            " WHEN APPL_TYPE_CODE=1 THEN convert(varchar, DATEADD(day, 60, convert(varchar(10), GRANT_ASSIGN_DATE, 101)), 101)"+
            " WHEN APPL_TYPE_CODE=2 THEN convert(varchar, DATEADD(day, 60, convert(varchar(10), GRANT_ASSIGN_DATE, 101)), 101)"+
            " ELSE convert(varchar, DATEADD(day, DATEDIFF(day, GRANT_ASSIGN_DATE, GETDATE()), convert(varchar(10), GRANT_ASSIGN_DATE, 101)), 101) END AS due_date"+
            " FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS WHERE APPL_ID =@appl_id ", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@appl_id", System.Data.SqlDbType.VarChar).Value = appl_id;
            conn.Open();

            var appl = new List<Appls>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                appl.Add(new Appls
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString(),
                    assign_date = rdr["assign_date"]?.ToString(),
                    due_date = rdr["due_date"]?.ToString(),
                });
            }
            conn.Close();
            return appl;
        }

        public static void run_db(string event_type, int appl_id,string effective_date, string reminder_text, string by_email, string by_display, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("INSERT dbo.DB_REMINDER(event_type,user_id,appl_id,effective_date,Txt,to_be_emailed,to_be_displayed,created_date,created_by_person_id)"+
            " SELECT @event_type, @operator, @appl_id, @effective_date, @reminder_text, ISNULL(@by_email, null), ISNULL(@by_display, null), GETDATE(), person_id "+
            " FROM vw_people WHERE userid=@operator", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@event_type", System.Data.SqlDbType.VarChar).Value = event_type;
            cmd.Parameters.Add("@appl_id", System.Data.SqlDbType.Int).Value = appl_id;
            cmd.Parameters.Add("@effective_date", System.Data.SqlDbType.VarChar).Value = effective_date;
            cmd.Parameters.Add("@reminder_text", System.Data.SqlDbType.VarChar).Value = reminder_text;
            cmd.Parameters.Add("@by_email", System.Data.SqlDbType.VarChar).Value = by_email;
            cmd.Parameters.Add("@by_display", System.Data.SqlDbType.VarChar).Value = by_display;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            SqlDataReader rdr = cmd.ExecuteReader();

            conn.Close();
        }
    }
}