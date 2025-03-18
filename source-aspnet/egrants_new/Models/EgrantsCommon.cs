#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EgrantsCommon.cs
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
using System.IO;
using System.Web;

using DocumentFormat.OpenXml.Wordprocessing;

using egrants_new.Egrants.Functions;

using Hangfire.Annotations;

using MsgReader.Outlook;

#endregion

namespace egrants_new.Models
{
    /// <summary>
    ///     The egrants common.
    /// </summary>
    public class EgrantsCommon
    {
        public static readonly string[] UNSUPPORTED_FILE_TYPES = { ".xlsx", ".xls", ".xlst", ".xlsm" };

        /// <summary>
        /// Update the logged in users last_login_date to the value of Sql Sever GETDATE().
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool UpdateUsersLastLoginDate(string userId)
        {
            bool count = false;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand(
                    "UPDATE people SET last_login_date = GETDATE() where userid = @userId",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@userId", SqlDbType.VarChar).Value = userId;
                conn.Open();


                int i = cmd.ExecuteNonQuery();
            }

            return true;
        }

        // check user validation
        /// <summary>
        /// The check user validation.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="int"/> .
        /// </returns>
        public static int CheckUserValidation(string ic, string userid)
        {
            int count = 0;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("sp_web_egrants_user_validation", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

                conn.Open();

                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    count = Convert.ToInt16(rdr["count"]);
                }
            }

            return count;
        }

        // check user validation
        /// <summary>
        /// The check users exception.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="int"/> .
        /// </returns>
        public static int CheckUsersException(string userid)
        {
            int count = 0;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("sp_web_egrants_user_exception", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

                conn.Open();

                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    count = Convert.ToInt16(rdr["count"]);
                }
            }

