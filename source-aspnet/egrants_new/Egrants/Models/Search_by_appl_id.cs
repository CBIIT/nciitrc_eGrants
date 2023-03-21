#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  Search_by_appl_id.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-12-02
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

#endregion

namespace egrants_new.Egrants.Models
{

    // load documents by appl_id
    /// <summary>
    ///     The search_by_appl_id.
    /// </summary>
    public class Search_by_appl_id
    {
        /// <summary>
        ///     Gets or sets the doclayerproperty.
        /// </summary>
        public static List<doclayer> doclayerproperty { get; set; }

        /// <summary>
        ///     Gets or sets the doclayerproperty_era.
        /// </summary>
        public static List<doclayer> doclayerproperty_era { get; set; }

        /// <summary>
        /// The load docs.
        /// </summary>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="search_type">
        /// The search_type.
        /// </param>
        /// <param name="category_list">
        /// The category_list.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        public static void LoadDocs(int appl_id, string search_type, string category_list, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_search_by_appl_id", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = appl_id;
            cmd.Parameters.Add("@search_type", SqlDbType.VarChar).Value = search_type;
            cmd.Parameters.Add("@category_list", SqlDbType.VarChar).Value = category_list;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

            // cmd.CommandTimeout = 120;
            conn.Open();

            doclayerproperty = null;
            var docList = new List<doclayer>();

            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                var doc = new doclayer();
                doc.grant_id = rdr["grant_id"]?.ToString();
                doc.appl_id = rdr["appl_id"]?.ToString();

                // doc.full_grant_num = rdr["full_grant_num"]?.ToString();
                doc.document_id = rdr["document_id"]?.ToString();
                doc.document_date = rdr["document_date"]?.ToString();
                doc.document_name = rdr["document_name"]?.ToString();

                if (rdr["doc_date"] != DBNull.Value)
                    doc.doc_date = Convert.ToDateTime(rdr["doc_date"]);

                doc.category_id = rdr["category_id"]?.ToString();
                doc.category_name = rdr["category_name"]?.ToString();
                doc.sub_category_name = rdr["sub_category_name"]?.ToString();
                doc.created_by = rdr["created_by"]?.ToString();
                doc.created_date = rdr["created_date"]?.ToString();
                doc.modified_by = rdr["modified_by"]?.ToString();
                doc.modified_date = rdr["modified_date"]?.ToString();
                doc.file_modified_by = rdr["file_modified_by"]?.ToString();
                doc.file_modified_date = rdr["file_modified_date"]?.ToString();
                doc.problem_msg = rdr["problem_msg"]?.ToString();
                doc.problem_reported_by = rdr["problem_reported_by"]?.ToString();
                doc.page_count = rdr["page_count"]?.ToString();
                doc.fsr_count = rdr["fsr_count"]?.ToString();
                doc.attachment_count = rdr["attachment_count"]?.ToString();
                doc.url = rdr["url"]?.ToString();
                doc.frc_destroyed = rdr["frc_destroyed"]?.ToString();
                doc.qc_date = rdr["qc_date"]?.ToString();
                doc.can_qc = rdr["can_qc"]?.ToString();
                doc.can_upload = rdr["can_upload"]?.ToString();
                doc.can_modify_index = rdr["can_modify_index"]?.ToString();
                doc.can_delete = rdr["can_delete"]?.ToString();
                doc.can_restore = rdr["can_restore"]?.ToString();
                doc.can_store = rdr["can_store"]?.ToString();

                // doc.ic = rdr["ic"]?.ToString();
                docList.Add(doc);
            }

            // added by Leon 5/11/2019
            conn.Close();
            doclayerproperty = docList;
        }
    }
}