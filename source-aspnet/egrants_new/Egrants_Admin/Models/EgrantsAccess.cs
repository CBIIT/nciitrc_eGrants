#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EgrantsAccess.cs
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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using egrants_new.Models;

#endregion

namespace egrants_new.Egrants_Admin.Models
{
    /// <summary>
    /// The egrants access.
    /// </summary>
    public class EgrantsAccess
    {
        // return user list
        /// <summary>
        /// The load users.
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
        /// The <see cref="List"/>.
        /// </returns>
        public static List<EgrantsUsers> LoadUsers(
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
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
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
        public static string to_preview(
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
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
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

            while (rdr.Read())
                return_notice = rdr[0].ToString();

            conn.Close();

            return return_notice;
        }

        // update user data
        /// <summary>
        /// The run_db.
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
        public static void run_db(
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
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
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

        // to review user data inserted by IC Coordinator
        /// <summary>
        /// The to_ review.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<EgrantsUsers> To_Review(int id)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_access_control", conn);
            cmd.Parameters.Add("@act", SqlDbType.Int).Value = "review";
            cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = id;

            conn.Open();

            var Users = new List<EgrantsUsers>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Users.Add(
                    new EgrantsUsers
                        {
                            PersonId = rdr["person_id"]?.ToString(),
                            userid = rdr["userid"]?.ToString(),
                            last_name = rdr["last_name"]?.ToString(),
                            first_name = rdr["first_name"]?.ToString(),
                            middle_name = rdr["middle_name"]?.ToString(),
                            email_address = rdr["email"]?.ToString(),
                            phone_number = rdr["phone_number"]?.ToString(),
                            position_id = rdr["position_id"]?.ToString(),
                            position_name = rdr["position_name"]?.ToString(),
                            active = rdr["active"]?.ToString(),
                            ic = rdr["ic"]?.ToString(),
                            coordinator_id = rdr["coordinator_id"]?.ToString(),
                            start_date = rdr["start_date"]?.ToString(),
                            end_date = rdr["end_date"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return Users;
        }

        /// <summary>
        /// The load accept.
        /// </summary>
        /// <param name="accect_person_id">
        /// The accect_person_id.
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
        public static List<EgrantsUsers> LoadAccept(int accect_person_id, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_ic_coordinator", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = "accept";
            cmd.Parameters.Add("@cord_id", SqlDbType.Int).Value = 0;
            cmd.Parameters.Add("@request_user_id", SqlDbType.Int).Value = accect_person_id;
            cmd.Parameters.Add("@first_name", SqlDbType.VarChar).Value = string.Empty;
            cmd.Parameters.Add("@middle_name", SqlDbType.VarChar).Value = string.Empty;
            cmd.Parameters.Add("@last_name", SqlDbType.VarChar).Value = string.Empty;
            cmd.Parameters.Add("@login_id", SqlDbType.VarChar).Value = string.Empty;
            cmd.Parameters.Add("@email_address", SqlDbType.VarChar).Value = string.Empty;
            cmd.Parameters.Add("@phone_number", SqlDbType.VarChar).Value = string.Empty;
            cmd.Parameters.Add("@division", SqlDbType.VarChar).Value = string.Empty;
            cmd.Parameters.Add("@access_type", SqlDbType.VarChar).Value = string.Empty;
            cmd.Parameters.Add("@start_date", SqlDbType.VarChar).Value = string.Empty;
            cmd.Parameters.Add("@end_date", SqlDbType.VarChar).Value = string.Empty;
            cmd.Parameters.Add("@comments", SqlDbType.VarChar).Value = string.Empty;
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
                            last_name = rdr["last_name"]?.ToString(),
                            first_name = rdr["first_name"]?.ToString(),
                            middle_name = rdr["middle_name"]?.ToString(),
                            email_address = rdr["email"]?.ToString(),
                            phone_number = rdr["phone_number"]?.ToString(),
                            position_id = rdr["position_id"]?.ToString(),
                            position_name = rdr["position_name"]?.ToString(),
                            active = rdr["active"]?.ToString(),
                            ic = rdr["ic"]?.ToString(),
                            coordinator_id = rdr["coordinator_id"]?.ToString(),
                            start_date = rdr["start_date"]?.ToString(),
                            end_date = rdr["end_date"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return Users;
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
        public static int getCharacterIndex(string first_letter)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
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

        // check userid if exists in the system
        /// <summary>
        /// The to check userid.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int ToCheckUserid(string userid)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
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

        // public static List<EgrantsCommon.EgrantsUsers> LoadRequest(int id)
        // {
        // System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
        // System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select id as person_id, user_login as userid, user_fname as first_name, user_lname as last_name,user_mi as middle_name,"+
        // "email, phone_number, cord_id as coordinator_id, 1 as profile_id, access_type as active, pp.position_id, pp.position_name, 'nci' as ic, convert(varchar,start_date,101) as start_date, end_date " +
        // " FROM dbo.cord_manager inner join people_positions as pp on dbo.fn_get_position_id(access_type) = pp.position_id WHERE id = @id ", conn);
        // cmd.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = id;

        // conn.Open();

        // var Users = new List<EgrantsCommon.EgrantsUsers>();
        // SqlDataReader rdr = cmd.ExecuteReader();
        // while (rdr.Read())
        // {
        // Users.Add(new EgrantsCommon.EgrantsUsers
        // {
        // person_id = rdr["person_id"]?.ToString(),
        // userid = rdr["userid"]?.ToString(),
        // last_name = rdr["last_name"]?.ToString(),
        // first_name = rdr["first_name"]?.ToString(),
        // middle_name = rdr["middle_name"]?.ToString(),
        // email_address = rdr["email"]?.ToString(),
        // phone_number = rdr["phone_number"]?.ToString(),
        // position_id = rdr["position_id"]?.ToString(),
        // position_name = rdr["position_name"]?.ToString(),
        // active = rdr["active"]?.ToString(),
        // ic = rdr["ic"]?.ToString(),
        // coordinator_id = rdr["coordinator_id"]?.ToString(),
        // start_date = rdr["start_date"]?.ToString(),
        // end_date = rdr["end_date"]?.ToString()
        // });
        // }

        // rdr.Close();
        // conn.Close();

        // return Users;
        // }
    }
}