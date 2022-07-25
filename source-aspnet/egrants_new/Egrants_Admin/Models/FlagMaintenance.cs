#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  FlagMaintenance.cs
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

namespace egrants_new.Egrants_Admin.Models
{
    /// <summary>
    ///     The flag maintenance.
    /// </summary>
    public class FlagMaintenance
    {
        /// <summary>
        ///     The load flag types.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<FlagTypes> LoadFlagTypes()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT UPPER(flag_type_code) as flag_type_code, flag_application_code FROM Grants_Flag_Master WHERE end_date is null",
                conn);

            cmd.CommandType = CommandType.Text;

            conn.Open();

            var list = new List<FlagTypes>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                list.Add(
                    new FlagTypes { flag_type = rdr["flag_type_code"]?.ToString(), flag_application = rdr["flag_application_code"]?.ToString() });

            rdr.Close();
            conn.Close();

            return list;
        }

        // load flags
        /// <summary>
        /// The load flags.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <param name="id_string">
        /// The id_string.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="System.Collections.Generic.List`1"/> .
        /// </returns>
        public static List<Flags> LoadFlags(
            string act,
            string flag_type,
            string admin_code,
            int serial_num,
            string id_string,
            string ic,
            string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("dbo.sp_web_admin_flag_maintenance", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@flag_type", SqlDbType.VarChar).Value = flag_type;
            cmd.Parameters.Add("@admin_code", SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@serial_num", SqlDbType.Int).Value = serial_num;
            cmd.Parameters.Add("@id_string", SqlDbType.VarChar).Value = id_string;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

            conn.Open();

            var list = new List<Flags>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                list.Add(
                    new Flags
                        {
                            gf_id = rdr["gf_id"]?.ToString(),
                            serial_num = rdr["serial_num"]?.ToString(),
                            grant_id = rdr["grant_id"]?.ToString(),
                            appl_id = rdr["appl_id"]?.ToString(),
                            grant_num = rdr["grant_num"]?.ToString(),
                            full_grant_num = rdr["full_grant_num"]?.ToString(),
                            flag = rdr["flag"]?.ToString(),
                            flag_type = rdr["flag_type"]?.ToString(),
                            flag_application = rdr["flag_application"]?.ToString(),
                            flag_icon_namepath = rdr["flag_icon_namepath"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return list;
        }

        // load appls with flag
        /// <summary>
        /// The load appls.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <param name="id_string">
        /// The id_string.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="System.Collections.Generic.List`1"/> .
        /// </returns>
        public static List<ApplFlags> LoadAppls(
            string act,
            string flag_type,
            string admin_code,
            int serial_num,
            string id_string,
            string ic,
            string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("dbo.sp_web_admin_flag_maintenance", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@flag_type", SqlDbType.VarChar).Value = flag_type;
            cmd.Parameters.Add("@admin_code", SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@serial_num", SqlDbType.Int).Value = serial_num;
            cmd.Parameters.Add("@id_string", SqlDbType.VarChar).Value = id_string;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

            conn.Open();

            var list = new List<ApplFlags>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                list.Add(
                    new ApplFlags
                        {
                            appl_id = rdr["appl_id"]?.ToString(),
                            fgn = rdr["fgn"]?.ToString(),
                            creator = rdr["creator"]?.ToString(),
                            created_date = rdr["created_date"]?.ToString(),
                            exclusion_reason = rdr["exclusion_reason"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return list;
        }

        // add, delete or edit flag
        /// <summary>
        /// The run_db.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <param name="id_string">
        /// The id_string.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        public static void run_db(string act, string flag_type, string admin_code, int serial_num, string id_string, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("dbo.sp_web_admin_flag_maintenance", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@flag_type", SqlDbType.VarChar).Value = flag_type;
            cmd.Parameters.Add("@admin_code", SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@serial_num", SqlDbType.Int).Value = serial_num;
            cmd.Parameters.Add("@id_string", SqlDbType.VarChar).Value = id_string;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            conn.Open();
            var rdr = cmd.ExecuteReader();
            rdr.Close();
            conn.Close();
        }

        /// <summary>
        ///     The flag types.
        /// </summary>
        public class FlagTypes
        {
            /// <summary>
            ///     Gets or sets the flag_type.
            /// </summary>
            public string flag_type { get; set; }

            /// <summary>
            ///     Gets or sets the flag_application.
            /// </summary>
            public string flag_application { get; set; }
        }

        /// <summary>
        ///     The flags.
        /// </summary>
        public class Flags
        {
            /// <summary>
            ///     Gets or sets the gf_id.
            /// </summary>
            public string gf_id { get; set; }

            /// <summary>
            ///     Gets or sets the serial_num.
            /// </summary>
            public string serial_num { get; set; }

            /// <summary>
            ///     Gets or sets the grant_id.
            /// </summary>
            public string grant_id { get; set; }

            /// <summary>
            ///     Gets or sets the appl_id.
            /// </summary>
            public string appl_id { get; set; }

            /// <summary>
            ///     Gets or sets the grant_num.
            /// </summary>
            public string grant_num { get; set; }

            /// <summary>
            ///     Gets or sets the full_grant_num.
            /// </summary>
            public string full_grant_num { get; set; }

            /// <summary>
            ///     Gets or sets the flag.
            /// </summary>
            public string flag { get; set; }

            /// <summary>
            ///     Gets or sets the flag_type.
            /// </summary>
            public string flag_type { get; set; }

            /// <summary>
            ///     Gets or sets the flag_application.
            /// </summary>
            public string flag_application { get; set; }

            /// <summary>
            ///     Gets or sets the flag_icon_namepath.
            /// </summary>
            public string flag_icon_namepath { get; set; }
        }

        /// <summary>
        ///     The appl flags.
        /// </summary>
        public class ApplFlags
        {
            /// <summary>
            ///     Gets or sets the appl_id.
            /// </summary>
            public string appl_id { get; set; }

            /// <summary>
            ///     Gets or sets the fgn.
            /// </summary>
            public string fgn { get; set; }

            /// <summary>
            ///     Gets or sets the creator.
            /// </summary>
            public string creator { get; set; }

            /// <summary>
            ///     Gets or sets the created_date.
            /// </summary>
            public string created_date { get; set; }

            /// <summary>
            ///     Gets or sets the exclusion_reason.
            /// </summary>
            public string exclusion_reason { get; set; }
        }
    }
}