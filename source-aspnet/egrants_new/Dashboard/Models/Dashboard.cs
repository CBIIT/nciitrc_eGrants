#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  Dashboard.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-05-05
// Contributors:
//      - Briggs, Robin (NIH/NCI) [C] - briggsr2
//      -
// Copyright (c) National Institute of Health
// 
// <Description of the file>
// 
// This source is subject to the NIH Softwre License.
// See https://ncihub.org/resources/899/download/Guidelines_for_Releasing_Research_Software_04062015.pdf
// All other rights reserved.
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT ARE DISCLAIMED. IN NO EVENT SHALL THE NATIONAL
// CANCER INSTITUTE (THE PROVIDER), THE NATIONAL INSTITUTES OF HEALTH, THE
// U.S. GOVERNMENT OR THE INDIVIDUAL DEVELOPERS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
// \***************************************************************************/

#endregion

#region

using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

#endregion

namespace egrants_new.Dashboard.Models
{
    /// <summary>
    /// The dashboard.
    /// </summary>
    public class Dashboard
    {
        /// <summary>
        /// The get total widgets.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetTotalWidgets()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("SELECT max(widget_id) as total_widgets FROM dbo.DB_Widget_Master WHERE end_date is null", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var total_widgets = string.Empty;
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                total_widgets = rdr["total_widgets"]?.ToString();

            rdr.Close();
            conn.Close();

            return total_widgets;
        }

