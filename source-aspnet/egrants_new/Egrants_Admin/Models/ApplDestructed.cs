#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  ApplDestructed.cs
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
    /// The appl destructed.
    /// </summary>
    public class ApplDestructed
    {
        /// <summary>
        /// The load years.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<DestructionYears> LoadYears()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT distinct year(EGRANTS_CREATED_DATE) as [year] FROM dbo.IMPAC_DESTRUCTED_APPL order by [year] desc",
                conn);

            conn.Open();

            var Years = new List<DestructionYears>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Years.Add(new DestructionYears { year = rdr["year"]?.ToString() });

            rdr.Close();
            conn.Close();

            return Years;
        }

        /// <summary>
        /// The load descrip codes.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<DescripCodes> LoadDescripCodes()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT distinct APPL_STATUS_GRP_DESCRIP as descrip_code FROM dbo.IMPAC_DESTRUCTED_APPL "
              + "WHERE APPL_STATUS_GRP_DESCRIP is not null ORDER BY APPL_STATUS_GRP_DESCRIP",
                conn);

            conn.Open();

            var Codes = new List<DescripCodes>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Codes.Add(new DescripCodes { descrip_code = rdr["descrip_code"]?.ToString() });

            rdr.Close();
            conn.Close();

            return Codes;
        }

        /// <summary>
        /// The load exception codes.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<ExceptionCodes> LoadExceptionCodes()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT id, code as exception_code, detail, convert(varchar,created_date,101) as created_date, dbo.fn_get_person_name(created_by_person_id) as created_by "
              + " FROM dbo.IMPAC_DESTRUCT_OGA_EXCEPTION WHERE disable_date is null ORDER BY exception_code",
                conn);

            conn.Open();

            var Codes = new List<ExceptionCodes>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Codes.Add(
                    new ExceptionCodes
                        {
                            id = rdr["id"]?.ToString(),
                            exception_code = rdr["exception_code"]?.ToString(),
                            detail = rdr["detail"]?.ToString(),
                            created_date = rdr["created_date"]?.ToString(),
                            created_by = rdr["created_by"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return Codes;
        }

        /// <summary>
        /// The load appls.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="year">
        /// The year.
        /// </param>
        /// <param name="status_code">
        /// The status_code.
        /// </param>
        /// <param name="exception_code">
        /// The exception_code.
        /// </param>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="id_string">
        /// The id_string.
        /// </param>
        /// <param name="exception_type">
        /// The exception_type.
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
        public static List<DestructedsAppls> LoadAppls(
            string act,
            int year,
            string status_code,
            string exception_code,
            string str,
            string id_string,
            string exception_type,
            string ic,
            string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_admin_appl_destructed", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@year", SqlDbType.Int).Value = year;
            cmd.Parameters.Add("@status_code", SqlDbType.VarChar).Value = status_code;
            cmd.Parameters.Add("@exception_code", SqlDbType.VarChar).Value = exception_code;
            cmd.Parameters.Add("@str", SqlDbType.VarChar).Value = str;
            cmd.Parameters.Add("@id_string", SqlDbType.VarChar).Value = id_string;
            cmd.Parameters.Add("@exception_type", SqlDbType.VarChar).Value = exception_type;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var Appls = new List<DestructedsAppls>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Appls.Add(
                    new DestructedsAppls
                        {
                            appl_id = rdr["appl_id"]?.ToString(),
                            full_grant_num = rdr["full_grant_num"]?.ToString(),
                            serial_num = rdr["serial_num"]?.ToString(),
                            exception_code = rdr["exception_code"]?.ToString(),
                            status_code = rdr["status_code"]?.ToString(),
                            step_code = rdr["step_code"]?.ToString(),
                            appl_editable = rdr["appl_editable"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return Appls;
        }

        /// <summary>
        /// The load search info.
        /// </summary>
        /// <param name="year">
        /// The year.
        /// </param>
        /// <param name="status_code">
        /// The status_code.
        /// </param>
        /// <param name="exception_code">
        /// The exception_code.
        /// </param>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<SearchInfo> LoadSearchInfo(int year, string status_code, string exception_code, string str)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_admin_appl_destructed_index", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@year", SqlDbType.Int).Value = year;
            cmd.Parameters.Add("@status_code", SqlDbType.VarChar).Value = status_code;
            cmd.Parameters.Add("@exception_code", SqlDbType.VarChar).Value = exception_code;
            cmd.Parameters.Add("@str", SqlDbType.VarChar).Value = str;

            conn.Open();

            var SearchInfo = new List<SearchInfo>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                SearchInfo.Add(
                    new SearchInfo
                        {
                            total_appls = rdr["total_appls"]?.ToString(),
                            total_pages = rdr["total_pages"]?.ToString(),
                            per_page = rdr["per_page"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return SearchInfo;
        }

        /// <summary>
        /// The check permission.
        /// </summary>
        /// <param name="year">
        /// The year.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string CheckPermission(int year, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "select dbo.fn_is_Archival_admin(@year,(select person_id from people where userid=@userid)) as permission",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@year", SqlDbType.Int).Value = year;
            cmd.Parameters.Add("@userid", SqlDbType.VarChar).Value = userid;
            conn.Open();
            var Processable = (string)cmd.ExecuteScalar();
            conn.Close();

            return Processable;
        }

        /// <summary>
        /// The edit exception code.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="detail">
        /// The detail.
        /// </param>
        /// <param name="code">
        /// The code.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        public static void EditExceptionCode(string act, int id, string detail, string code, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_admin_appl_destructed_edit", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@detail", SqlDbType.VarChar).Value = detail;
            cmd.Parameters.Add("@code", SqlDbType.VarChar).Value = code;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@Operator", SqlDbType.VarChar).Value = userid;
            conn.Open();
            var rdr = cmd.ExecuteReader();
            rdr.Close();
            conn.Close();
        }

        /// <summary>
        /// The destruction years.
        /// </summary>
        public class DestructionYears
        {
            /// <summary>
            /// Gets or sets the year.
            /// </summary>
            public string year { get; set; }
        }

        /// <summary>
        /// The descrip codes.
        /// </summary>
        public class DescripCodes
        {
            /// <summary>
            /// Gets or sets the descrip_code.
            /// </summary>
            public string descrip_code { get; set; }
        }

        /// <summary>
        /// The exception codes.
        /// </summary>
        public class ExceptionCodes
        {
            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// Gets or sets the exception_code.
            /// </summary>
            public string exception_code { get; set; }

            /// <summary>
            /// Gets or sets the detail.
            /// </summary>
            public string detail { get; set; }

            /// <summary>
            /// Gets or sets the created_date.
            /// </summary>
            public string created_date { get; set; }

            /// <summary>
            /// Gets or sets the created_by.
            /// </summary>
            public string created_by { get; set; }
        }

        /// <summary>
        /// The destructeds appls.
        /// </summary>
        public class DestructedsAppls
        {
            /// <summary>
            /// Gets or sets the appl_id.
            /// </summary>
            public string appl_id { get; set; }

            /// <summary>
            /// Gets or sets the full_grant_num.
            /// </summary>
            public string full_grant_num { get; set; }

            /// <summary>
            /// Gets or sets the serial_num.
            /// </summary>
            public string serial_num { get; set; }

            /// <summary>
            /// Gets or sets the exception_code.
            /// </summary>
            public string exception_code { get; set; }

            /// <summary>
            /// Gets or sets the status_code.
            /// </summary>
            public string status_code { get; set; }

            /// <summary>
            /// Gets or sets the step_code.
            /// </summary>
            public string step_code { get; set; }

            /// <summary>
            /// Gets or sets the appl_editable.
            /// </summary>
            public string appl_editable { get; set; }
        }

        /// <summary>
        /// The search info.
        /// </summary>
        public class SearchInfo
        {
            /// <summary>
            /// Gets or sets the total_appls.
            /// </summary>
            public string total_appls { get; set; }

            /// <summary>
            /// Gets or sets the total_pages.
            /// </summary>
            public string total_pages { get; set; }

            /// <summary>
            /// Gets or sets the per_page.
            /// </summary>
            public string per_page { get; set; }
        }
    }
}