using System.Data;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Services.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Serilog;

namespace eGrants.Services
{
    public class SupplementService : ISupplementService
    {
        private readonly ISessionInfoService _sessionInfoService;
        private readonly AppDbContext _context;

        public SupplementService(ISessionInfoService sessionInfoService, AppDbContext context)
        {
            _sessionInfoService = sessionInfoService;
            _context = context;
        }

        public List<Notifications> LoadNotifications(string act, string pa, string detail, int id, string ic, string userid)
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand("sp_web_admin_supplement_loaddata", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                    cmd.Parameters.Add("@pa", SqlDbType.VarChar).Value = pa;
                    cmd.Parameters.Add("@detail", SqlDbType.VarChar).Value = detail;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                    cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
                    conn.Open();

                    var notifications = new List<Notifications>();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        notifications.Add(
                            new Notifications
                            {
                                id = rdr["id"]?.ToString(),
                                full_grant_num = rdr["full_grant_num"]?.ToString(),
                                appl_id = rdr["appl_id"]?.ToString(),
                                pa = rdr["pa"]?.ToString(),
                                subjectLine = rdr["subjectLine"]?.ToString(),
                                NotificationBody = rdr["NotificationBody"]?.ToString(),
                                NotRcvd_dt = rdr["NotRcvd_dt"]?.ToString(),
                                created_date = rdr["created_date"]?.ToString()
                            });

                    rdr.Close();
                    conn.Close();

                    return notifications;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "LoadNotifications: Error occurred - Act: {Act}, PA: {PA}, Id: {Id}, UserId: {UserId}",
                    act, pa, id, userid);
                throw;
            }
        }

        public List<NotificationStatus> ReviewNotifications(string act, string pa, string detail, int id, string ic, string userid)
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand("sp_web_admin_supplement_loaddata", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                    cmd.Parameters.Add("@pa", SqlDbType.VarChar).Value = pa;
                    cmd.Parameters.Add("@detail", SqlDbType.VarChar).Value = detail;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                    cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
                    conn.Open();

                    var notificationStatus = new List<NotificationStatus>();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        notificationStatus.Add(
                            new NotificationStatus
                            {
                                tag = rdr["tag"]?.ToString(),
                                full_grant_num = rdr["full_grant_num"]?.ToString(),
                                pa = rdr["pa"]?.ToString(),
                                id = rdr["id"]?.ToString(),
                                document_id = rdr["document_id"]?.ToString(),
                                document_date = rdr["document_date"]?.ToString(),
                                category_name = rdr["category_name"]?.ToString(),
                                url = rdr["url"]?.ToString()
                            });

                    rdr.Close();
                    conn.Close();

                    return notificationStatus;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "ReviewNotifications: Error occurred - Act: {Act}, Id: {Id}, UserId: {UserId}",
                    act, id, userid);
                throw;
            }
        }

        public List<EmailStatus> ReviewEmailStatus(int id)
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand(
                        "select id,email as email_type, position, person_name,email_address,convert(varchar, email_date, 101) as email_date, "
                      + " convert(varchar, created_date, 101) as created_date, email_send_status, convert(varchar, reply_recieved_date, 101) as reply_recieved_date, reply_status "
                      + " from adsup_Notification_email_status where Notification_id = @id",
                        conn);

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    conn.Open();

                    var emailStatus = new List<EmailStatus>();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        emailStatus.Add(
                            new EmailStatus
                            {
                                id = rdr["id"]?.ToString(),
                                email_type = rdr["email_type"]?.ToString(),
                                email_date = rdr["email_date"]?.ToString(),
                                email_address = rdr["email_address"]?.ToString(),
                                email_send_status = rdr["email_send_status"]?.ToString(),
                                position = rdr["position"]?.ToString(),
                                person_name = rdr["person_name"]?.ToString(),
                                reply_status = rdr["reply_status"]?.ToString(),
                                reply_recieved_date = rdr["reply_recieved_date"]?.ToString(),
                                created_date = rdr["created_date"]?.ToString()
                            });

                    rdr.Close();
                    conn.Close();

                    return emailStatus;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ReviewEmailStatus: Error occurred - Id: {Id}", id);
                throw;
            }
        }

        public List<EmailPositions> LoadEmailPositionList()
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand(
                        "select distinct ltrim(rtrim(email_position_code)) as email_position_code from dbo.adsup_email_position_master order by email_position_code",
                        conn);

                    cmd.CommandType = CommandType.Text;
                    conn.Open();

                    var emailPositionList = new List<EmailPositions>();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        emailPositionList.Add(new EmailPositions { email_position_code = rdr["email_position_code"]?.ToString() });

                    rdr.Close();
                    conn.Close();

                    return emailPositionList;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LoadEmailPositionList: Error occurred while loading email positions");
                throw;
            }
        }

        public string GetNotice(string act, string pa, string detail, int id, string name, string subject, string ic, string userid)
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand("sp_web_admin_supplement_modify", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                    cmd.Parameters.Add("@pa", SqlDbType.VarChar).Value = pa;
                    cmd.Parameters.Add("@detail", SqlDbType.VarChar).Value = detail;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
                    cmd.Parameters.Add("@subject", SqlDbType.VarChar).Value = subject;
                    cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                    cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
                    cmd.Parameters.Add("@return_notice", SqlDbType.VarChar, 200);
                    cmd.Parameters["@return_notice"].Direction = ParameterDirection.Output;

                    conn.Open();
                    var dataReader = cmd.ExecuteReader();
                    dataReader.Close();
                    conn.Close();

                    var return_notice = Convert.ToString(cmd.Parameters["@return_notice"].Value);

                    return return_notice;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "GetNotice: Error occurred - Act: {Act}, Id: {Id}, UserId: {UserId}",
                    act, id, userid);
                throw;
            }
        }

        public List<EmailTemplates> LoadEmailTemplates()
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand(
                        "SELECT ltrim(rtrim(template_name)) as template_name,id,[subject] as subject, dbo.fn_clean_characters(body) as body,"
                      + "created_by_person_id,convert(varchar,created_date,101) as created_date FROM dbo.adsup_email_master order by template_name",
                        conn);

                    cmd.CommandType = CommandType.Text;
                    conn.Open();

                    var emailTemplate = new List<EmailTemplates>();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        emailTemplate.Add(
                            new EmailTemplates
                            {
                                id = rdr["id"]?.ToString(),
                                template_name = rdr["template_name"]?.ToString(),
                                subject = rdr["subject"]?.ToString(),
                                body = rdr["body"]?.ToString(),
                                created_date = rdr["created_date"]?.ToString(),
                                created_by_person_id = rdr["created_by_person_id"]?.ToString()
                            });

                    rdr.Close();
                    conn.Close();

                    return emailTemplate;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LoadEmailTemplates: Error occurred while loading email templates");
                throw;
            }
        }

        public List<EmailRules> LoadEmailRulesList()
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand(
                        "SELECT id, ltrim(rtrim(pa)) as pa FROM dbo.adsup_email_rules WHERE end_date is null order by ltrim(rtrim(pa))",
                        conn);

                    cmd.CommandType = CommandType.Text;
                    conn.Open();

                    var emailRules = new List<EmailRules>();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        emailRules.Add(new EmailRules { id = rdr["id"]?.ToString(), pa = rdr["pa"]?.ToString() });

                    rdr.Close();
                    conn.Close();

                    return emailRules;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LoadEmailRulesList: Error occurred while loading email rules list");
                throw;
            }
        }

        public List<EmailRule> LoadEmailRule(string act, string pa, string detail, int id, string ic, string userid)
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    var cmd = new SqlCommand("sp_web_admin_supplement_loaddata", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                    cmd.Parameters.Add("@pa", SqlDbType.VarChar).Value = pa;
                    cmd.Parameters.Add("@detail", SqlDbType.VarChar).Value = detail;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                    cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
                    conn.Open();

                    var emailRule = new List<EmailRule>();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        emailRule.Add(
                            new EmailRule
                            {
                                pa = rdr["pa"]?.ToString(),
                                email_to = rdr["email_to"]?.ToString(),
                                email_cc = rdr["email_cc"]?.ToString(),
                                email_template_id = rdr["email_template_id"]?.ToString(),
                                email_template_name = rdr["email_template_name"]?.ToString(),
                                email_subject = rdr["email_subject"]?.ToString(),
                                email_body = rdr["email_body"]?.ToString(),
                                person_name = rdr["person_name"]?.ToString(),
                                start_date = rdr["start_date"]?.ToString(),
                                end_date = rdr["end_date"]?.ToString()
                            });

                    rdr.Close();
                    conn.Close();

                    return emailRule;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "LoadEmailRule: Error occurred - Act: {Act}, PA: {PA}, UserId: {UserId}",
                    act, pa, userid);
                throw;
            }
        }
    }
}