        /// <summary>
        /// The load widgets.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="idstr">
        /// The idstr.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<WidgetAssigments> LoadWidgets(string act, string idstr, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_dashboard", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@idstr", SqlDbType.VarChar).Value = idstr;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var Widgets = new List<WidgetAssigments>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Widgets.Add(
                    new WidgetAssigments
                        {
                            widget_id = rdr["widget_id"]?.ToString(),
                            widget_title = rdr["widget_title"]?.ToString(),
                            selected = rdr["selected"]?.ToString()
                        }
                );

            rdr.Close();
            conn.Close();

            return Widgets;
        }

        /// <summary>
        /// The load seleted widgets.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<SelectedWidgets> LoadSeletedWidgets(string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT ROW_NUMBER()OVER(ORDER BY widget.widget_id) AS order_id, widget.widget_id,widget_title,template_name "
              + " FROM dbo.DB_Widget_Master as widget, dbo.DB_WIDGET_ASSIGNMENT a "
              + " WHERE widget.widget_id = a.widget_id and widget.end_date is null and a.userid = @userid and a.end_date is null",
                conn
            );

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@userid", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var SeletedWidgets = new List<SelectedWidgets>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                SeletedWidgets.Add(
                    new SelectedWidgets
                        {
                            order_id = rdr["order_id"]?.ToString(),
                            widget_id = rdr["widget_id"]?.ToString(),
                            widget_title = rdr["widget_title"]?.ToString(),
                            template_name = rdr["template_name"]?.ToString()
                        }
                );

            rdr.Close();
            conn.Close();

            return SeletedWidgets;
        }

        /// <summary>
        /// The save_selected.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="idstr">
        /// The idstr.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        public static void save_selected(string act, string idstr, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_dashboard", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@idstr", SqlDbType.VarChar).Value = idstr;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            conn.Open();
            var rdr = cmd.ExecuteReader();
            rdr.Close();
            conn.Close();
        }

        /// <summary>
        /// The load grants togo cc.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<WidgetData> LoadGrantsTogoCC(string userid, string type)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("DB_LISTOF_GRANTS_TOGO_OFTYPE", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userid", SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@type", SqlDbType.VarChar).Value = type;
            conn.Open();

            var GrantsTogoCC = new List<WidgetData>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                GrantsTogoCC.Add(
                    new WidgetData
                        {
                            appl_id = rdr["appl_id"]?.ToString(), fgn = rdr["fgn"]?.ToString(), assigned_date = rdr["assigned_date"]?.ToString()
                        }
                );

            rdr.Close();
            conn.Close();

            return GrantsTogoCC;
        }

        /// <summary>
        /// The load grants togo nc.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<WidgetData> LoadGrantsTogoNC(string userid, string type)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("DB_LISTOF_GRANTS_TOGO_OFTYPE", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userid", SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@type", SqlDbType.VarChar).Value = type;
            conn.Open();

            var GrantsTogoCC = new List<WidgetData>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                GrantsTogoCC.Add(
                    new WidgetData
                        {
                            appl_id = rdr["appl_id"]?.ToString(), fgn = rdr["fgn"]?.ToString(), assigned_date = rdr["assigned_date"]?.ToString()
                        }
                );

            rdr.Close();
            conn.Close();

            return GrantsTogoCC;
        }

        /// <summary>
        /// The load grants expedited.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<WidgetData> LoadGrantsExpedited(string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("DB_GET_WIDGET_EXPEDITED_GRANTS", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userid", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var GrantsExpedited = new List<WidgetData>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                GrantsExpedited.Add(
                    new WidgetData { appl_id = rdr["appl_id"]?.ToString(), fgn = rdr["fgn"]?.ToString(), ncab_date = rdr["ncab_date"]?.ToString() }
                );

            rdr.Close();
            conn.Close();

            return GrantsExpedited;
        }

        /// <summary>
        /// The load grants delayed.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<WidgetData> LoadGrantsDelayed(string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("DB_GET_WIDGET_LATEGRANTS", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userid", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var GrantsLate = new List<WidgetData>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                GrantsLate.Add(
                    new WidgetData
                        {
                            appl_id = rdr["appl_id"]?.ToString(),
                            fgn = rdr["fgn"]?.ToString(),
                            status_code = rdr["status_code"]?.ToString(),
                            days_late = rdr["days_late"]?.ToString()
                        }
                );

            rdr.Close();
            conn.Close();

            return GrantsLate;
        }

        /// <summary>
        /// The load grants new.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<WidgetData> LoadGrantsNew(string userid, string type)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("DB_LISTOF_NEW_GRANTS_OFTYPE ", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userid", SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@type", SqlDbType.VarChar).Value = type;
            conn.Open();

            var GrantsNew = new List<WidgetData>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                GrantsNew.Add(
                    new WidgetData
                        {
                            appl_id = rdr["appl_id"]?.ToString(), fgn = rdr["fgn"]?.ToString(), assigned_date = rdr["assigned_date"]?.ToString()
                        }
                );

            rdr.Close();
            conn.Close();

            return GrantsNew;
        }

        /// <summary>
        /// The load link list.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<LinkLists> LoadLinkList()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT 1 as tag, category_name,category_id, null as link_title ,null as link_url,null as sort_order,null as icon_name "
              + " FROM dbo.DB_WIDGET_LINK WHERE end_date is null and Category_name <> '' " + " UNION "
              + " SELECT 2 as tag, category_name, category_id, Link_title, Link_url, sort_order,"
              + " CASE WHEN icon_name is null THEN '' WHEN icon_name is not null THEN icon_name END as icon_name"
              + " FROM dbo.DB_WIDGET_LINK WHERE end_date is null and Category_name <> '' ORDER BY Category_name, tag, sort_order",
                conn
            );

            cmd.CommandType = CommandType.Text;
            conn.Open();

            var LinkList = new List<LinkLists>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                LinkList.Add(
                    new LinkLists
                        {
                            tag = rdr["tag"]?.ToString(),
                            category_id = rdr["category_id"]?.ToString(),
                            category_name = rdr["category_name"]?.ToString(),
                            link_title = rdr["link_title"]?.ToString(),
                            link_url = rdr["link_url"]?.ToString(),
                            sort_order = rdr["sort_order"]?.ToString(),
                            icon_name = rdr["icon_name"]?.ToString()
                        }
                );

            rdr.Close();
            conn.Close();

            return LinkList;
        }

        /// <summary>
        /// The load avgtime.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<avgtime> LoadAvgtime(string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("dbo.DB_WIDGET_AVGTIME", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userid", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var AvgtimeList = new List<avgtime>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                AvgtimeList.Add(
                    new avgtime
                        {
                            ALLOWED_RELEASE_DAYS = rdr["ALLOWED_RELEASE_DAYS"]?.ToString(),
                            AVG_DAYSTAKEN = rdr["AVG_DAYSTAKEN"]?.ToString(),
                            GRANT_COUNT = rdr["GRANT_COUNT"]?.ToString()
                        }
                );

            rdr.Close();
            conn.Close();

            return AvgtimeList;
        }

        /// <summary>
        /// The load grants status.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<GrantStatus> LoadGrantsStatus()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT 1 as tag,action_type,null as status_code,null grants_count " + " FROM DB_GPMATS_ASSIGNMENT_STATUS GROUP BY action_type"
                                                                                     + " UNION "
                                                                                     + " SELECT 2 as tag,action_type,status_code,COUNT(*) AS grants_count "
                                                                                     + " FROM DB_GPMATS_ASSIGNMENT_STATUS GROUP BY action_type, status_code "
                                                                                     + " order by action_type, status_code",
                conn
            );

            cmd.CommandType = CommandType.Text;
            conn.Open();

            var GrantsStatusList = new List<GrantStatus>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                GrantsStatusList.Add(
                    new GrantStatus
                        {
                            tag = rdr["tag"]?.ToString(),
                            action_type = rdr["action_type"]?.ToString(),
                            status_code = rdr["status_code"]?.ToString(),
                            grants_count = rdr["grants_count"]?.ToString()
                        }
                );

            rdr.Close();
            conn.Close();

            return GrantsStatusList;
        }

        /// <summary>
        /// The load audit report.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<AuditReport> LoadAuditReport()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("DB_GET_EGRANTS_AUDIT_REPORT", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();

            var AuditReports = new List<AuditReport>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                AuditReports.Add(
                    new AuditReport
                        {
                            report_name = rdr["report_name"]?.ToString(),
                            report_url = rdr["report_url"]?.ToString(),
                            run_date = rdr["run_date"]?.ToString()
                        }
                );

            rdr.Close();
            conn.Close();

            return AuditReports;
        }

        /// <summary>
        /// The widget assigments.
        /// </summary>
        public class WidgetAssigments
        {
            /// <summary>
            /// Gets or sets the widget_id.
            /// </summary>
            public string widget_id { get; set; }

            /// <summary>
            /// Gets or sets the widget_title.
            /// </summary>
            public string widget_title { get; set; }

            /// <summary>
            /// Gets or sets the selected.
            /// </summary>
            public string selected { get; set; }
        }

        /// <summary>
        /// The selected widgets.
        /// </summary>
        public class SelectedWidgets
        {
            /// <summary>
            /// Gets or sets the order_id.
            /// </summary>
            public string order_id { get; set; }

            /// <summary>
            /// Gets or sets the widget_id.
            /// </summary>
            public string widget_id { get; set; }

            /// <summary>
            /// Gets or sets the widget_title.
            /// </summary>
            public string widget_title { get; set; }

            /// <summary>
            /// Gets or sets the template_name.
            /// </summary>
            public string template_name { get; set; }
        }

        /// <summary>
        /// The widget data.
        /// </summary>
        public class WidgetData
        {
            /// <summary>
            /// Gets or sets the appl_id.
            /// </summary>
            public string appl_id { get; set; }

            /// <summary>
            /// Gets or sets the fgn.
            /// </summary>
            public string fgn { get; set; }

            /// <summary>
            /// Gets or sets the userid.
            /// </summary>
            public string userid { get; set; }

            /// <summary>
            /// Gets or sets the assigned_date.
            /// </summary>
            public string assigned_date { get; set; }

            /// <summary>
            /// Gets or sets the ncab_date.
            /// </summary>
            public string ncab_date { get; set; }

            /// <summary>
            /// Gets or sets the status_code.
            /// </summary>
            public string status_code { get; set; }

            /// <summary>
            /// Gets or sets the days_late.
            /// </summary>
            public string days_late { get; set; }
        }

        /// <summary>
        /// The link lists.
        /// </summary>
        public class LinkLists
        {
            /// <summary>
            /// Gets or sets the tag.
            /// </summary>
            public string tag { get; set; }

            /// <summary>
            /// Gets or sets the category_id.
            /// </summary>
            public string category_id { get; set; }

            /// <summary>
            /// Gets or sets the category_name.
            /// </summary>
            public string category_name { get; set; }

            /// <summary>
            /// Gets or sets the link_title.
            /// </summary>
            public string link_title { get; set; }

            /// <summary>
            /// Gets or sets the link_url.
            /// </summary>
            public string link_url { get; set; }

            /// <summary>
            /// Gets or sets the sort_order.
            /// </summary>
            public string sort_order { get; set; }

            /// <summary>
            /// Gets or sets the icon_name.
            /// </summary>
            public string icon_name { get; set; }
        }

        /// <summary>
        /// The avgtime.
        /// </summary>
        public class avgtime
        {
            /// <summary>
            /// Gets or sets the userid.
            /// </summary>
            public string USERID { get; set; }

            /// <summary>
            /// Gets or sets the allowe d_ releas e_ days.
            /// </summary>
            public string ALLOWED_RELEASE_DAYS { get; set; }

            /// <summary>
            /// Gets or sets the av g_ daystaken.
            /// </summary>
            public string AVG_DAYSTAKEN { get; set; }

            /// <summary>
            /// Gets or sets the gran t_ count.
            /// </summary>
            public string GRANT_COUNT { get; set; }
        }

        /// <summary>
        /// The grant status.
        /// </summary>
        public class GrantStatus
        {
            /// <summary>
            /// Gets or sets the tag.
            /// </summary>
            public string tag { get; set; }

            /// <summary>
            /// Gets or sets the action_type.
            /// </summary>
            public string action_type { get; set; }

            /// <summary>
            /// Gets or sets the status_code.
            /// </summary>
            public string status_code { get; set; }

            /// <summary>
            /// Gets or sets the grants_count.
            /// </summary>
            public string grants_count { get; set; }
        }

        /// <summary>
        /// The audit report.
        /// </summary>
        public class AuditReport
        {
            /// <summary>
            /// Gets or sets the report_name.
            /// </summary>
            public string report_name { get; set; }

            /// <summary>
            /// Gets or sets the report_url.
            /// </summary>
            public string report_url { get; set; }

            /// <summary>
            /// Gets or sets the run_date.
            /// </summary>
            public string run_date { get; set; }
        }
    }
}