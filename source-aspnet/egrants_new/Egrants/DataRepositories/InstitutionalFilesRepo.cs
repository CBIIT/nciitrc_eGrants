#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  InstitutionalFilesRepo.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-05-17
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
using System.Runtime.InteropServices;

#endregion

namespace egrants_new.Egrants.Models
{
    /// <summary>
    /// The institutional files repo.
    /// </summary>
    public class InstitutionalFilesRepo : IDisposable
    {
        /// <summary>
        /// The conn.
        /// </summary>
        private readonly SqlConnection conn;

        // private System.Data.SqlClient.SqlCommand cmd;
        /// <summary>
        /// Initializes a new instance of the <see cref="InstitutionalFilesRepo"/> class.
        /// </summary>
        public InstitutionalFilesRepo()
        {
            this.conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
        }

        // to load Character index for intitution name 
        /// <summary>
        /// The load org name character indices.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<InsitutionalOrgNameIndex> LoadOrgNameCharacterIndices()
        {
            var cmd = new SqlCommand("SELECT index_id, character_index, index_seq from dbo.character_index order by index_seq", this.conn);
            cmd.CommandType = CommandType.Text;
            this.conn.Open();

            var OrgNameCharacterIndexs = new List<InsitutionalOrgNameIndex>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                OrgNameCharacterIndexs.Add(
                    new InsitutionalOrgNameIndex
                        {
                            IndexId = rdr["index_id"]?.ToString(),
                            CharacterIndex = rdr["character_index"]?.ToString(),
                            IndexSeq = rdr["index_seq"]?.ToString()
                        });

            this.conn.Close();

            return OrgNameCharacterIndexs;
        }