            return count;
        }

        // check user application type
        /// <summary>
        /// The user type.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> .
        /// </returns>
        public static string UserType(string ic, string userid)
        {
            string applicationType = string.Empty;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("sp_web_egrants_user_type_check", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@Operator", SqlDbType.VarChar).Value = userid;
                cmd.Parameters.Add("@user_application_type", SqlDbType.VarChar, 2);
                cmd.Parameters["@user_application_type"].Direction = ParameterDirection.Output;
                conn.Open();

                int i = cmd.ExecuteNonQuery();

                applicationType = Convert.ToString(cmd.Parameters["@user_application_type"].Value);
            }

            return applicationType;
        }

        /// <summary>
        /// The uservar.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="System.Collections.Generic.IEnumerable`1"/> .
        /// </returns>
        public static IEnumerable<User> uservar(string userid, string ic, string type)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString;
            var list = new List<User>();

            using (var conn = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand("sp_web_egrants_user_profile", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ic", ic);
                cmd.Parameters.AddWithValue("@Operator", userid);
                cmd.Parameters.AddWithValue("@type", type);

                conn.Open();
                var user = new User();

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        if (rdr["nam"].ToString() == "VALIDATION")
                        {
                            user.Validation = rdr["val"].ToString();
                        }

                        if (rdr["nam"].ToString() == "USERID")
                        {
                            user.UserId = rdr["val"].ToString();
                        }

                        if (rdr["nam"].ToString() == "IC")
                        {
                            user.ic = rdr["val"].ToString();
                        }

                        if (rdr["nam"].ToString() == "PERSONID")
                        {
                            user.personID = Convert.ToInt32(rdr["val"]);
                        }

                        if (rdr["nam"].ToString() == "POSITIONID")
                        {
                            user.positionID = Convert.ToInt32(rdr["val"]);
                        }

                        if (rdr["nam"].ToString() == "ISCOORDINATOR")
                        {
                            user.isCoordinator = Convert.ToInt32(rdr["val"]);
                        }

                        if (rdr["nam"].ToString() == "USERNAME")
                        {
                            user.PersonName = rdr["val"].ToString();
                        }

                        if (rdr["nam"].ToString() == "USEREMAIL")
                        {
                            user.PersonEmail = rdr["val"].ToString();
                        }

                        if (rdr["nam"].ToString() == "MENULIST")
                        {
                            user.menulist = rdr["val"].ToString();
                        }
                    }
                }

                list.Add(user);
            }

            return list;
        }

        /// <summary>
        /// The load specialists.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <returns>
        /// The <see cref="System.Collections.Generic.List`1"/> .
        /// </returns>
        public static List<EgrantsUsers> LoadSpecialists(string ic)
        {
            List<EgrantsUsers> list = new List<EgrantsUsers>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand(
                    "SELECT person_name, person_id FROM vw_people WHERE ic=@IC and application_type='egrants' and position_id>1 and PATINDEX('%,%', person_name)>0 ORDER BY person_name",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                conn.Open();

                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new EgrantsUsers { PersonId = rdr["person_id"].ToString(), person_name = rdr["person_name"].ToString() });
                }
            }

            return list;
        }

        // load postions
        /// <summary>
        ///     The load positions.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<Position> LoadPositions()
        {
            List<Position> positions = new List<Position>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("select position_id, position_name from people_positions order by position_id", conn);
                cmd.CommandType = CommandType.Text;
                conn.Open();

                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    positions.Add(new Position { PositionId = rdr["position_id"].ToString(), PositionName = rdr["position_name"].ToString() });
                }
            }

            return positions;
        }

        /// <summary>
        ///     The load coordinators.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<EgrantsUsers> LoadCoordinators()
        {
            var list = new List<EgrantsUsers>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("select person_id, person_name from vw_people where is_coordinator=1 order by person_name", conn);
                cmd.CommandType = CommandType.Text;
                conn.Open();

                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new EgrantsUsers { PersonId = rdr["person_id"]?.ToString(), person_name = rdr["person_name"]?.ToString() });
                }
            }

            return list;
        }

        /// <summary>
        ///     The load admin codes.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<AdminCodes> LoadAdminCodes()
        {
            var list = new List<AdminCodes>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
            {

                var cmd = new SqlCommand(
                    "select distinct admin_phs_org_code, case when admin_phs_org_code = 'ca' then 'NCI' else null end as profile "
                  + " from grants ORDER BY admin_phs_org_code",
                    conn);

                // System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT DISTINCT a.admin_phs_org_code, p.profile as profile " +
                // " FROM vw_appls a LEFT OUTER JOIN profiles p ON a.admin_phs_org_code = p.admin_phs_org_code ORDER BY a.admin_phs_org_code", conn);
                cmd.CommandType = CommandType.Text;
                conn.Open();


                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new AdminCodes { profile = rdr["profile"]?.ToString(), admin_phs_org_code = rdr["admin_phs_org_code"]?.ToString() });
                }
            }

            return list;
        }

        /// <summary>
        ///     The load character index.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<CharacterIndex> LoadCharacterIndex()
        {
            var list = new List<CharacterIndex>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand(
                    "SELECT index_id, character_index, index_seq from dbo.character_index where index_id>1 order by index_seq",
                    conn);

                cmd.CommandType = CommandType.Text;
                conn.Open();


                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(
                        new CharacterIndex
                        {
                            index_id = rdr["index_id"]?.ToString(),
                            character_index = rdr["character_index"]?.ToString(),
                            index_seq = rdr["index_seq"]?.ToString()
                        });
                }

            }

            return list;
        }

        // load profile list
        /// <summary>
        ///     The load profiles.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<Profiles> LoadProfiles()
        {
            var list = new List<Profiles>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("select profile_id, [profile], admin_phs_org_code from profiles order by admin_phs_org_code", conn);
                cmd.CommandType = CommandType.Text;
                conn.Open();


                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(
                        new Profiles
                        {
                            ProfileId = rdr["profile_id"]?.ToString(),
                            Profile = rdr["profile"]?.ToString(),
                            AdminPhsOrgCode = rdr["admin_phs_org_code"]?.ToString()
                        });
                }
            }

            return list;
        }

        // load admin menu list
        /// <summary>
        /// The load admin menu.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="System.Collections.Generic.List`1"/> .
        /// </returns>
        public static List<AdminMenus> LoadAdminMenu(string userid)
        {
            var list = new List<AdminMenus>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
            {

                var cmd = new SqlCommand(
                    "select menu_id, menu_title, menu_action from vw_adm_menu_assignment where person_id=(select person_id from vw_people where menu_action is not null and userid = @userid) order by menu_title",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@userid", SqlDbType.VarChar).Value = userid;
                conn.Open();


                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(
                        new AdminMenus
                        {
                            MenuId = rdr["menu_id"]?.ToString(),
                            MenuTitle = rdr["menu_title"]?.ToString(),
                            MenuAction = rdr["menu_action"]?.ToString()
                        });
                }
            }

            return list;
        }

        // get person_id with userid
        /// <summary>
        /// The get person id.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="int"/> .
        /// </returns>
        public static int GetPersonID(string userid)
        {
            int personId = 0;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("select person_id from vw_people where userid=@userid", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@userid", SqlDbType.VarChar).Value = userid;
                conn.Open();


                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    personId = Convert.ToInt16(rdr["person_id"]);
                }
            }

            return personId;
        }

        // check uesr run  Admin Menu Permission by userid and menu_id
        /// <summary>
        /// The check admin menu permission.
        /// </summary>
        /// <param name="menu_id">
        /// The menu_id.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="int"/> .
        /// </returns>
        public static int CheckAdminMenuPermission(int menu_id, string userid)
        {
            int permission = 0;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
            {

                var cmd = new SqlCommand(
                    "select count(*) as Permission from vw_adm_menu_assignment where person_id=(select person_id from people where userid=@userid) and menu_id=@menu_id",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@menu_id", SqlDbType.Int).Value = menu_id;
                cmd.Parameters.Add("@userid", SqlDbType.VarChar).Value = userid;
                conn.Open();

                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    permission = Convert.ToInt16(rdr["Permission"]);
                }
            }

            return permission;
        }

        internal static bool ContainsUnsupportedFileTypes(IEnumerable<HttpPostedFileBase> inboundFiles)
        {
            foreach (var inboundFile in inboundFiles)
            {
                var fileName = Path.GetFileName(inboundFile.FileName);
                var fileExtension = Path.GetExtension(fileName);

                foreach (var unsupportedType in UNSUPPORTED_FILE_TYPES)
                {
                    if (inboundFile.FileName.ToLower().EndsWith(unsupportedType.ToLower()))
                        return true;
                }

                if (fileExtension.Equals(".msg", StringComparison.InvariantCultureIgnoreCase))
                {
                    byte[] fileData = inboundFile.ToByteArray();

                    using (var memoryStream = new MemoryStream(fileData))
                    {
                        var emailFile = new Storage.Message(memoryStream);
                        if (MessageContainsUnsupported(emailFile))
                        {
                            inboundFile.InputStream.Position = 0;
                            return true;
                        }
                    }

                    inboundFile.InputStream.Position = 0;
                }
            }
            return false;
        }

        private static bool MessageContainsUnsupported(Storage.Message emailFile)
        {
            foreach (var attachment in emailFile.Attachments)
            {
                if (attachment is Storage.Attachment storageAttachment)
                {
                    foreach (var unsupportedType in UNSUPPORTED_FILE_TYPES)
                    {
                        if (storageAttachment.FileName.ToLower().EndsWith(unsupportedType.ToLower()))
                            return true;
                    }
                }
                else if (attachment is Storage.Message messageAttachment)                                               // email message
                {
                    if (MessageContainsUnsupported((Storage.Message)attachment))
                        return true;
                }
            }
            return false;
        }
    }
}