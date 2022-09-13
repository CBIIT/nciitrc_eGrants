#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  DocmanAccess.cs
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

using egrants_new.Models;

#endregion

namespace egrants_new.Egrants_Admin.Models
{
    /// <summary>
    /// The docman access.
    /// </summary>
    public class DocmanAccess
    {
        // to load user data
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
        /// <param name="docman_tab">
        /// The docman_tab.
        /// </param>
        /// <param name="cft_tab">
        /// The cft_tab.
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
        public static List<DocmanCommon.DocmanUsers> LoadUsers(
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
            int docman_tab,
            int cft_tab,
            int is_coordinator,
            string end_date,
            string ic,
            string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_docman_access_control", conn);
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
            cmd.Parameters.Add("@position_id", SqlDbType.Int).Value = position_id;
            cmd.Parameters.Add("@docman_tab", SqlDbType.Int).Value = docman_tab;
            cmd.Parameters.Add("@cft_tab", SqlDbType.Int).Value = cft_tab;
            cmd.Parameters.Add("@coordinator_id", SqlDbType.Int).Value = coordinator_id;
            cmd.Parameters.Add("@is_coordinator", SqlDbType.Int).Value = is_coordinator;
            cmd.Parameters.Add("@end_date", SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var Users = new List<DocmanCommon.DocmanUsers>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Users.Add(
                    new DocmanCommon.DocmanUsers
                        {
                            person_id = rdr["person_id"]?.ToString(),
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
                            can_cft = rdr["can_cft"]?.ToString(),
                            can_docman = rdr["can_docman"]?.ToString(),
                            is_coordinator = rdr["is_coordinator"]?.ToString(),
                            coordinator_id = rdr["coordinator_id"]?.ToString(),
                            start_date = rdr["start_date"]?.ToString(),
                            end_date = rdr["end_date"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return Users;
        }

        // to create or update user data            
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
        /// <param name="docman_tab">
        /// The docman_tab.
        /// </param>
        /// <param name="cft_tab">
        /// The cft_tab.
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
            int docman_tab,
            int cft_tab,
            int is_coordinator,
            string end_date,
            string ic,
            string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_docman_access_control", conn);
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
            cmd.Parameters.Add("@docman_tab", SqlDbType.Int).Value = docman_tab;
            cmd.Parameters.Add("@cft_tab", SqlDbType.Int).Value = cft_tab;
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
}