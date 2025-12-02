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

        // update user data
        public void run_db(
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

                var rdr = cmd.ExecuteReader();

                rdr.Close();
                conn.Close();
            }
               
        }

        // to prevent user data duplicate, before create new or update, check user data and get return notice
        /// <summary>
        /// The to_preview.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="index_id">
        /// The index_id.
        /// </param>
        /// <param name="active_id">
        /// The active_id.
        /// </param>
        /// <param name="user_id">
        /// The user_id.
        /// </param>
        /// <param name="login_id">
        /// The login_id.
        /// </param>
        /// <param name="last_name">
        /// The last_name.
        /// </param>
        /// <param name="first_name">
        /// The first_name.
        /// </param>
        /// <param name="middle_name">
        /// The middle_name.
        /// </param>
        /// <param name="email_address">
        /// The email_address.
        /// </param>
        /// <param name="phone_number">
        /// The phone_number.
        /// </param>
        /// <param name="coordinator_id">
        /// The coordinator_id.
        /// </param>
        /// <param name="position_id">
        /// The position_id.
        /// </param>
        /// <param name="ic_id">
        /// The ic_id.
        /// </param>
        /// <param name="egrants_tab">
        /// The egrants_tab.
        /// </param>
        /// <param name="mgt_tab">
        /// The mgt_tab.
        /// </param>
        /// <param name="admin_tab">
        /// The admin_tab.
        /// </param>
        /// <param name="docman_tab">
        /// The docman_tab.
        /// </param>
        /// <param name="cft_tab">
        /// The cft_tab.
        /// </param>
        /// <param name="dashboard_tab">
        /// The dashboard_tab.
        /// </param>
        /// <param name="iccoord_tab">
        /// The iccoord_tab.
        /// </param>
        /// <param name="is_coordinator">
        /// The is_coordinator.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string to_preview(
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
            var return_notice = string.Empty;
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
                cmd.Parameters.Add("@middle_name", SqlDbType.VarChar).Value = middle_name ?? (object)DBNull.Value;
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
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    return_notice = rdr[0].ToString();

                conn.Close();

                return return_notice;
            }
            
        }

        /// <summary>
        /// The get character index.
        /// </summary>
        /// <param name="first_letter">
        /// The first_letter.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int getCharacterIndex(string first_letter)
        {
            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
                var cmd = new SqlCommand("select index_id from character_index where character_index=@first_letter", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@first_letter", SqlDbType.VarChar).Value = first_letter;

                conn.Open();
                var CharacterIndex = 0;
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    CharacterIndex = Convert.ToInt16(rdr["index_id"]);

                conn.Close();

                return CharacterIndex;
            }
           
        }

    }
}

