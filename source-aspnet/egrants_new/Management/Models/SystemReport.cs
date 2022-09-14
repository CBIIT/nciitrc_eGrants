#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  SystemReport.cs
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

namespace egrants_new.Models
{
    /// <summary>
    /// The system report.
    /// </summary>
    public class SystemReport
    {

        /// <summary>
        /// The load accessions.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<EgrantAccessions> LoadAccessions(string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT accession_id,accession_number FROM eim.dbo.accessions WHERE contract = 0 and profile_id = (select profile_id from profiles where profile = @ic) order by accession_id desc",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            conn.Open();

            var Accessions = new List<EgrantAccessions>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Accessions.Add(
                    new EgrantAccessions { accession_id = rdr["accession_id"].ToString(), accession_number = rdr["accession_number"].ToString() });

            return Accessions;
        }

        /// <summary>
        /// The load folders.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="search_number">
        /// The search_number.
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
        public static List<EgrantFolders> LoadFolders(string act, int search_number, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_management_system_report", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@search_number", SqlDbType.Int).Value = search_number;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

            conn.Open();

            var EgrantsFolders = new List<EgrantFolders>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                EgrantsFolders.Add(
                    new EgrantFolders
                        {
                            folder_id = rdr["folder_id"]?.ToString(),
                            bar_code = rdr["bar_code"]?.ToString(),
                            grant_num = rdr["grant_num"]?.ToString(),
                            former_grant_num = rdr["former_grant_num"]?.ToString(),
                            id_string = rdr["id_string"]?.ToString(),
                            latest_move_date = rdr["latest_move_date"]?.ToString(),
                            current_status = rdr["current_status"]?.ToString(),
                            closed_out = rdr["closed_out"]?.ToString(),
                            accession_destroyed_date = rdr["accession_destroyed_date"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return EgrantsFolders;
        }

        /// <summary>
        /// The egrant accessions.
        /// </summary>
        public class EgrantAccessions
        {
            /// <summary>
            /// Gets or sets the accession_id.
            /// </summary>
            public string accession_id { get; set; }

            /// <summary>
            /// Gets or sets the accession_number.
            /// </summary>
            public string accession_number { get; set; }

            /// <summary>
            /// Gets or sets the accession_year.
            /// </summary>
            public string accession_year { get; set; }

            /// <summary>
            /// Gets or sets the accession_counter.
            /// </summary>
            public string accession_counter { get; set; }
        }

        /// <summary>
        /// The egrant folders.
        /// </summary>
        public class EgrantFolders
        {
            /// <summary>
            /// Gets or sets the folder_id.
            /// </summary>
            public string folder_id { get; set; }

            /// <summary>
            /// Gets or sets the grant_num.
            /// </summary>
            public string grant_num { get; set; }

            /// <summary>
            /// Gets or sets the bar_code.
            /// </summary>
            public string bar_code { get; set; }

            /// <summary>
            /// Gets or sets the former_grant_num.
            /// </summary>
            public string former_grant_num { get; set; }

            /// <summary>
            /// Gets or sets the id_string.
            /// </summary>
            public string id_string { get; set; }

            /// <summary>
            /// Gets or sets the latest_move_date.
            /// </summary>
            public string latest_move_date { get; set; }

            /// <summary>
            /// Gets or sets the current_status.
            /// </summary>
            public string current_status { get; set; }

            /// <summary>
            /// Gets or sets the closed_out.
            /// </summary>
            public string closed_out { get; set; }

            /// <summary>
            /// Gets or sets the accession_destroyed_date.
            /// </summary>
            public string accession_destroyed_date { get; set; }
        }
    }
}