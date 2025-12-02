using System.Data;
using System.Runtime.CompilerServices;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Services.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Serilog;

namespace eGrants.Services {
    /// <summary>
    /// The dashboard.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        // Dependency injection of a database context to access data
        private readonly AppDbContext _context;

        // Constructor that initializes the repository via dependency injection
        public DashboardService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        /// <summary>
        /// The get total widgets.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public async Task<string> GetTotalWidgets()
        {
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand(
                    "SELECT MAX(widget_id) AS total_widgets FROM dbo.DB_Widget_Master WHERE end_date IS NULL", conn);

                await using var rdr = await cmd.ExecuteReaderAsync();

                return await rdr.ReadAsync() ? rdr["total_widgets"]?.ToString() ?? string.Empty : string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred in GetTotalWidgets");
                return string.Empty;
            }
        }

        public async Task<List<WidgetAssigments>> LoadWidgets(string act, string idstr, string ic, string userid)
        {
            var results = new List<WidgetAssigments>();
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("sp_web_egrants_dashboard", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@act", act);
                cmd.Parameters.AddWithValue("@idstr", idstr);
                cmd.Parameters.AddWithValue("@ic", ic);
                cmd.Parameters.AddWithValue("@operator", userid);

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    results.Add(new WidgetAssigments
                    {
                        widget_id = rdr["widget_id"]?.ToString(),
                        widget_title = rdr["widget_title"]?.ToString(),
                        selected = rdr["selected"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred in LoadWidgets");
            }

            return results;
        }

        public async Task<List<SelectedWidgets>> LoadSeletedWidgets(string userid)
        {
            var results = new List<SelectedWidgets>();
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand(
                    @"SELECT ROW_NUMBER() OVER (ORDER BY widget.widget_id) AS order_id, 
                         widget.widget_id, widget_title, template_name
                  FROM dbo.DB_Widget_Master AS widget
                  INNER JOIN dbo.DB_WIDGET_ASSIGNMENT a 
                  ON widget.widget_id = a.widget_id
                  WHERE widget.end_date IS NULL 
                  AND a.userid = @userid 
                  AND a.end_date IS NULL", conn);

                cmd.Parameters.AddWithValue("@userid", userid);

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    results.Add(new SelectedWidgets
                    {
                        order_id = rdr["order_id"]?.ToString(),
                        widget_id = rdr["widget_id"]?.ToString(),
                        widget_title = rdr["widget_title"]?.ToString(),
                        template_name = rdr["template_name"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred in LoadSeletedWidgets");
            }

            return results;
        }

        public async Task save_selected(string act, string idstr, string ic, string userid)
        {
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("sp_web_egrants_dashboard", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@act", act);
                cmd.Parameters.AddWithValue("@idstr", idstr);
                cmd.Parameters.AddWithValue("@ic", ic);
                cmd.Parameters.AddWithValue("@operator", userid);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred in SaveSelected");
            }
        }

        public async Task<List<WidgetData>> LoadGrantsTogoCC(string userid, string type)
        {
            var results = new List<WidgetData>();
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("DB_LISTOF_GRANTS_TOGO_OFTYPE", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@type", type);

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    results.Add(new WidgetData
                    {
                        appl_id = rdr["appl_id"]?.ToString(),
                        fgn = rdr["fgn"]?.ToString(),
                        assigned_date = rdr["assigned_date"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred in LoadGrantsTogoCC");
            }

            return results;
        }

        public async Task<List<WidgetData>> LoadGrantsTogoNC(string userid, string type)
        {
            var results = new List<WidgetData>();
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("DB_LISTOF_GRANTS_TOGO_OFTYPE", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@type", type);

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    results.Add(new WidgetData
                    {
                        appl_id = rdr["appl_id"]?.ToString(),
                        fgn = rdr["fgn"]?.ToString(),
                        assigned_date = rdr["assigned_date"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred in LoadGrantsTogoNC for userid={UserId}, type={Type}", userid, type);
            }

            return results;
        }

        public async Task<List<WidgetData>> LoadGrantsExpedited(string userid)
        {
            var results = new List<WidgetData>();
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("DB_GET_WIDGET_EXPEDITED_GRANTS", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@userid", userid);

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    results.Add(new WidgetData
                    {
                        appl_id = rdr["appl_id"]?.ToString(),
                        fgn = rdr["fgn"]?.ToString(),
                        ncab_date = rdr["ncab_date"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred in LoadGrantsExpedited for userid={UserId}", userid);
            }

            return results;
        }

        public async Task<List<WidgetData>> LoadGrantsDelayed(string userid)
        {
            var results = new List<WidgetData>();
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("DB_GET_WIDGET_LATEGRANTS", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@userid", userid);

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    results.Add(new WidgetData
                    {
                        appl_id = rdr["appl_id"]?.ToString(),
                        fgn = rdr["fgn"]?.ToString(),
                        status_code = rdr["status_code"]?.ToString(),
                        days_late = rdr["days_late"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred in LoadGrantsDelayed for userid={UserId}", userid);
            }

            return results;
        }

        public async Task<List<WidgetData>> LoadGrantsNew(string userid, string type)
        {
            var results = new List<WidgetData>();
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("DB_LISTOF_NEW_GRANTS_OFTYPE", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@type", type);

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    results.Add(new WidgetData
                    {
                        appl_id = rdr["appl_id"]?.ToString(),
                        fgn = rdr["fgn"]?.ToString(),
                        assigned_date = rdr["assigned_date"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred in LoadGrantsNew for userid={UserId}, type={Type}", userid, type);
            }

            return results;
        }

        public async Task<List<LinkLists>> LoadLinkList()
        {
            var results = new List<LinkLists>();
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand(
                    @"SELECT 1 as tag, category_name, category_id, null as link_title, null as link_url, null as sort_order, null as icon_name
                  FROM dbo.DB_WIDGET_LINK WHERE end_date IS NULL AND Category_name <> ''
                  UNION
                  SELECT 2 as tag, category_name, category_id, Link_title, Link_url, sort_order,
                         CASE WHEN icon_name IS NULL THEN '' ELSE icon_name END as icon_name
                  FROM dbo.DB_WIDGET_LINK WHERE end_date IS NULL AND Category_name <> ''
                  ORDER BY Category_name, tag, sort_order", conn);

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    results.Add(new LinkLists
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
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred in LoadLinkList");
            }

            return results;
        }

        public async Task<List<avgtime>> LoadAvgtime(string userid)
        {
            var results = new List<avgtime>();
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("dbo.DB_WIDGET_AVGTIME", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@userid", userid);

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    results.Add(new avgtime
                    {
                        ALLOWED_RELEASE_DAYS = rdr["ALLOWED_RELEASE_DAYS"]?.ToString(),
                        AVG_DAYSTAKEN = rdr["AVG_DAYSTAKEN"]?.ToString(),
                        GRANT_COUNT = rdr["GRANT_COUNT"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred in LoadAvgtime for userid={UserId}", userid);
            }

            return results;
        }

        public async Task<List<GrantStatus>> LoadGrantsStatus()
        {
            var results = new List<GrantStatus>();
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand(
                    @"SELECT 1 as tag, action_type, null as status_code, null as grants_count 
                  FROM DB_GPMATS_ASSIGNMENT_STATUS GROUP BY action_type
                  UNION
                  SELECT 2 as tag, action_type, status_code, COUNT(*) AS grants_count
                  FROM DB_GPMATS_ASSIGNMENT_STATUS GROUP BY action_type, status_code
                  ORDER BY action_type, status_code", conn);

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    results.Add(new GrantStatus
                    {
                        tag = rdr["tag"]?.ToString(),
                        action_type = rdr["action_type"]?.ToString(),
                        status_code = rdr["status_code"]?.ToString(),
                        grants_count = rdr["grants_count"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred in LoadGrantsStatus");
            }

            return results;
        }

        public async Task<List<AuditReport>> LoadAuditReport()
        {
            var results = new List<AuditReport>();
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("DB_GET_EGRANTS_AUDIT_REPORT", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    results.Add(new AuditReport
                    {
                        report_name = rdr["report_name"]?.ToString(),
                        report_url = rdr["report_url"]?.ToString(),
                        run_date = rdr["run_date"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred in LoadAuditReport");
            }

            return results;
        }
    }
}