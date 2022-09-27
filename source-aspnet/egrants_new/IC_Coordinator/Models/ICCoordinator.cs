#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  ICCoordinator.cs
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

namespace egrants_new.IC_Coordinator.Models
{
    /// <summary>
    /// The coordinator.
    /// </summary>
    public class Coordinator
    {

        /// <summary>
        /// The load requested users.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="cord_id">
        /// The cord_id.
        /// </param>
        /// <param name="request_user_id">
        /// The request_user_id.
        /// </param>
        /// <param name="first_name">
        /// The first_name.
        /// </param>
        /// <param name="middle_name">
        /// The middle_name.
        /// </param>
        /// <param name="last_name">
        /// The last_name.
        /// </param>
        /// <param name="login_id">
        /// The login_id.
        /// </param>
        /// <param name="email_address">
        /// The email_address.
        /// </param>
        /// <param name="phone_number">
        /// The phone_number.
        /// </param>
        /// <param name="division">
        /// The division.
        /// </param>
        /// <param name="access_type">
        /// The access_type.
        /// </param>
        /// <param name="start_date">
        /// The start_date.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <param name="comments">
        /// The comments.
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
        public static List<RequestedUsers> LoadRequestedUsers(
            string act,
            int cord_id,
            int request_user_id,
            string first_name,
            string middle_name,
            string last_name,
            string login_id,
            string email_address,
            string phone_number,
            string division,
            string access_type,
            string start_date,
            string end_date,
            string comments,
            string ic,
            string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_ic_coordinator", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@cord_id", SqlDbType.Int).Value = cord_id;
            cmd.Parameters.Add("@request_user_id", SqlDbType.Int).Value = request_user_id;
            cmd.Parameters.Add("@first_name", SqlDbType.VarChar).Value = first_name;
            cmd.Parameters.Add("@middle_name", SqlDbType.VarChar).Value = middle_name;
            cmd.Parameters.Add("@last_name", SqlDbType.VarChar).Value = last_name;
            cmd.Parameters.Add("@login_id", SqlDbType.VarChar).Value = login_id;
            cmd.Parameters.Add("@email_address", SqlDbType.VarChar).Value = email_address;
            cmd.Parameters.Add("@phone_number", SqlDbType.VarChar).Value = phone_number;
            cmd.Parameters.Add("@division", SqlDbType.VarChar).Value = division;
            cmd.Parameters.Add("@access_type", SqlDbType.VarChar).Value = access_type;
            cmd.Parameters.Add("@start_date", SqlDbType.VarChar).Value = start_date;
            cmd.Parameters.Add("@end_date", SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@comments", SqlDbType.VarChar).Value = comments;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

            conn.Open();

            var Users = new List<RequestedUsers>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Users.Add(
                    new RequestedUsers
                        {
                            id = rdr["id"]?.ToString(),
                            first_name = rdr["first_name"]?.ToString(),
                            middle_name = rdr["middle_name"]?.ToString(),
                            last_name = rdr["last_name"]?.ToString(),
                            userid = rdr["userid"]?.ToString(),
                            division = rdr["division"]?.ToString(),
                            status = rdr["status"]?.ToString(),
                            access_type = rdr["access_type"]?.ToString(),
                            email_address = rdr["email_address"]?.ToString(),
                            phone_number = rdr["phone_number"]?.ToString(),
                            review_by_person_id = rdr["review_by_person_id"]?.ToString(),
                            created_by_person_id = rdr["created_by_person_id"]?.ToString(),
                            start_date = rdr["start_date"]?.ToString(),
                            end_date = rdr["end_date"]?.ToString(),
                            status_date = rdr["status_date"]?.ToString(),
                            requested_date = rdr["requested_date"]?.ToString(),
                            lastaccess_date = rdr["lastaccess_date"]?.ToString(),
                            coordinator_name = rdr["coordinator_name"]?.ToString(),
                            coordinator_id = rdr["coordinator_id"]?.ToString(),
                            comments = rdr["comments"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return Users;
        }

        /// <summary>
        /// The access users.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="cord_id">
        /// The cord_id.
        /// </param>
        /// <param name="request_user_id">
        /// The request_user_id.
        /// </param>
        /// <param name="first_name">
        /// The first_name.
        /// </param>
        /// <param name="middle_name">
        /// The middle_name.
        /// </param>
        /// <param name="last_name">
        /// The last_name.
        /// </param>
        /// <param name="login_id">
        /// The login_id.
        /// </param>
        /// <param name="email_address">
        /// The email_address.
        /// </param>
        /// <param name="phone_number">
        /// The phone_number.
        /// </param>
        /// <param name="division">
        /// The division.
        /// </param>
        /// <param name="access_type">
        /// The access_type.
        /// </param>
        /// <param name="start_date">
        /// The start_date.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <param name="comments">
        /// The comments.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        public static void AccessUsers(
            string act,
            int cord_id,
            int request_user_id,
            string first_name,
            string middle_name,
            string last_name,
            string login_id,
            string email_address,
            string phone_number,
            string division,
            string access_type,
            string start_date,
            string end_date,
            string comments,
            string ic,
            string userid)
        {
            if (middle_name == "null")
                middle_name = string.Empty;

            if (end_date == "null")
                end_date = string.Empty;

            if (comments == "null")
                comments = string.Empty;

            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_ic_coordinator", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@cord_id", SqlDbType.Int).Value = cord_id;
            cmd.Parameters.Add("@request_user_id", SqlDbType.Int).Value = request_user_id;
            cmd.Parameters.Add("@first_name", SqlDbType.VarChar).Value = first_name;
            cmd.Parameters.Add("@middle_name", SqlDbType.VarChar).Value = middle_name;
            cmd.Parameters.Add("@last_name", SqlDbType.VarChar).Value = last_name;
            cmd.Parameters.Add("@login_id", SqlDbType.VarChar).Value = login_id;
            cmd.Parameters.Add("@email_address", SqlDbType.VarChar).Value = email_address;
            cmd.Parameters.Add("@phone_number", SqlDbType.VarChar).Value = phone_number;
            cmd.Parameters.Add("@division", SqlDbType.VarChar).Value = division;
            cmd.Parameters.Add("@access_type", SqlDbType.VarChar).Value = access_type;
            cmd.Parameters.Add("@start_date", SqlDbType.VarChar).Value = start_date;
            cmd.Parameters.Add("@end_date", SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@comments", SqlDbType.VarChar).Value = comments;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

            conn.Open();
            var DataReader = cmd.ExecuteReader();
            DataReader.Close();
            conn.Close();
        }

        // load Coordinators
        // public static List<EgrantsCommon.EgrantsUsers> LoadCoordinators()
        // {
        // System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
        // System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select person_id, person_name from vw_people where is_coordinator=1 order by person_name", conn);
        // cmd.CommandType = CommandType.Text;
        // conn.Open();

        // var Coordinators = new List<EgrantsCommon.EgrantsUsers>();
        // SqlDataReader rdr = cmd.ExecuteReader();
        // while (rdr.Read())
        // {
        // Coordinators.Add(new EgrantsCommon.EgrantsUsers
        // {
        // person_id = rdr["person_id"]?.ToString(),
        // person_name = rdr["person_name"]?.ToString(),
        // });
        // }
        // rdr.Close();
        // conn.Close();
        // return Coordinators;
        // }

        /// <summary>
        /// The requested users.
        /// </summary>
        public class RequestedUsers
        {
            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// Gets or sets the person_id.
            /// </summary>
            public string person_id { get; set; }

            /// <summary>
            /// Gets or sets the person_name.
            /// </summary>
            public string person_name { get; set; }

            /// <summary>
            /// Gets or sets the userid.
            /// </summary>
            public string userid { get; set; }

            /// <summary>
            /// Gets or sets the first_name.
            /// </summary>
            public string first_name { get; set; }

            /// <summary>
            /// Gets or sets the middle_name.
            /// </summary>
            public string middle_name { get; set; }

            /// <summary>
            /// Gets or sets the last_name.
            /// </summary>
            public string last_name { get; set; }

            /// <summary>
            /// Gets or sets the phone_number.
            /// </summary>
            public string phone_number { get; set; }

            /// <summary>
            /// Gets or sets the email_address.
            /// </summary>
            public string email_address { get; set; }

            /// <summary>
            /// Gets or sets the profile_id.
            /// </summary>
            public string profile_id { get; set; }

            /// <summary>
            /// Gets or sets the position_id.
            /// </summary>
            public string position_id { get; set; }

            /// <summary>
            /// Gets or sets the position_name.
            /// </summary>
            public string position_name { get; set; }

            /// <summary>
            /// Gets or sets the access_type.
            /// </summary>
            public string access_type { get; set; }

            /// <summary>
            /// Gets or sets the comments.
            /// </summary>
            public string comments { get; set; }

            /// <summary>
            /// Gets or sets the division.
            /// </summary>
            public string division { get; set; }

            /// <summary>
            /// Gets or sets the application_type.
            /// </summary>
            public string application_type { get; set; }

            /// <summary>
            /// Gets or sets the status.
            /// </summary>
            public string status { get; set; }

            /// <summary>
            /// Gets or sets the coordinator_id.
            /// </summary>
            public string coordinator_id { get; set; }

            /// <summary>
            /// Gets or sets the coordinator_name.
            /// </summary>
            public string coordinator_name { get; set; }

            /// <summary>
            /// Gets or sets the start_date.
            /// </summary>
            public string start_date { get; set; }

            /// <summary>
            /// Gets or sets the status_date.
            /// </summary>
            public string status_date { get; set; }

            /// <summary>
            /// Gets or sets the end_date.
            /// </summary>
            public string end_date { get; set; }

            /// <summary>
            /// Gets or sets the create_date.
            /// </summary>
            public string create_date { get; set; }

            /// <summary>
            /// Gets or sets the created_by_person_id.
            /// </summary>
            public string created_by_person_id { get; set; }

            /// <summary>
            /// Gets or sets the review_by_person_id.
            /// </summary>
            public string review_by_person_id { get; set; }

            /// <summary>
            /// Gets or sets the lastaccess_date.
            /// </summary>
            public string lastaccess_date { get; set; }

            /// <summary>
            /// Gets or sets the requested_date.
            /// </summary>
            public string requested_date { get; set; }
        }
    }
}