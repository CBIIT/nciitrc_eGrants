using System.Data;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eGrants.Services
{
    public class EgrantsAccessService : IEgrantsAccessService
    {
        // Dependency injection of a product repository to access data
        private readonly ISessionInfoService _sessionInfoService;
        private readonly AppDbContext _context;

        // Constructor that initializes the repository via dependency injection
        public EgrantsAccessService(ISessionInfoService sessionInfoService, AppDbContext context)
        {
            _sessionInfoService = sessionInfoService;
            _context = context;
        }

        public List<EgrantsUsers> LoadUsers(
            string act,
            int index_id,
            int active_id,
            int user_id,
            string login_id,
            string last_name,
            string first_name,
            string middle_name,
            string email_address,
            string phone_number,
            int coordinator_id,
            int position_id,
            int ic_id,
            int egrants_tab,
            int mgt_tab,
            int admin_tab,
            int docman_tab,
            int cft_tab,
            int dashboard_tab,
            int iccoord_tab,
            int is_coordinator,
            string end_date,
            string ic,
            string userid)
        {
            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
                var cmd = new SqlCommand("sp_web_egrants_access_control", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                cmd.Parameters.Add("@index_id", SqlDbType.Int).Value = index_id;
                cmd.Parameters.Add("@active_id", SqlDbType.Int).Value = active_id;
                cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = user_id;
                cmd.Parameters.Add("@login_id", SqlDbType.VarChar).Value = login_id;
                cmd.Parameters.Add("@first_name", SqlDbType.VarChar).Value = first_name;
                cmd.Parameters.Add("@middle_name", SqlDbType.VarChar).Value = middle_name;
                cmd.Parameters.Add("@last_name", SqlDbType.VarChar).Value = last_name;
                cmd.Parameters.Add("@email_address", SqlDbType.VarChar).Value = email_address;
                cmd.Parameters.Add("@phone_number", SqlDbType.VarChar).Value = phone_number;
                cmd.Parameters.Add("@coordinator_id", SqlDbType.Int).Value = coordinator_id;
                cmd.Parameters.Add("@position_id", SqlDbType.Int).Value = position_id;
                cmd.Parameters.Add("@ic_id", SqlDbType.Int).Value = ic_id;
                cmd.Parameters.Add("@egrants_tab", SqlDbType.Int).Value = egrants_tab;
                cmd.Parameters.Add("@mgt_tab", SqlDbType.Int).Value = mgt_tab;
                cmd.Parameters.Add("@admin_tab", SqlDbType.Int).Value = admin_tab;
                cmd.Parameters.Add("@docman_tab", SqlDbType.Int).Value = docman_tab;
                cmd.Parameters.Add("@cft_tab", SqlDbType.Int).Value = cft_tab;
                cmd.Parameters.Add("@dashboard_tab", SqlDbType.Int).Value = dashboard_tab;
                cmd.Parameters.Add("@iccoord_tab", SqlDbType.Int).Value = iccoord_tab;
                cmd.Parameters.Add("@is_coordinator", SqlDbType.Int).Value = is_coordinator;
                cmd.Parameters.Add("@end_date", SqlDbType.VarChar).Value = end_date;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

                conn.Open();

                var Users = new List<EgrantsUsers>();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    Users.Add(
                        new EgrantsUsers
                        {
                            PersonId = rdr["person_id"]?.ToString(),
                            userid = rdr["userid"]?.ToString(),
                            person_name = rdr["person_name"]?.ToString(),
                            last_name = rdr["last_name"]?.ToString(),
                            first_name = rdr["first_name"]?.ToString(),
                            middle_name = rdr["middle_name"]?.ToString(),
                            email_address = rdr["email"]?.ToString(),
                            phone_number = rdr["phone_number"]?.ToString(),
                            position_id = rdr["position_id"]?.ToString(),
                            position_name = rdr["position_name"]?.ToString(),
                            application_type = rdr["application_type"]?.ToString(),
                            active = rdr["active"]?.ToString(),
                            ic = rdr["ic"]?.ToString(),
                            can_admin = rdr["can_admin"]?.ToString(),
                            can_egrants = rdr["can_egrants"]?.ToString(),
                            can_dashboard = rdr["can_dashboard"]?.ToString(),
                            can_mgt = rdr["can_mgt"]?.ToString(),
                            can_docman = rdr["can_docman"]?.ToString(),
                            can_cft = rdr["can_cft"]?.ToString(),
                            can_iccoord = rdr["can_iccoord"]?.ToString(),
                            is_coordinator = rdr["is_coordinator"]?.ToString(),
                            coordinator_id = rdr["coordinator_id"]?.ToString(),
                            start_date = rdr["start_date"]?.ToString(),
                            end_date = rdr["end_date"]?.ToString()
                        });

                rdr.Close();
                conn.Close();

                return Users;
            }
        }

        public int ToCheckUserid(string userid)
        {
            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
                var cmd = new SqlCommand(
                    "select count(*) from people where application_type='egrants' and userid = @userid",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@userid", SqlDbType.VarChar).Value = userid;

                conn.Open();
                var count_userid = 0;
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    count_userid = Convert.ToInt16(rdr[0]);


                return count_userid;
            }
        }

    }
}

