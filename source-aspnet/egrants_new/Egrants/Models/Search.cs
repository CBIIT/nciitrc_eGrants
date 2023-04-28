#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  Search.cs
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
using System.Linq;

#endregion

namespace egrants_new.Egrants.Models
{
    /// <summary>
    ///     The search.
    /// </summary>
    public static class Search
    {
        /// <summary>
        ///     Gets or sets the GrantlayerList.
        /// </summary>
        public static List<GrantLayer> GrantlayerList { get; set; }

        /// <summary>
        ///     Gets or sets the appllayerproperty.
        /// </summary>
        public static List<ApplLayerObject> appllayerproperty { get; set; }

        /// <summary>
        ///     Gets or sets the doclayerproperty.
        /// </summary>
        public static List<doclayer> doclayerproperty { get; set; }

        /// <summary>
        ///     Gets or sets the doclayerproperty_era.
        /// </summary>
        public static List<doclayer> doclayerproperty_era { get; set; }

        /// <summary>
        /// The egrants_search.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="current_page">
        /// The current_page.
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
        public static void egrants_search(
            string str,
            int grant_id,
            string package,
            int appl_id,
            int current_page,
            string browser,
            string ic,
            string userid)
        {
            bool isGrant = false;
            bool isStr = false;
            bool isAppl = false;
            if (grant_id != 0)
            {
                isGrant = true;
            }

            if (!string.IsNullOrEmpty(str))
            {
                isStr = true;
            }

            if (appl_id != 0)
            {
                isAppl = true;
            }

            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("dbo.sp_web_egrants", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@str", SqlDbType.NVarChar).Value = str;
            cmd.Parameters.Add("@grant_id", SqlDbType.Int).Value = grant_id;
            cmd.Parameters.Add("@package", SqlDbType.VarChar).Value = package;
            cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = appl_id;
            cmd.Parameters.Add("@current_page", SqlDbType.Int).Value = current_page;
            cmd.Parameters.Add("@browser", SqlDbType.VarChar).Value = browser;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

            conn.Open();

            GrantlayerList = null;
            appllayerproperty = null;
            doclayerproperty = null;

            var grantList = new List<GrantLayer>();
            var applList = new List<ApplLayerObject>();
            var docList = new List<doclayer>();

            var rdr = cmd.ExecuteReader();

            var tag = 0;

            while (rdr.Read())
            {
                tag = Convert.ToInt32(rdr["tag"]);

                if (tag == 1)
                {
                    var grant = new GrantLayer();
                    grant.grant_id = rdr["grant_id"]?.ToString();
                    grant.org_name = rdr["org_name"]?.ToString();
                    grant.serial_num = rdr["serial_num"]?.ToString();
                    grant.grant_num = string.Concat(rdr["admin_phs_org_code"] + Convert.ToInt32(rdr["serial_num"]).ToString("000000"));
                    grant.former_grant_num = rdr["former_grant_num"]?.ToString();
                    grant.latest_full_grant_num = rdr["latest_full_grant_num"]?.ToString();
                    grant.admin_phs_org_code = rdr["admin_phs_org_code"]?.ToString();
                    grant.project_title = rdr["project_title"]?.ToString();
                    grant.pi_name = rdr["pi_name"]?.ToString();
                    grant.prog_class_code = rdr["prog_class_code"]?.ToString();
                    grant.all_activity_code = rdr["all_activity_code"]?.ToString();
                    grant.current_pi_name = rdr["current_pi_name"]?.ToString();
                    grant.current_pi_email_address = rdr["current_pi_email_address"]?.ToString();
                    grant.current_pd_name = rdr["current_pd_name"]?.ToString();
                    grant.current_pd_email_address = rdr["current_pd_email_address"]?.ToString();
                    grant.current_spec_name = rdr["current_spec_name"]?.ToString();
                    grant.current_spec_email_address = rdr["current_spec_email_address"]?.ToString();
                    grant.current_bo_email_address = rdr["current_bo_email_address"]?.ToString();
                    grant.sv_url = rdr["sv_url"]?.ToString();
                    grant.arra_flag = rdr["arra_flag"]?.ToString();
                    grant.fda_flag = rdr["fda_flag"]?.ToString();
                    grant.stop_flag = rdr["stop_flag"]?.ToString();
                    grant.ms_flag = rdr["ms_flag"]?.ToString();
                    grant.od_flag = rdr["od_flag"]?.ToString();
                    grant.ds_flag = rdr["ds_flag"]?.ToString();
                    grant.adm_supp = rdr["adm_supp"]?.ToString();
                    if (appl_id <= 0)
                        grant.institutional_flag1 = rdr["institutional_flag1"].ToString() == "1" ? true : false;
                    else
                        grant.institutional_flag1 = rdr["specific_year_institution1"].ToString() == "1" ? true : false;
                    if (appl_id <= 0)
                        grant.AnyOrgDoc = rdr["institutional_flag2"].ToString() == "1" ? true : false;
                    else
                        grant.AnyOrgDoc = rdr["specific_year_institution2"].ToString() == "1" ? true : false;
                    grant.inst_flag1_url = rdr["inst_flag1_url"].ToString();
                    grant.IsCurrentPi = rdr["is_current_pi"]?.ToString() == "1" ? true : false;
                    grant.SelectedGrantPiName = rdr["specific_year_pi_name"].ToString();
                    grant.SelectedGrantPiEmail = rdr["specific_year_pi_email_address"].ToString();
                    grant.SelectedProjectName = rdr["specific_year_project_name"].ToString();
                    grant.SelectedOrganizationName = rdr["specific_year_org_name"].ToString();
                    if (string.IsNullOrWhiteSpace(grant.SelectedOrganizationName))
                        grant.SelectedOrganizationName = grant.org_name;
                    grantList.Add(grant);
                }
                else if (tag == 2)
                {
                    var appl = new ApplLayerObject();
                    appl.grant_id = rdr["grant_id"]?.ToString();
                    appl.appl_id = rdr["appl_id"]?.ToString();
                    appl.appl_type_code = rdr["appl_type_code"]?.ToString();
                    appl.full_grant_num = rdr["full_grant_num"]?.ToString();
                    appl.support_year = rdr["support_year"]?.ToString();
                    appl.deleted_by_impac = rdr["deleted_by_impac"]?.ToString();
                    appl.doc_count = rdr["doc_count"]?.ToString();
                    appl.closeout_notcount = rdr["closeout_notcount"]?.ToString();
                    appl.can_add_doc = rdr["can_add_doc"]?.ToString();
                    appl.competing = rdr["competing"]?.ToString();
                    appl.fsr_count = rdr["fsr_count"]?.ToString();
                    appl.frc_destroyed = rdr["frc_destroyed"]?.ToString();
                    appl.appl_fda_flag = rdr["appl_fda_flag"]?.ToString();
                    appl.appl_ms_flag = rdr["appl_ms_flag"]?.ToString();
                    appl.appl_od_flag = rdr["appl_od_flag"]?.ToString();
                    appl.appl_ds_flag = rdr["appl_ds_flag"]?.ToString();
                    appl.closeout_flag = rdr["closeout_flag"]?.ToString();
                    appl.irppr_id = rdr["irppr_id"]?.ToString();
                    appl.can_add_funding = rdr["can_add_funding"]?.ToString();

                    applList.Add(appl);
                }
                else if (tag == 3)
                {
                    var doc = new doclayer();
                    doc.appl_id = rdr["appl_id"]?.ToString();
                    doc.docs_count = rdr["docs_count"]?.ToString();

                    docList.Add(doc);
                }
            }

            // added by Leon 5/11/2019
            conn.Close();

            if (isGrant || isStr)
            {
                PopulateGrantAndStringViews(true, grantList, applList);
            }

            // every appl with > 1 person from IRDB will be in the response
            var mpi_info = GetAllMPIInfo(applList.Select(al => al.appl_id).ToList());
            PopulateMPIIntoGrants(grantList, applList, mpi_info);

            GrantlayerList = grantList;
            appllayerproperty = applList;
            doclayerproperty = docList;
        }

        private static void PopulateGrantAndStringViews(bool isGrant, List<GrantLayer> grantList, List<ApplLayerObject> applList)
        {

            if (isGrant)
            {
                string project_title = string.Empty;
                string org_name = string.Empty;
                string first_name = string.Empty;
                string last_name = string.Empty;

                foreach (var grant in grantList)
                {
                    foreach (var appl in applList)
                    {
                        if (grant.latest_full_grant_num == appl.full_grant_num) // && appl.appl_type_code.Contains(new int[] {1,2,5,7,9}))
                        {
                            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString))
                            {
                                var cmd = new SqlCommand("SELECT a.project_title as project_title, a.org_name as org_name, a.first_name,a.last_name"
                                                       + " FROM dbo.vw_appls as a"
                                                       + " WHERE a.appl_id = @appl_id",
                                    conn);



                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Add("@appl_id", SqlDbType.VarChar).Value = appl.appl_id;
                                conn.Open();

                                var list = grantList;
                                var sqlDataReader = cmd.ExecuteReader();

                                while (sqlDataReader.Read())
                                {
                                    grant.SelectedProjectName = sqlDataReader["project_title"]?.ToString();
                                    grant.SelectedOrganizationName = sqlDataReader["org_name"]?.ToString();
                                    grant.SelectedGrantPiName
                                        = sqlDataReader["first_name"]?.ToString() + " " + sqlDataReader["last_name"]?.ToString();
                                }
                            }
                        }
                    }
                }
            }
        }

