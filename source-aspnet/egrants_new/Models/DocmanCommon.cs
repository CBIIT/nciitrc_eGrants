#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  DocmanCommon.cs
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

#endregion

namespace egrants_new.Models
{
    /// <summary>
    /// The docman common.
    /// </summary>
    public class DocmanCommon
    {
        /// <summary>
        /// The load docman contract.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="econ_id">
        /// The econ_id.
        /// </param>
        /// <param name="page_index">
        /// The page_index.
        /// </param>
        /// <param name="browser">
        /// The browser.
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
        public static List<DocmanContract> LoadDocmanContract(string str, int econ_id, int page_index, string browser, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_docman_search", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@str", SqlDbType.VarChar).Value = str;
            cmd.Parameters.Add("@econ_id", SqlDbType.Int).Value = econ_id;
            cmd.Parameters.Add("@page_index", SqlDbType.Int).Value = page_index;
            cmd.Parameters.Add("@browser", SqlDbType.VarChar).Value = browser;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

            var DocmanContract = new List<DocmanContract>();
            conn.Open();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                DocmanContract.Add(
                    new DocmanContract
                        {
                            econ_id = rdr["econ_id"]?.ToString(),
                            piid = rdr["piid"]?.ToString(),
                            ref_piid = rdr["ref_piid"]?.ToString(),
                            full_contract_number = rdr["full_contract_number"]?.ToString(),
                            award_mod_num = rdr["award_mod_num"]?.ToString(),
                            rfp_number = rdr["rfp_number"]?.ToString(),
                            institution = rdr["vendor_name"]?.ToString(),
                            project_enddate = rdr["project_enddate"]?.ToString(),
                            close_out = rdr["close_out"]?.ToString(),
                            person_id = rdr["person_id"]?.ToString(),
                            specialist_name = rdr["specialist_name"]?.ToString(),
                            team_leader = rdr["team_leader"]?.ToString(),
                            branch_chief = rdr["branch_chief"]?.ToString(),
                            can_upload = rdr["can_upload"]?.ToString(),
                            document_count = rdr["document_count"]?.ToString(),
                            page_count = rdr["page_count"]?.ToString()
                        });

            conn.Close();

            return DocmanContract;
        }

        // load Docman contract for CFT project only
        /// <summary>
        /// The load additional contract.
        /// </summary>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<DocmanContract> LoadAdditionalContract(int serial_num)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT contract_number,fiscal_year,dbo.fn_clean_characters(institution) AS institution,"
              + " contract_type, activity_code, upper(activity_code) + '-' + upper(contract_type) + '-' + CONVERT(varchar, right(contract_number, 5)) as full_contract_number,"
              + " piid, rfp, specialist_name, upper(close_out) AS close_out, combined_piid FROM dbo.vw_CFT_I2EContracts "
              + " WHERE piid is not null and contract_number = @serial_num",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@serial_num", SqlDbType.Int).Value = serial_num;

            var DocmanContracts = new List<DocmanContract>();
            conn.Open();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                DocmanContracts.Add(
                    new DocmanContract
                        {
                            piid = rdr["piid"]?.ToString(),
                            combined_piid = rdr["combined_piid"]?.ToString(),
                            specialist_name = rdr["specialist_name"]?.ToString(),
                            full_contract_number = rdr["full_contract_number"]?.ToString(),
                            institution = rdr["institution"]?.ToString(),
                            close_out = rdr["close_out"]?.ToString(),
                            fiscal_year = rdr["fiscal_year"]?.ToString()
                        });

            conn.Close();

