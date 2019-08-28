using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Dashboard.Models
{
    public class Dashboard
    {           
        public static string GetTotalWidgets()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT max(widget_id) as total_widgets FROM dbo.DB_Widget_Master WHERE end_date is null", conn);
                cmd.CommandType = CommandType.Text;
                conn.Open();

                string total_widgets = "";
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    total_widgets = rdr["total_widgets"]?.ToString();
                }
                rdr.Close();
                conn.Close();
                return total_widgets;
        }

        public class WidgetAssigments
        {
            public string widget_id { get; set; }
            public string widget_title { get; set; }
            public string selected { get; set; }
        }

        public static List<WidgetAssigments> LoadWidgets(string act, string idstr, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_dashboard", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@idstr", System.Data.SqlDbType.VarChar).Value = idstr;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var Widgets = new List<WidgetAssigments>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Widgets.Add(new WidgetAssigments
                {
                    widget_id = rdr["widget_id"]?.ToString(),
                    widget_title = rdr["widget_title"]?.ToString(),
                    selected = rdr["selected"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return Widgets;
        }

        public class SelectedWidgets
        {
            public string order_id { get; set; }
            public string widget_id { get; set; }
            public string widget_title { get; set; }
            public string template_name { get; set; }
        }

        public static List<SelectedWidgets> LoadSeletedWidgets(string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT ROW_NUMBER()OVER(ORDER BY widget.widget_id) AS order_id, widget.widget_id,widget_title,template_name "+
            " FROM dbo.DB_Widget_Master as widget, dbo.DB_WIDGET_ASSIGNMENT a "+
            " WHERE widget.widget_id = a.widget_id and widget.end_date is null and a.userid = @userid and a.end_date is null", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@userid", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var SeletedWidgets = new List<SelectedWidgets>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                SeletedWidgets.Add(new SelectedWidgets
                {
                    order_id= rdr["order_id"]?.ToString(),
                    widget_id = rdr["widget_id"]?.ToString(),
                    widget_title = rdr["widget_title"]?.ToString(),
                    template_name = rdr["template_name"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return SeletedWidgets;
        }
        public static void save_selected(string act, string idstr, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_dashboard", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;     
            cmd.Parameters.Add("@idstr", System.Data.SqlDbType.VarChar).Value = idstr;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            rdr.Close();
            conn.Close();
        }

        public class WidgetData
        {
            public string appl_id { get; set; }
            public string fgn { get; set; }
            public string userid { get; set; }
            public string assigned_date { get; set; }
            public string ncab_date { get; set; }
            public string status_code { get; set; }
            public string days_late { get; set; }
           
        }

        public static List<WidgetData> LoadGrantsTogoCC(string userid, string type)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DB_LISTOF_GRANTS_TOGO_OFTYPE", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userid", System.Data.SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@type", System.Data.SqlDbType.VarChar).Value = type;
            conn.Open();

            var GrantsTogoCC = new List<WidgetData>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                GrantsTogoCC.Add(new WidgetData
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    fgn = rdr["fgn"]?.ToString(),
                    assigned_date = rdr["assigned_date"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return GrantsTogoCC;
        }

        public static List<WidgetData> LoadGrantsTogoNC(string userid, string type)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DB_LISTOF_GRANTS_TOGO_OFTYPE", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userid", System.Data.SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@type", System.Data.SqlDbType.VarChar).Value = type;
            conn.Open();

            var GrantsTogoCC = new List<WidgetData>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                GrantsTogoCC.Add(new WidgetData
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    fgn = rdr["fgn"]?.ToString(),
                    assigned_date = rdr["assigned_date"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return GrantsTogoCC;
        }
        public static List<WidgetData> LoadGrantsExpedited(string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DB_GET_WIDGET_EXPEDITED_GRANTS", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userid", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var GrantsExpedited = new List<WidgetData>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                GrantsExpedited.Add(new WidgetData
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    fgn = rdr["fgn"]?.ToString(),
                    ncab_date = rdr["ncab_date"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return GrantsExpedited;
        }
        public static List<WidgetData> LoadGrantsDelayed(string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DB_GET_WIDGET_LATEGRANTS", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userid", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var GrantsLate = new List<WidgetData>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                GrantsLate.Add(new WidgetData
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    fgn = rdr["fgn"]?.ToString(),
                    status_code = rdr["status_code"]?.ToString(),
                    days_late = rdr["days_late"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return GrantsLate;
        }

        public static List<WidgetData> LoadGrantsNew(string userid, string type)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DB_LISTOF_NEW_GRANTS_OFTYPE ", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userid", System.Data.SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@type", System.Data.SqlDbType.VarChar).Value = type;
            conn.Open();

            var GrantsNew = new List<WidgetData>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                GrantsNew.Add(new WidgetData
                {
                    appl_id = rdr["appl_id"]?.ToString(),
                    fgn = rdr["fgn"]?.ToString(),
                    assigned_date = rdr["assigned_date"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return GrantsNew;
        }

        public class LinkLists
        {
            public string tag { get; set; }
            public string category_id { get; set; }
            public string category_name { get; set; }
            public string link_title { get; set; }
            public string link_url { get; set; }
            public string sort_order { get; set; }
            public string icon_name { get; set; }
        }

        public static List<LinkLists> LoadLinkList()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT 1 as tag, category_name,category_id, null as link_title ,null as link_url,null as sort_order,null as icon_name "+
            " FROM dbo.DB_WIDGET_LINK WHERE end_date is null and Category_name <> '' "+
            " UNION "+
            " SELECT 2 as tag, category_name, category_id, Link_title, Link_url, sort_order,"+
            " CASE WHEN icon_name is null THEN '' WHEN icon_name is not null THEN icon_name END as icon_name"+
            " FROM dbo.DB_WIDGET_LINK WHERE end_date is null and Category_name <> '' ORDER BY Category_name, tag, sort_order", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var LinkList = new List<LinkLists>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                LinkList.Add(new LinkLists
                {
                    tag = rdr["tag"]?.ToString(),
                    category_id = rdr["category_id"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    link_title = rdr["link_title"]?.ToString(),
                    link_url = rdr["link_url"]?.ToString(),
                    sort_order = rdr["sort_order"]?.ToString(),
                    icon_name = rdr["icon_name"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return LinkList;
        }

        public class avgtime
        {
            public string USERID { get; set; }
            public string ALLOWED_RELEASE_DAYS { get; set; }
            public string AVG_DAYSTAKEN { get; set; }
            public string GRANT_COUNT { get; set; }
        }

        public static List<avgtime> LoadAvgtime( string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("dbo.DB_WIDGET_AVGTIME", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userid", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var AvgtimeList = new List<avgtime>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                AvgtimeList.Add(new avgtime
                {
                    ALLOWED_RELEASE_DAYS = rdr["ALLOWED_RELEASE_DAYS"]?.ToString(),
                    AVG_DAYSTAKEN = rdr["AVG_DAYSTAKEN"]?.ToString(),
                    GRANT_COUNT = rdr["GRANT_COUNT"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return AvgtimeList;
        }

        public class GrantStatus
        {
            public string tag { get; set; }
            public string action_type { get; set; }
            public string status_code { get; set; }
            public string grants_count { get; set; }
        }

        public static List<GrantStatus> LoadGrantsStatus()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT 1 as tag,action_type,null as status_code,null grants_count "+
            " FROM DB_GPMATS_ASSIGNMENT_STATUS GROUP BY action_type" +
            " UNION " +
            " SELECT 2 as tag,action_type,status_code,COUNT(*) AS grants_count "+
            " FROM DB_GPMATS_ASSIGNMENT_STATUS GROUP BY action_type, status_code "+
            " order by action_type, status_code", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var GrantsStatusList = new List<GrantStatus>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                GrantsStatusList.Add(new GrantStatus
                {
                    tag = rdr["tag"]?.ToString(),
                    action_type = rdr["action_type"]?.ToString(),
                    status_code = rdr["status_code"]?.ToString(),
                    grants_count = rdr["grants_count"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return GrantsStatusList;
        }

        public class AuditReport
        {
            public string report_name { get; set; }
            public string report_url { get; set; }
            public string run_date { get; set; }
        }

        public static List<AuditReport> LoadAuditReport()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DB_GET_EGRANTS_AUDIT_REPORT", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();

            var AuditReports = new List<AuditReport>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                AuditReports.Add(new AuditReport
                {
                    report_name = rdr["report_name"]?.ToString(),
                    report_url = rdr["report_url"]?.ToString(),
                    run_date = rdr["run_date"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();
            return AuditReports;
        }
    }
}