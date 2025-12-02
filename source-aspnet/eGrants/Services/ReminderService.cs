using System.Data;

using eGrants.DAL;

using Microsoft.EntityFrameworkCore;

using Serilog;

using Microsoft.Data.SqlClient;
using eGrants.Models;
using eGrants.Services.Interfaces;


namespace eGrants.Services
{
    public class ReminderService : IReminderService
    {
        private readonly AppDbContext _context;

        public ReminderService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Appls>> LoadAppls(int serial_num)
        {
            var appls = new List<Appls>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand(@"
                SELECT appl.appl_id, appl.serial_num, appl.full_grant_num
                FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS d
                JOIN vw_appls appl ON appl.appl_id = d.APPL_ID
                WHERE appl.serial_num = @serial_num", conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@serial_num", SqlDbType.VarChar).Value = serial_num;

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    appls.Add(new Appls
                    {
                        appl_id = rdr["appl_id"]?.ToString(),
                        serial_num = rdr["serial_num"]?.ToString(),
                        full_grant_num = rdr["full_grant_num"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading applications for serial_num {SerialNum}", serial_num);
            }

            return appls;
        }

        public async Task<List<Appls>> LoadSelectedAppl(int appl_id)
        {
            var appls = new List<Appls>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand(@"
                SELECT appl_id, fgn as full_grant_num,
                       CONVERT(varchar, GRANT_ASSIGN_DATE, 101) as assign_date,
                       CASE 
                           WHEN APPL_TYPE_CODE = 5 THEN CONVERT(varchar, DATEADD(day, 45, GRANT_ASSIGN_DATE), 101)
                           WHEN APPL_TYPE_CODE IN (1,2) THEN CONVERT(varchar, DATEADD(day, 60, GRANT_ASSIGN_DATE), 101)
                           ELSE CONVERT(varchar, DATEADD(day, DATEDIFF(day, GRANT_ASSIGN_DATE, GETDATE()), GRANT_ASSIGN_DATE), 101)
                       END AS due_date
                FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS
                WHERE APPL_ID = @appl_id", conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@appl_id", SqlDbType.VarChar).Value = appl_id;

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    appls.Add(new Appls
                    {
                        appl_id = rdr["appl_id"]?.ToString(),
                        full_grant_num = rdr["full_grant_num"]?.ToString(),
                        assign_date = rdr["assign_date"]?.ToString(),
                        due_date = rdr["due_date"]?.ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading selected application for appl_id {ApplId}", appl_id);
            }

            return appls;
        }

        public async Task run_db(string event_type, int appl_id, string effective_date, string reminder_text, string by_email, string by_display, string userid)
        {
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await conn.OpenAsync();

                await using var cmd = new SqlCommand(@"
                INSERT dbo.DB_REMINDER(event_type, user_id, appl_id, effective_date, Txt, to_be_emailed, to_be_displayed, created_date, created_by_person_id)
                SELECT @event_type, @operator, @appl_id, @effective_date, @reminder_text,
                       ISNULL(@by_email, null), ISNULL(@by_display, null), GETDATE(), person_id
                FROM vw_people WHERE userid = @operator", conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@event_type", SqlDbType.VarChar).Value = event_type;
                cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = appl_id;
                cmd.Parameters.Add("@effective_date", SqlDbType.VarChar).Value = effective_date;
                cmd.Parameters.Add("@reminder_text", SqlDbType.VarChar).Value = reminder_text;
                cmd.Parameters.Add("@by_email", SqlDbType.VarChar).Value = by_email;
                cmd.Parameters.Add("@by_display", SqlDbType.VarChar).Value = by_display;
                cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inserting reminder for appl_id {ApplId}, userid {UserId}", appl_id, userid);
            }
        }
    }
}