            return DocmanContracts;
        }

        /// <summary>
        /// The load main categories.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<MainCategories> LoadMainCategories()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);

            var cmd = new SqlCommand(
                "select mcat_id, mcat_name, label, sort_order from dbo.DocMan_Main_categories where End_date is null ORDER BY mcat_id",
                conn);

            cmd.CommandType = CommandType.Text;

            var MainCategorieList = new List<MainCategories>();
            conn.Open();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                MainCategorieList.Add(
                    new MainCategories
                        {
                            mcat_id = rdr["mcat_id"]?.ToString(),
                            mcat_name = rdr["mcat_name"]?.ToString(),
                            mcat_label = rdr["label"]?.ToString(),
                            mcat_sort_order = rdr["sort_order"]?.ToString()
                        });

            conn.Close();

            return MainCategorieList;
        }

        /// <summary>
        /// The get m cat total.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int GetMCatTotal()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            var cmd = new SqlCommand("select count(*) as mcat_total from dbo.DocMan_Main_categories where end_date is null", conn);
            cmd.CommandType = CommandType.Text;
            var MCateTotal = 0;
            conn.Open();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                MCateTotal = Convert.ToInt16(rdr["mcat_total"]);

            conn.Close();

            return MCateTotal;
        }

        /// <summary>
        /// The load sub categories.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<SubCategories> LoadSubCategories()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);

            var cmd = new SqlCommand(
                "select Mcat_id, Ccat_id, Ccat_name,label, sort_order from dbo.DocMan_Child_categories where end_dt is null order by Mcat_id, Ccat_id",
                conn);

            cmd.CommandType = CommandType.Text;

            var SubCategorieList = new List<SubCategories>();
            conn.Open();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                SubCategorieList.Add(
                    new SubCategories
                        {
                            mcat_id = rdr["mcat_id"]?.ToString(),
                            ccat_id = rdr["ccat_id"]?.ToString(),
                            ccat_name = rdr["ccat_name"]?.ToString(),
                            ccat_label = rdr["label"]?.ToString(),
                            ccat_sort_order = rdr["sort_order"]?.ToString()
                        });

            conn.Close();

            return SubCategorieList;
        }

        /// <summary>
        /// The getd c cat total.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int GetdCCatTotal()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            var cmd = new SqlCommand("select count(*) as ccat_total from dbo.DocMan_Child_Categories where End_dt is null", conn);
            cmd.CommandType = CommandType.Text;
            var CCateTotal = 0;
            conn.Open();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                CCateTotal = Convert.ToInt16(rdr["ccat_total"]);

            conn.Close();

            return CCateTotal;
        }

        /// <summary>
        /// The load accessions.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<DocmantAccessions> LoadAccessions()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            var cmd = new SqlCommand("SELECT distinct accession_id,accession_number from vw_boxes order by accession_id desc", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var Accessions = new List<DocmantAccessions>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Accessions.Add(
                    new DocmantAccessions { accession_id = rdr["accession_id"].ToString(), accession_number = rdr["accession_number"].ToString() });

            conn.Close();

            return Accessions;
        }

        /// <summary>
        /// The load coordinators.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<DocmanUsers> LoadCoordinators()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            var cmd = new SqlCommand("select person_id, person_name from people where is_coordinator=1 and active=1 order by person_name", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var Coordinators = new List<DocmanUsers>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Coordinators.Add(new DocmanUsers { person_id = rdr["person_id"]?.ToString(), person_name = rdr["person_name"]?.ToString() });

            conn.Close();

            return Coordinators;
        }

        /// <summary>
        /// The load docman users.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<DocmanUsers> LoadDocmanUsers(string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);

            var cmd = new SqlCommand(
                "select person_id, person_name from vw_people "
              + "where ic = @ic and application_type = 'econtracts' and (person_name is not null and person_name <> '') order by person_name",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            conn.Open();

            var DocmanUsers = new List<DocmanUsers>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                DocmanUsers.Add(new DocmanUsers { person_id = rdr["person_id"]?.ToString(), person_name = rdr["person_name"]?.ToString() });

            conn.Close();

            return DocmanUsers;
        }

        // public class User
        // {
        // public int personID { get; set; }
        // public int positionID { get; set; }
        // public string PersonName { get; set; }
        // public string UserId { get; set; }
        // public string PersonEmail { get; set; }
        // public string Validation { get; set; }
        // public string menulist { get; set; }
        // public string ic { get; set; }
        // }

        // public class SMVariable
        // {
        // public string loginid { get; set; }
        // public string ic { get; set; }
        // }

        // public static IEnumerable<User> uservar(string userid, string ic)
        // {

        // string connectionString = ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString;
        // List<User> users = new List<User>();
        // using (SqlConnection conn = new SqlConnection(connectionString))
        // {
        // SqlCommand cmd = new SqlCommand("sp_web_docman_user_detail", conn);
        // cmd.CommandType = CommandType.StoredProcedure;
        // cmd.Parameters.AddWithValue("@ic", ic);
        // cmd.Parameters.AddWithValue("@usrid", userid);

        // conn.Open();
        // User user = new User();
        // using (SqlDataReader rdr = cmd.ExecuteReader())
        // {
        // while (rdr.Read())
        // {
        // if (rdr["nam"].ToString() == "VALIDATION")
        // user.Validation = rdr["val"].ToString();
        // if (rdr["nam"].ToString() == "USERID")
        // user.UserId = rdr["val"].ToString();
        // if (rdr["nam"].ToString() == "IC")
        // user.ic = rdr["val"].ToString();
        // if (rdr["nam"].ToString() == "PERSONID")
        // user.personID = Convert.ToInt32(rdr["val"]);
        // if (rdr["nam"].ToString() == "POSITIONID")
        // user.positionID = Convert.ToInt32(rdr["val"]);
        // if (rdr["nam"].ToString() == "USERNAME")
        // user.PersonName = rdr["val"].ToString();
        // if (rdr["nam"].ToString() == "USEREMAIL")
        // user.PersonEmail = rdr["val"].ToString();
        // if (rdr["nam"].ToString() == "MENULIST")
        // user.menulist = rdr["val"].ToString();
        // }
        // }
        // users.Add(user);
        // return users;
        // }
        // }

        /// <summary>
        /// The docman contract.
        /// </summary>
        public class DocmanContract
        {
            /// <summary>
            /// Gets or sets the econ_id.
            /// </summary>
            public string econ_id { get; set; }

            /// <summary>
            /// Gets or sets the piid.
            /// </summary>
            public string piid { get; set; }

            /// <summary>
            /// Gets or sets the ref_piid.
            /// </summary>
            public string ref_piid { get; set; }

            /// <summary>
            /// Gets or sets the combined_piid.
            /// </summary>
            public string combined_piid { get; set; }

            /// <summary>
            /// Gets or sets the award_mod_num.
            /// </summary>
            public string award_mod_num { get; set; }

            /// <summary>
            /// Gets or sets the rfp_number.
            /// </summary>
            public string rfp_number { get; set; }

            /// <summary>
            /// Gets or sets the full_contract_number.
            /// </summary>
            public string full_contract_number { get; set; }

            /// <summary>
            /// Gets or sets the person_id.
            /// </summary>
            public string person_id { get; set; }

            /// <summary>
            /// Gets or sets the specialist_id.
            /// </summary>
            public string specialist_id { get; set; }

            /// <summary>
            /// Gets or sets the specialist_name.
            /// </summary>
            public string specialist_name { get; set; }

            /// <summary>
            /// Gets or sets the team_leader.
            /// </summary>
            public string team_leader { get; set; }

            /// <summary>
            /// Gets or sets the branch_chief.
            /// </summary>
            public string branch_chief { get; set; }

            /// <summary>
            /// Gets or sets the institution.
            /// </summary>
            public string institution { get; set; }

            /// <summary>
            /// Gets or sets the project_enddate.
            /// </summary>
            public string project_enddate { get; set; }

            /// <summary>
            /// Gets or sets the project_startdate.
            /// </summary>
            public string project_startdate { get; set; }

            /// <summary>
            /// Gets or sets the created_date.
            /// </summary>
            public string created_date { get; set; }

            /// <summary>
            /// Gets or sets the close_out.
            /// </summary>
            public string close_out { get; set; }

            /// <summary>
            /// Gets or sets the fiscal_year.
            /// </summary>
            public string fiscal_year { get; set; }

            /// <summary>
            /// Gets or sets the can_upload.
            /// </summary>
            public string can_upload { get; set; }

            /// <summary>
            /// Gets or sets the document_count.
            /// </summary>
            public string document_count { get; set; }

            /// <summary>
            /// Gets or sets the page_count.
            /// </summary>
            public string page_count { get; set; }
        }

        /// <summary>
        /// The main categories.
        /// </summary>
        public class MainCategories
        {
            /// <summary>
            /// Gets or sets the mcat_id.
            /// </summary>
            public string mcat_id { get; set; }

            /// <summary>
            /// Gets or sets the mcat_name.
            /// </summary>
            public string mcat_name { get; set; }

            /// <summary>
            /// Gets or sets the mcat_sort_order.
            /// </summary>
            public string mcat_sort_order { get; set; }

            /// <summary>
            /// Gets or sets the mcat_label.
            /// </summary>
            public string mcat_label { get; set; }
        }

        /// <summary>
        /// The sub categories.
        /// </summary>
        public class SubCategories
        {
            /// <summary>
            /// Gets or sets the mcat_id.
            /// </summary>
            public string mcat_id { get; set; }

            /// <summary>
            /// Gets or sets the ccat_id.
            /// </summary>
            public string ccat_id { get; set; }

            /// <summary>
            /// Gets or sets the ccat_name.
            /// </summary>
            public string ccat_name { get; set; }

            /// <summary>
            /// Gets or sets the ccat_sort_order.
            /// </summary>
            public string ccat_sort_order { get; set; }

            /// <summary>
            /// Gets or sets the ccat_label.
            /// </summary>
            public string ccat_label { get; set; }
        }

        /// <summary>
        /// The docman deleted documents.
        /// </summary>
        public class DocmanDeletedDocuments
        {
            /// <summary>
            /// Gets or sets the docment_id.
            /// </summary>
            public string docment_id { get; set; }

            /// <summary>
            /// Gets or sets the econ_id.
            /// </summary>
            public string econ_id { get; set; }

            /// <summary>
            /// Gets or sets the piid.
            /// </summary>
            public string piid { get; set; }

            /// <summary>
            /// Gets or sets the ref_piid.
            /// </summary>
            public string ref_piid { get; set; }

            /// <summary>
            /// Gets or sets the ccat_name.
            /// </summary>
            public string ccat_name { get; set; }

            /// <summary>
            /// Gets or sets the url_loc.
            /// </summary>
            public string url_loc { get; set; }

            /// <summary>
            /// Gets or sets the report_label.
            /// </summary>
            public string report_label { get; set; }

            /// <summary>
            /// Gets or sets the disabled_date.
            /// </summary>
            public string disabled_date { get; set; }

            /// <summary>
            /// Gets or sets the disabled_reason.
            /// </summary>
            public string disabled_reason { get; set; }

            /// <summary>
            /// Gets or sets the disabled_by.
            /// </summary>
            public string disabled_by { get; set; }
        }

        /// <summary>
        /// The docmant accessions.
        /// </summary>
        public class DocmantAccessions
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
        /// The docman users.
        /// </summary>
        public class DocmanUsers
        {
            /// <summary>
            /// Gets or sets the character_index.
            /// </summary>
            public string character_index { get; set; }

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
            /// Gets or sets the active.
            /// </summary>
            public string active { get; set; }

            /// <summary>
            /// Gets or sets the ic.
            /// </summary>
            public string ic { get; set; }

            /// <summary>
            /// Gets or sets the application_type.
            /// </summary>
            public string application_type { get; set; }

            /// <summary>
            /// Gets or sets the is_coordinator.
            /// </summary>
            public string is_coordinator { get; set; }

            /// <summary>
            /// Gets or sets the coordinator_id.
            /// </summary>
            public string coordinator_id { get; set; }

            /// <summary>
            /// Gets or sets the can_admin.
            /// </summary>
            public string can_admin { get; set; }

            /// <summary>
            /// Gets or sets the can_egrants.
            /// </summary>
            public string can_egrants { get; set; }

            /// <summary>
            /// Gets or sets the can_docman.
            /// </summary>
            public string can_docman { get; set; }

            /// <summary>
            /// Gets or sets the can_cft.
            /// </summary>
            public string can_cft { get; set; }

            /// <summary>
            /// Gets or sets the start_date.
            /// </summary>
            public string start_date { get; set; }

            /// <summary>
            /// Gets or sets the end_date.
            /// </summary>
            public string end_date { get; set; }

            /// <summary>
            /// Gets or sets the create_date.
            /// </summary>
            public string create_date { get; set; }

            /// <summary>
            /// Gets or sets the create_by.
            /// </summary>
            public string create_by { get; set; }

            /// <summary>
            /// Gets or sets the last_update_date.
            /// </summary>
            public string last_update_date { get; set; }

            /// <summary>
            /// Gets or sets the last_update_by.
            /// </summary>
            public string last_update_by { get; set; }
        }

        // public class CFTContract
        // {
        // public string contract_id { get; set; }
        // public string contract_number { get; set; }
        // public string contract_type { get; set; }
        // public string activity_code { get; set; }
        // public string full_contract_number { get; set; }
        // public string piid { get; set; }
        // public string fiscal_year { get; set; }
        // public string institution { get; set; }
        // public string rfp { get; set; }
        // public string specialist_name { get; set; }
        // public string close_out { get; set; }
        // }

        /// <summary>
        /// The docmant boxes.
        /// </summary>
        public class DocmantBoxes
        {
            /// <summary>
            /// Gets or sets the accession_id.
            /// </summary>
            public string accession_id { get; set; }

            /// <summary>
            /// Gets or sets the box_id.
            /// </summary>
            public string box_id { get; set; }

            /// <summary>
            /// Gets or sets the box_number.
            /// </summary>
            public string box_number { get; set; }

            /// <summary>
            /// Gets or sets the box.
            /// </summary>
            public string box { get; set; }
        }
    }
}