#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  QCAssignment.cs
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
    ///     The qc assignment.
    /// </summary>
    public class QCAssignment
    {

        /// <summary>
        /// The load qc reasons.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <returns>
        /// The <see cref="System.Collections.Generic.List`1"/> .
        /// </returns>
        public static List<QCReasons> LoadQCReasons(string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("SELECT distinct qc_reason FROM vw_quality_control WHERE profile = @ic ORDER BY qc_reason", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            conn.Open();

            var qcReasons = new List<QCReasons>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                qcReasons.Add(new QCReasons { qc_reason = rdr["qc_reason"].ToString() });
            }

            return qcReasons;
        }

        /// <summary>
        /// The load qc persons.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <returns>
        /// The <see cref="System.Collections.Generic.List`1"/> .
        /// </returns>
        public static List<QCPersons> LoadQCPersons(string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT qc_reason, userid, person_id, person_name FROM vw_quality_control WHERE profile=@ic order by qc_reason",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            conn.Open();

            var qcPeasons = new List<QCPersons>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                qcPeasons.Add(
                    new QCPersons
                        {
                            qc_reason = rdr["qc_reason"].ToString(),
                            userid = rdr["userid"].ToString(),
                            person_id = rdr["person_id"].ToString(),
                            person_name = rdr["person_name"].ToString()
                        });
            }

            return qcPeasons;
        }

        /// <summary>
        /// The load qc report.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <returns>
        /// The <see cref="System.Collections.Generic.List`1"/> .
        /// </returns>
        public static List<QCReports> LoadQCReport(string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                @"	with qc as (Select count(*) as files_to_qc, 
            AVG(datediff(d, qc_date, getdate())) as qc_days,
            qc_person_id from egrants where
            qc_date is not null
            and qc_person_id is not null
            and qc_reason is not null
            and disabled_date is null
            and ic = @ic
            AND parent_id IS null
            and not(grant_id is null) 
            group by qc_person_id)

            Select qc.files_to_qc, qc.qc_days,
            qc.qc_person_id, COALESCE(vp.person_name, CAST(qc.qc_person_id as varchar(10))) as qc_person_name
            from qc inner join vw_people vp on qc.qc_person_id = vp.person_id",
                conn
            );

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            conn.Open();

            var QCReport = new List<QCReports>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                QCReport.Add(
                    new QCReports
                        {
                            files_to_qc = rdr["files_to_qc"].ToString(),
                            qc_person_id = rdr["qc_person_id"].ToString(),
                            qc_person_name = rdr["qc_person_name"].ToString(),
                            qc_days = rdr["qc_days"].ToString()
                        }
                );

            return QCReport;
        }

        /// <summary>
        /// The run_db.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="qc_person_id">
        /// The qc_person_id.
        /// </param>
        /// <param name="qc_reason">
        /// The qc_reason.
        /// </param>
        /// <param name="percent">
        /// The percent.
        /// </param>
        /// <param name="person_id">
        /// The person_id.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        public static void run_db(string act, int qc_person_id, string qc_reason, int percent, int person_id, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_management_qc_assign", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@person_id", SqlDbType.Int).Value = person_id;
            cmd.Parameters.Add("@qc_person_id", SqlDbType.Int).Value = qc_person_id;
            cmd.Parameters.Add("@qc_reason", SqlDbType.VarChar).Value = qc_reason;
            cmd.Parameters.Add("@percent", SqlDbType.Int).Value = percent;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

            conn.Open();
            var DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();
        }

        /// <summary>
        ///     The qc reasons.
        /// </summary>
        public class QCReasons
        {
            /// <summary>
            ///     Gets or sets the qc_reason.
            /// </summary>
            public string qc_reason { get; set; }
        }

        /// <summary>
        ///     The qc persons.
        /// </summary>
        public class QCPersons
        {
            /// <summary>
            ///     Gets or sets the qc_reason.
            /// </summary>
            public string qc_reason { get; set; }

            /// <summary>
            ///     Gets or sets the userid.
            /// </summary>
            public string userid { get; set; }

            /// <summary>
            ///     Gets or sets the person_id.
            /// </summary>
            public string person_id { get; set; }

            /// <summary>
            ///     Gets or sets the person_name.
            /// </summary>
            public string person_name { get; set; }
        }

        /// <summary>
        ///     The qc reports.
        /// </summary>
        public class QCReports
        {
            /// <summary>
            ///     Gets or sets the qc_days.
            /// </summary>
            public string qc_days { get; set; }

            /// <summary>
            ///     Gets or sets the files_to_qc.
            /// </summary>
            public string files_to_qc { get; set; }

            /// <summary>
            ///     Gets or sets the qc_person_id.
            /// </summary>
            public string qc_person_id { get; set; }

            /// <summary>
            ///     Gets or sets the qc_person_name.
            /// </summary>
            public string qc_person_name { get; set; }
        }
    }
}