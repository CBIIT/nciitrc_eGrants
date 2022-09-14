#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  GPMATWorkReport.cs
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
    /// The gpmat work report.
    /// </summary>
    public class GPMATWorkReport
    {

        /// <summary>
        /// The load reports.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<PMATWorkReports> LoadReports(string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_admin_gpmat_workload", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var Reports = new List<PMATWorkReports>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Reports.Add(
                    new PMATWorkReports
                        {
                            specialist_name = rdr["specialist_name"]?.ToString(),
                            specialist_code = rdr["specialist_code"]?.ToString(),
                            branch = rdr["branch"]?.ToString(),
                            team = rdr["team"]?.ToString(),
                            fy = rdr["fy"]?.ToString(),
                            OCT_CNT = rdr["OCT_CNT"]?.ToString(),
                            OCT_REL = rdr["OCT_REL"]?.ToString(),
                            OCT_WRKLD = rdr["OCT_WRKLD"]?.ToString(),
                            NOV_CNT = rdr["NOV_CNT"]?.ToString(),
                            NOV_REL = rdr["NOV_REL"]?.ToString(),
                            NOV_WRKLD = rdr["NOV_WRKLD"]?.ToString(),
                            DEC_CNT = rdr["DEC_CNT"]?.ToString(),
                            DEC_REL = rdr["DEC_REL"]?.ToString(),
                            DEC_WRKLD = rdr["DEC_WRKLD"]?.ToString(),
                            JAN_CNT = rdr["JAN_CNT"]?.ToString(),
                            JAN_REL = rdr["JAN_REL"]?.ToString(),
                            JAN_WRKLD = rdr["JAN_WRKLD"]?.ToString(),
                            FEB_CNT = rdr["FEB_CNT"]?.ToString(),
                            FEB_REL = rdr["FEB_REL"]?.ToString(),
                            FEB_WRKLD = rdr["FEB_WRKLD"]?.ToString(),
                            MAR_CNT = rdr["MAR_CNT"]?.ToString(),
                            MAR_REL = rdr["MAR_REL"]?.ToString(),
                            MAR_WRKLD = rdr["MAR_WRKLD"]?.ToString(),
                            APR_CNT = rdr["APR_CNT"]?.ToString(),
                            APR_REL = rdr["APR_REL"]?.ToString(),
                            APR_WRKLD = rdr["APR_WRKLD"]?.ToString(),
                            MAY_CNT = rdr["MAY_CNT"]?.ToString(),
                            MAY_REL = rdr["MAY_REL"]?.ToString(),
                            MAY_WRKLD = rdr["MAY_WRKLD"]?.ToString(),
                            JUN_CNT = rdr["JUN_CNT"]?.ToString(),
                            JUN_REL = rdr["JUN_REL"]?.ToString(),
                            JUN_WRKLD = rdr["JUN_WRKLD"]?.ToString(),
                            JUL_CNT = rdr["JUL_CNT"]?.ToString(),
                            JUL_REL = rdr["JUL_REL"]?.ToString(),
                            JUL_WRKLD = rdr["JUL_WRKLD"]?.ToString(),
                            AUG_CNT = rdr["AUG_CNT"]?.ToString(),
                            AUG_REL = rdr["AUG_REL"]?.ToString(),
                            AUG_WRKLD = rdr["AUG_WRKLD"]?.ToString(),
                            SEP_CNT = rdr["SEP_CNT"]?.ToString(),
                            SEP_REL = rdr["SEP_REL"]?.ToString(),
                            SEP_WRKLD = rdr["SEP_WRKLD"]?.ToString(),
                            TOTAL_CNT = rdr["TOTAL_CNT"]?.ToString(),
                            TOTAL_REL = rdr["TOTAL_REL"]?.ToString(),
                            TOTAL_WRKLD = rdr["TOTL_WRKLD"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return Reports;
        }

        /// <summary>
        /// The pmat work reports.
        /// </summary>
        public class PMATWorkReports
        {
            /// <summary>
            /// Gets or sets the specialist_name.
            /// </summary>
            public string specialist_name { get; set; }

            /// <summary>
            /// Gets or sets the specialist_code.
            /// </summary>
            public string specialist_code { get; set; }

            /// <summary>
            /// Gets or sets the branch.
            /// </summary>
            public string branch { get; set; }

            /// <summary>
            /// Gets or sets the team.
            /// </summary>
            public string team { get; set; }

            /// <summary>
            /// Gets or sets the fy.
            /// </summary>
            public string fy { get; set; }

            /// <summary>
            /// Gets or sets the oc t_ cnt.
            /// </summary>
            public string OCT_CNT { get; set; }

            /// <summary>
            /// Gets or sets the oc t_ rel.
            /// </summary>
            public string OCT_REL { get; set; }

            /// <summary>
            /// Gets or sets the oc t_ wrkld.
            /// </summary>
            public string OCT_WRKLD { get; set; }

            /// <summary>
            /// Gets or sets the no v_ cnt.
            /// </summary>
            public string NOV_CNT { get; set; }

            /// <summary>
            /// Gets or sets the no v_ rel.
            /// </summary>
            public string NOV_REL { get; set; }

            /// <summary>
            /// Gets or sets the no v_ wrkld.
            /// </summary>
            public string NOV_WRKLD { get; set; }

            /// <summary>
            /// Gets or sets the de c_ cnt.
            /// </summary>
            public string DEC_CNT { get; set; }

            /// <summary>
            /// Gets or sets the de c_ rel.
            /// </summary>
            public string DEC_REL { get; set; }

            /// <summary>
            /// Gets or sets the de c_ wrkld.
            /// </summary>
            public string DEC_WRKLD { get; set; }

            /// <summary>
            /// Gets or sets the ja n_ cnt.
            /// </summary>
            public string JAN_CNT { get; set; }

            /// <summary>
            /// Gets or sets the ja n_ rel.
            /// </summary>
            public string JAN_REL { get; set; }

            /// <summary>
            /// Gets or sets the ja n_ wrkld.
            /// </summary>
            public string JAN_WRKLD { get; set; }

            /// <summary>
            /// Gets or sets the fe b_ cnt.
            /// </summary>
            public string FEB_CNT { get; set; }

            /// <summary>
            /// Gets or sets the fe b_ rel.
            /// </summary>
            public string FEB_REL { get; set; }

            /// <summary>
            /// Gets or sets the fe b_ wrkld.
            /// </summary>
            public string FEB_WRKLD { get; set; }

            /// <summary>
            /// Gets or sets the ma r_ cnt.
            /// </summary>
            public string MAR_CNT { get; set; }

            /// <summary>
            /// Gets or sets the ma r_ rel.
            /// </summary>
            public string MAR_REL { get; set; }

            /// <summary>
            /// Gets or sets the ma r_ wrkld.
            /// </summary>
            public string MAR_WRKLD { get; set; }

            /// <summary>
            /// Gets or sets the ap r_ cnt.
            /// </summary>
            public string APR_CNT { get; set; }

            /// <summary>
            /// Gets or sets the ap r_ rel.
            /// </summary>
            public string APR_REL { get; set; }

            /// <summary>
            /// Gets or sets the ap r_ wrkld.
            /// </summary>
            public string APR_WRKLD { get; set; }

            /// <summary>
            /// Gets or sets the ma y_ cnt.
            /// </summary>
            public string MAY_CNT { get; set; }

            /// <summary>
            /// Gets or sets the ma y_ rel.
            /// </summary>
            public string MAY_REL { get; set; }

            /// <summary>
            /// Gets or sets the ma y_ wrkld.
            /// </summary>
            public string MAY_WRKLD { get; set; }

            /// <summary>
            /// Gets or sets the ju n_ cnt.
            /// </summary>
            public string JUN_CNT { get; set; }

            /// <summary>
            /// Gets or sets the ju n_ rel.
            /// </summary>
            public string JUN_REL { get; set; }

            /// <summary>
            /// Gets or sets the ju n_ wrkld.
            /// </summary>
            public string JUN_WRKLD { get; set; }

            /// <summary>
            /// Gets or sets the ju l_ cnt.
            /// </summary>
            public string JUL_CNT { get; set; }

            /// <summary>
            /// Gets or sets the ju l_ rel.
            /// </summary>
            public string JUL_REL { get; set; }

            /// <summary>
            /// Gets or sets the ju l_ wrkld.
            /// </summary>
            public string JUL_WRKLD { get; set; }

            /// <summary>
            /// Gets or sets the au g_ cnt.
            /// </summary>
            public string AUG_CNT { get; set; }

            /// <summary>
            /// Gets or sets the au g_ rel.
            /// </summary>
            public string AUG_REL { get; set; }

            /// <summary>
            /// Gets or sets the au g_ wrkld.
            /// </summary>
            public string AUG_WRKLD { get; set; }

            /// <summary>
            /// Gets or sets the se p_ cnt.
            /// </summary>
            public string SEP_CNT { get; set; }

            /// <summary>
            /// Gets or sets the se p_ rel.
            /// </summary>
            public string SEP_REL { get; set; }

            /// <summary>
            /// Gets or sets the se p_ wrkld.
            /// </summary>
            public string SEP_WRKLD { get; set; }

            /// <summary>
            /// Gets or sets the tota l_ cnt.
            /// </summary>
            public string TOTAL_CNT { get; set; }

            /// <summary>
            /// Gets or sets the tota l_ rel.
            /// </summary>
            public string TOTAL_REL { get; set; }

            /// <summary>
            /// Gets or sets the tota l_ wrkld.
            /// </summary>
            public string TOTAL_WRKLD { get; set; }
        }
    }
}