        public static Dictionary<string, List<PersonContact>> GetAllMPIInfo(List<string> appl_ids)
        {
            var results = new Dictionary<string, List<PersonContact>>();

            if (appl_ids == null || appl_ids.Count == 0)
                return results;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                // note that Ingrid learned retrieving email interferes with the ability of the query to return all the MPIs
                var sql = "DECLARE @TSQL varchar(8000);" +
                    "SELECT @TSQL = 'SELECT APPL_ID, First_Name, Last_name, Role_Type_Code  FROM OPENQUERY(IRDB,''select e.appl_id, d.person_id, d.first_name, d.last_name, d.mi_name src_mi_name, c.email_addr , e.role_type_code, c.addr_type_code from person_involvements_mv e join persons_secure d on d.person_id = e.person_id left outer join person_addresses_mv c on d.person_id = c.person_id and c.addr_type_code in (''''HOM'''') and c.preferred_addr_code = ''''Y'''' where e.role_type_code in (''''PI'''', ''''MPI'''',''''CPI'''') and appl_id in ( INSERT_APPL_IDs_HERE) and d.person_id = e.person_id '')';" +
                    "EXEC (@TSQL)";
                var applsParam = string.Join(",", appl_ids);
                sql = sql.Replace("INSERT_APPL_IDs_HERE", applsParam);

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    conn.Open();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var person = new PersonContact
                        {
                            appl_id = (rdr[0] == DBNull.Value) ? string.Empty : rdr[0].ToString(),
                            first_name = (rdr[1] == DBNull.Value) ? string.Empty : (string)rdr[1],
                            last_name = (rdr[2] == DBNull.Value) ? string.Empty : (string)rdr[2],
                            was_PI_that_year = (rdr[3] == DBNull.Value || ((string)rdr[3]).ToLower() != "pi") ? false : true
                        };
                        if (!results.ContainsKey(person.appl_id))
                        {
                            results.Add(person.appl_id, new List<PersonContact> { person });
                        }
                        else
                        {
                            results[person.appl_id].Add(person);
                        }
                    }

                }
            }

            // prune out the ones that have duplicates
            var deleteTheseKeys = new List<string>();
            foreach (var key in results.Keys)
            {
                if (results[key].Count <= 1)
                {
                    deleteTheseKeys.Add(key);
                }
            }
            foreach (var keyToDelete in deleteTheseKeys)
            {
                results.Remove(keyToDelete);
            }

            return results;
        }

        /// <summary>
        /// Put the MPI info into the grant layer
        /// </summary>
        /// <param name="grantList"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void PopulateMPIIntoGrants(List<GrantLayer> grantList, List<ApplLayerObject> applList, Dictionary<string, List<PersonContact>> mpiInfo)
        {
            foreach (var grant in grantList)
            {
                var applsThisGrant = applList.Where(al => al.grant_id == grant.grant_id).ToList();
                var piListThisGrant = new List<PersonContact>();
                var alreadyAddedFirstLastNamesThisGrant = new HashSet<string>();

                foreach (var appl in applsThisGrant)
                {
                    var piListThisAppl = new List<PersonContact>();
                    var alreadyAddedFirstLastNamesThisAppl = new HashSet<string>();
                    if (mpiInfo.ContainsKey(appl.appl_id))
                    {
                        foreach (var contact in mpiInfo[appl.appl_id])
                        {
                            var firstLastNameChecker = $"{contact.first_name},{contact.last_name}";
                            if (!alreadyAddedFirstLastNamesThisGrant.Contains(firstLastNameChecker))
                            {
                                piListThisGrant.Add(contact);
                                alreadyAddedFirstLastNamesThisGrant.Add(firstLastNameChecker);
                            }
                            if (!alreadyAddedFirstLastNamesThisAppl.Contains(firstLastNameChecker))
                            {
                                piListThisAppl.Add(contact);
                                alreadyAddedFirstLastNamesThisAppl.Add(firstLastNameChecker);
                            }
                        }
                        appl.MPIContacts = piListThisAppl;
                    }
                }
                grant.MPIContacts = piListThisGrant;
            }
        }
    }
}