        // to load all intitution list
        // public List<InstitutionalOrg> LoadOrgList(InstitutionalFilesPageAction action, string str, int index_id, int org_id, int doc_id, int category_id, string file_type, string start_date, string end_date, string ic, string userid)
        /// <summary>
        /// The load org list.
        /// </summary>
        /// <param name="index_id">
        /// The index_id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<InstitutionalOrg> LoadOrgList(int index_id)
        {
            var cmd = new SqlCommand("sp_web_egrants_inst_files_show_orgs", this.conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@index_id", SqlDbType.Int).Value = index_id;

            this.conn.Open();

            var OrgList = new List<InstitutionalOrg>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                OrgList.Add(
                    new InstitutionalOrg
                        {
                            OrgId = (int)rdr["org_id"],
                            OrgName = rdr["org_name"]?.ToString(),
                            SVCreatedBy = rdr["svcreated_by"]?.ToString(),
                            SVCreatedDate = rdr["svcreated_date"]?.ToString(),
                            SVEndDate = rdr["svend_date"]?.ToString(),
                            SvUrl = rdr["sv_url"]?.ToString(),
                            FUCreatedBy = rdr["fucreated_by"]?.ToString(),
                            FUCreatedDate = rdr["fucreated_date"]?.ToString(),
                            FUEndDate = rdr["fuend_date"]?.ToString(),
                            FUUrl = rdr["fu_url"]?.ToString(),
                            AnyOrgDoc = (bool)rdr["anyorgdoc"]
                        });

            this.conn.Close();

            return OrgList;
        }

        // sp_web_egrants_inst_files_search_orgs
        /// <summary>
        /// The search org list.
        /// </summary>
        /// <param name="search_str">
        /// The search_str.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<InstitutionalOrg> SearchOrgList(string search_str)
        {
            var cmd = new SqlCommand("sp_web_egrants_inst_files_search_orgs", this.conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@str", SqlDbType.VarChar).Value = search_str;

            this.conn.Open();

            var OrgList = new List<InstitutionalOrg>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                OrgList.Add(
                    new InstitutionalOrg
                        {
                            OrgId = (int)rdr["org_id"],
                            OrgName = rdr["org_name"]?.ToString(),
                            SVCreatedBy = rdr["svcreated_by"]?.ToString(),
                            SVCreatedDate = rdr["svcreated_date"]?.ToString(),
                            SVEndDate = rdr["svend_date"]?.ToString(),
                            SvUrl = rdr["sv_url"]?.ToString(),
                            FUCreatedBy = rdr["fucreated_by"]?.ToString(),
                            FUCreatedDate = rdr["fucreated_date"]?.ToString(),
                            FUEndDate = rdr["fuend_date"]?.ToString(),
                            FUUrl = rdr["fu_url"]?.ToString(),
                            AnyOrgDoc = (bool)rdr["anyorgdoc"]
                        });

            this.conn.Close();

            return OrgList;
        }

        /// <summary>
        /// The find org.
        /// </summary>
        /// <param name="org_id">
        /// The org_id.
        /// </param>
        /// <param name="org_name">
        /// The org_name.
        /// </param>
        /// <returns>
        /// The <see cref="InstitutionalOrg"/>.
        /// </returns>
        public InstitutionalOrg FindOrg(int org_id, string org_name = "")
        {
            var cmd = new SqlCommand("sp_web_egrants_institutional_file_find_org", this.conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // TODO:  This should branched to different Stored procedures based on the revision MAdhu does
            cmd.Parameters.Add("@org_id", SqlDbType.Int).Value = org_id;
            cmd.Parameters.Add("@org_name", SqlDbType.VarChar).Value = org_name;

            this.conn.Open();

            var Org = new InstitutionalOrg();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Org = new InstitutionalOrg
                          {
                              OrgId = (int)rdr["org_id"],
                              OrgName = rdr["org_name"]?.ToString(),
                              SVCreatedBy = rdr["created_by"]?.ToString(),
                              SVCreatedDate = rdr["created_date"]?.ToString(),
                              SVEndDate = rdr["end_date"]?.ToString(),
                              SvUrl = rdr["sv_url"]?.ToString()
                          };

            this.conn.Close();

            return Org;
        }

        // to load all intitutional files list   -   [sp_web_egrants_institutional_file_find_org]
        /// <summary>
        /// The load org doc list.
        /// </summary>
        /// <param name="org_id">
        /// The org_id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<InstitutionalDocFiles> LoadOrgDocList(int org_id)
        {
            var cmd = new SqlCommand("sp_web_egrants_inst_files_show_docs", this.conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@org_id", SqlDbType.Int).Value = org_id;

            this.conn.Open();

            var DocFilesList = new List<InstitutionalDocFiles>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                DocFilesList.Add(
                    new InstitutionalDocFiles
                        {
                            org_id = rdr["org_id"]?.ToString(),
                            org_name = rdr["org_name"]?.ToString(),
                            DocumentId = (int)rdr["document_id"],
                            category_name = rdr["category_name"]?.ToString(),
                            url = rdr["url"]?.ToString(),
                            start_date = rdr["start_date"]?.ToString(),
                            end_date = rdr["end_date"]?.ToString(),
                            created_date = rdr["created_date"]?.ToString(),
                            comments = rdr["comments"]?.ToString()
                        });

            this.conn.Close();

            return DocFilesList;
        }

        // to turn all categories for institutional file
        /// <summary>
        /// The load org category.
        /// </summary>
        /// <param name="activeOnly">
        /// The active only.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<InstitutionalOrgCategory> LoadOrgCategory(bool activeOnly)
        {
            var where = activeOnly ? "Where active = 1" : string.Empty;

            var cmd = new SqlCommand(
                $"SELECT doctype_id AS category_id, doctype_name AS category_name, tobe_flagged AS tobe_flag, Flag_period, isnull(comments_required,0) as comments_required, active FROM dbo.Org_Categories {where} ORDER BY category_name ",
                this.conn);

            cmd.CommandType = CommandType.Text;

            this.conn.Open();

            var OrgCategoryList = new List<InstitutionalOrgCategory>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                OrgCategoryList.Add(
                    new InstitutionalOrgCategory
                        {
                            category_id = rdr["category_id"]?.ToString(),
                            category_name = rdr["category_name"]?.ToString(),
                            tobe_flag = rdr["tobe_flag"]?.ToString(),
                            flag_period = rdr["Flag_period"]?.ToString(),
                            flag_data = rdr["tobe_flag"] + "_" + rdr["Flag_period"],
                            require_comments = (bool)rdr["comments_required"],
                            active = (bool)rdr["active"]
                        });

            this.conn.Close();

            return OrgCategoryList;
        }

        // to disable an institutional file
        /// <summary>
        /// The disable doc.
        /// </summary>
        /// <param name="doc_id">
        /// The doc_id.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        public void DisableDoc(int doc_id, string userid)
        {
            var cmd = new SqlCommand("sp_web_egrants_inst_files_disable_doc", this.conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = doc_id;
            cmd.Parameters.Add("@user_id", SqlDbType.VarChar).Value = userid;

            this.conn.Open();

            cmd.ExecuteNonQuery();

            this.conn.Close();
        }

        // to create new institutional file and return document_id
        /// <summary>
        /// The get doc id.
        /// </summary>
        /// <param name="org_id">
        /// The org_id.
        /// </param>
        /// <param name="category_id">
        /// The category_id.
        /// </param>
        /// <param name="file_type">
        /// The file_type.
        /// </param>
        /// <param name="start_date">
        /// The start_date.
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
        /// <param name="comments">
        /// The comments.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetDocID(
            int org_id,
            int category_id,
            string file_type,
            string start_date,
            string end_date,
            string ic,
            string userid,
            string comments)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_institutional_file_create", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@org_id", SqlDbType.Int).Value = org_id;
            cmd.Parameters.Add("@category_id", SqlDbType.Int).Value = category_id;
            cmd.Parameters.Add("@file_type", SqlDbType.VarChar).Value = file_type;
            cmd.Parameters.Add("@start_date", SqlDbType.VarChar).Value = start_date;
            cmd.Parameters.Add("@end_date", SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@document_id", SqlDbType.VarChar, 100);
            cmd.Parameters.Add("@comments", SqlDbType.VarChar).Value = comments;
            cmd.Parameters["@document_id"].Direction = ParameterDirection.Output;
            conn.Open();

            var DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();

            var document_id = Convert.ToString(cmd.Parameters["@document_id"].Value);

            return document_id;
        }

        /// <summary>
        /// The update document.
        /// </summary>
        /// <param name="doc_id">
        /// The doc_id.
        /// </param>
        /// <param name="category_id">
        /// The category_id.
        /// </param>
        /// <param name="start_date">
        /// The start_date.
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
        /// <param name="comments">
        /// The comments.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string UpdateDocument(int doc_id, int category_id, string start_date, string end_date, string ic, string userid, string comments)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_institutional_file_update", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@category_id", SqlDbType.Int).Value = category_id;
            cmd.Parameters.Add("@start_date", SqlDbType.VarChar).Value = start_date;
            cmd.Parameters.Add("@end_date", SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@document_id", SqlDbType.Int).Value = doc_id;
            cmd.Parameters.Add("@comments", SqlDbType.VarChar).Value = comments;
            conn.Open();

            cmd.ExecuteNonQuery();

            conn.Close();

            var document_id = Convert.ToString(cmd.Parameters["@document_id"].Value);

            return document_id;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (conn != null)
                {
                    conn.Dispose();
                }
            }
        }
    }
}