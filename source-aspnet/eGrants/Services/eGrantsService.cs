using System;
using System.Data;

using eGrants.DAL;
using eGrants.DTOs;
using eGrants.Functions;
using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eGrants.Services
{
    // Defines a service class that implements the IeGrantService interface
    public class eGrantsService : IeGrantsService
    {
        // Dependency injection of a product repository to access data
        private readonly IeGrantsRepository _eGrantRepository;
        const int MAX_RETRIES = 3;

        // Constructor that initializes the repository via dependency injection
        public eGrantsService(IeGrantsRepository eGrantRepository)
        {
            _eGrantRepository = eGrantRepository;
        }

        // Asynchronously retrieves a list of eGrants from the repository
        public async Task<eGrantsSearchViewModel> GetEgrantsByStrAsync(string aSearchString, int aGrantId, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator)
        {
            eGrantsSearchViewModel searchByStrViewModel = new eGrantsSearchViewModel();

            if (string.IsNullOrEmpty(aSearchString))
            {
                searchByStrViewModel.Message = "No data found for the search";
                searchByStrViewModel.grantlayer = null;
            }
            else
            {
                searchByStrViewModel.Str = aSearchString;
                //searchByStrViewModel.Mode = aMode;
                searchByStrViewModel.CurrentTab = 1;
                searchByStrViewModel.CurrentPage = 1;
                searchByStrViewModel.SearchStyle = "by_str";

                Exception exceptionKeeper = null;
                bool completed = false;
                for (int i = 0; i < MAX_RETRIES; ++i)
                {
                    try
                    {
                        searchByStrViewModel = await eGrantsSearchResults(aSearchString, aGrantId, "", aApplId, aCurrentPage, aBrowser, aIC, aOperator, searchByStrViewModel, true);
                        completed = true;
                    }
                    catch (Exception ex)
                    {
                        exceptionKeeper = ex;
                        // 5 retries, ok now log and deal with the error.
                    }
                }
                if (!completed)
                    throw exceptionKeeper;
            }
            return searchByStrViewModel;
        }

        public async Task<eGrantsSearchViewModel> GetEgrantsByFilterAsync(int aFiscalYear, string aMechanism, int aSerialNum, string aAdminCode, int aGrantId, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator)
        {
            eGrantsSearchViewModel searchByStrViewModel = new eGrantsSearchViewModel();

            if (aFiscalYear == 0 && string.IsNullOrEmpty(aMechanism) && aSerialNum == 0) /*string.IsNullOrEmpty(admincode) &&*/
            {
                searchByStrViewModel.Message = "No data found for the search";
                searchByStrViewModel.grantlayer = null;
            }
            else
            {
                var package = "by_filters";
                // create filters search sql query
                var FilterSearchQuery = await _eGrantRepository.FilterSearchQuery(
                    aFiscalYear,
                    aMechanism,
                    aAdminCode,
                    aSerialNum,
                    1,
                    aBrowser,
                    aIC,
                    aOperator);

                string filteredQuery = FilterSearchQuery.Select(x => x.Value).FirstOrDefault();

                searchByStrViewModel.SearchStyle = "by_filters";
                searchByStrViewModel.CurrentTab = 1;
                searchByStrViewModel.CurrentPage = 1;

                // create return value
                if (aFiscalYear != 0)
                {
                    searchByStrViewModel.FilterFY = aFiscalYear;
                }
                else
                {
                    searchByStrViewModel.FilterFY = null; // string.Empty;
                }

                if (aSerialNum != 0)
                    searchByStrViewModel.FilterSerialNumber = aSerialNum;

                searchByStrViewModel.FilterMechanism = aMechanism;
                searchByStrViewModel.FilterAdminCode = aAdminCode;

                searchByStrViewModel = await eGrantsSearchResults(filteredQuery, aGrantId, package, aApplId, aCurrentPage, aBrowser, aIC, aOperator, searchByStrViewModel, true);

                if (searchByStrViewModel.grantlayerproperty != null)
                {
                    searchByStrViewModel.grantlayer = searchByStrViewModel.grantlayerproperty;
                    searchByStrViewModel.appllayer = searchByStrViewModel.appllayerproperty;
                    searchByStrViewModel.ApplCount = searchByStrViewModel.appllayer.Count;
                    searchByStrViewModel.appllayer_All = searchByStrViewModel.appllayerproperty;

                    // show pagination
                    searchByStrViewModel.Pagination = await _eGrantRepository.LoadPaginationAsync(
                        filteredQuery,
                        aIC,
                        aBrowser,
                        package);
                }
                else
                {
                    searchByStrViewModel.Message = "No data found for the search";
                    searchByStrViewModel.grantlayer = null;
                }
            }
            return searchByStrViewModel;
        }

        public async Task<eGrantsSearchViewModel> GetEgrantsByGrantAsync(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator)
        {
            eGrantsSearchViewModel searchByStrViewModel = new eGrantsSearchViewModel();
            searchByStrViewModel = await eGrantsSearchResults(aSearchString, aGrantId, aPackage, aApplId, aCurrentPage, aBrowser, aIC, aOperator, searchByStrViewModel, false);
            return searchByStrViewModel;
        }

        public async Task<List<Pagination>> LoadPagination(string aSearchString, string aIC, string aUserId, string aPackage = null)
        {
            return await _eGrantRepository.LoadPaginationAsync(aSearchString, aIC, aUserId, aPackage);
        }

        public async Task<List<FilterSearchResult>> FilterSearchQuery(int aFiscalYear, string aMechanism, string aAdminCode, int aSerialnum, int aPageNum, string aBrowser, string aIc, string aUserId)
        {
            return await _eGrantRepository.FilterSearchQuery(aFiscalYear, aMechanism, aAdminCode, aSerialnum, aPageNum, aBrowser, aIc, aUserId);
        }

        public async Task<List<GrantDataYears>> GetYearList(string aFiscalYear, string aMechanism, string aAdminCode, string aSerialNumber)
        {
            return await _eGrantRepository.GetYearList(aFiscalYear, aMechanism, aAdminCode, aSerialNumber);
        }

        public async Task<int> CheckGrantID(int aGrantId)
        {
            return await _eGrantRepository.CheckGrantID(aGrantId);
        }

        public async Task<string> GetCategoryNameById(string aCategories)
        {
            return await _eGrantRepository.GetCategoryNameById(aCategories);
        }

        public async Task<List<FilterSearchResult>> GetApplsList(int aGrantId, string aFlagType = null, string aYears = null)
        {
            return await _eGrantRepository.GetApplsList(aGrantId, aFlagType, aYears);
        }
        private async Task<eGrantsSearchViewModel> eGrantsSearchResults(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator, eGrantsSearchViewModel searchByStrViewModel, Boolean loadPagination)
        {
            bool isGrant = false;
            bool isStr = false;
            bool isAppl = false;
            bool searchApplIdIsSoftDeleted = false;     // bail if true

            if (aGrantId != 0)
            {
                isGrant = true;
            }

            if (!string.IsNullOrEmpty(aSearchString))
            {
                isStr = true;
            }

            if (aApplId != 0)
            {
                isAppl = true;
            }

            //aCompleted = true;
            string ic = "NCI";
            var result = await _eGrantRepository.GetSearchResultsAsync(aSearchString, aGrantId, aPackage, aApplId, aCurrentPage, aBrowser, aIC, aOperator);
            if (result != null)
            {
                searchByStrViewModel.SearchResults = result;
            }

            int appl_id = 0;
            var grantList = new List<GrantLayer>();
            var applList = new List<ApplLayerObject>();
            var docList = new List<doclayer>();
            List<ApplLayerObject> appllayerproperty = null;

            foreach (eGrantsSearchResults value in result)
            {
                if (value.tag == 1)
                {
                    var grant = new GrantLayer();
                    grant.grant_id = value.grant_id.ToString();

                    string orgname = value.org_name.ToString();
                    grant.org_name = orgname;
                    long orgId = !string.IsNullOrWhiteSpace(value.org_id.ToString()) ? Convert.ToInt64(value.org_id) : -1;
                    grant.OrgId = orgId;
                    grant.OrgNameToolTip = orgname;
                    grant.OrgFullName = orgname;

                    grant.serial_num = value.serial_num.ToString();
                    grant.grant_num = string.Concat(value.admin_phs_org_code + Convert.ToInt32(value.serial_num).ToString("000000"));
                    grant.former_grant_num = value.former_grant_num?.ToString();
                    grant.latest_full_grant_num = value.latest_full_grant_num.ToString();
                    grant.admin_phs_org_code = value.admin_phs_org_code.ToString();
                    string projTitle = value.project_title.ToString();
                    grant.project_title = projTitle.Truncate(60, "...");
                    grant.pi_name = value.pi_name.ToString();
                    grant.prog_class_code = value.prog_class_code?.ToString();
                    grant.all_activity_code = value.all_activity_code.ToString();
                    grant.current_pi_name = value.current_pi_name.ToString();
                    grant.current_pi_email_address = value.current_pi_email_address.ToString();
                    grant.current_pd_name = value.current_pd_name?.ToString();
                    grant.current_pd_email_address = value.current_pd_email_address?.ToString();
                    grant.current_spec_name = value.current_spec_name?.ToString();
                    grant.current_spec_email_address = value.current_spec_email_address?.ToString();
                    grant.current_bo_email_address = value.current_bo_email_address?.ToString();
                    grant.sv_url = value.sv_url?.ToString();
                    grant.arra_flag = value.arra_flag.ToString();
                    grant.fda_flag = value.fda_flag.ToString();
                    grant.stop_flag = value.stop_flag.ToString();
                    grant.ms_flag = value.ms_flag.ToString();
                    grant.od_flag = value.od_flag.ToString();
                    grant.ds_flag = value.ds_flag.ToString();
                    //grant.adm_supp = value.adm_supp.ToString();

                    if (appl_id <= 0)
                    {
                        grant.institutional_flag1 = value.institutional_flag1.ToString() == "1" ? true : false;
                    }
                    else
                    {
                        grant.institutional_flag1 = value.specific_year_institution1.ToString() == "1" ? true : false;
                    }

                    if (appl_id <= 0)
                    {
                        grant.AnyOrgDoc = value.institutional_flag2.ToString() == "1" ? true : false;
                    }
                    else
                    {
                        grant.AnyOrgDoc = value.specific_year_institution2.ToString() == "1" ? true : false;
                    }

                    grant.inst_flag1_url = value.inst_flag1_url?.ToString();
                    grant.IsCurrentPi = value.is_current_pi.ToString() == "1" ? true : false;
                    grant.SelectedGrantPiName = value.specific_year_pi_name.ToString();
                    grant.SelectedGrantPiEmail = value.specific_year_pi_email_address.ToString();
                    grant.SelectedProjectName = value.specific_year_project_name.ToString();

                    if (string.IsNullOrEmpty(grant.SelectedGrantPiName))
                    {
                        grant.SelectedGrantPiName = grant.current_pi_name;
                    }

                    if (string.IsNullOrEmpty(grant.SelectedGrantPiEmail))
                    {
                        grant.SelectedGrantPiEmail = grant.current_pi_email_address;
                    }
                    // TODO:  Determine if this is still needed
                    //// else
                    //// {
                    ////     grant.current_pi_email_address = grant.SelectedGrantPiEmail;
                    //// }
                    ////

                    if (string.IsNullOrEmpty(grant.SelectedProjectName))
                    {
                        grant.SelectedProjectName = grant.project_title;
                    }

                    string selectedorgname = value.specific_year_org_name.ToString();
                    grant.SelectedOrganizationName = selectedorgname;
                    grant.SelectedOrganizationNameToolTip = selectedorgname;
                    grant.SelectedOrganizationFullName = orgname;
                    grant.FullGrantNumber = value.specific_year_full_grant_num.ToString();


                    if (string.IsNullOrEmpty(grant.SelectedOrganizationName))
                    {
                        grant.SelectedOrganizationName = grant.org_name;
                        grant.SelectedOrganizationNameToolTip = selectedorgname;
                    }

                    grantList.Add(grant);
                }
                else if (value.tag == 2)
                {
                    var appl = new ApplLayerObject();
                    appl.grant_id = value.grant_id.ToString();
                    appl.appl_id = value.appl_id.ToString();
                    appl.appl_type_code = value.appl_type_code.ToString();
                    appl.full_grant_num = value.full_grant_num.ToString();
                    appl.support_year = value.support_year.ToString();
                    appl.deleted_by_impac = value.deleted_by_impac.ToString();
                    appl.doc_count = value.doc_count.ToString();
                    appl.closeout_notcount = value.closeout_notcount.ToString();
                    appl.can_add_doc = value.can_add_doc.ToString();
                    appl.competing = value.competing.ToString();
                    appl.fsr_count = value.fsr_count.ToString();
                    appl.frc_destroyed = value.frc_destroyed.ToString();
                    appl.appl_fda_flag = value.appl_fda_flag.ToString();
                    appl.appl_ms_flag = value.appl_ms_flag.ToString();
                    appl.appl_od_flag = value.appl_od_flag.ToString();
                    appl.appl_ds_flag = value.appl_ds_flag.ToString();
                    appl.closeout_flag = value.closeout_flag.ToString();
                    appl.irppr_id = value.irppr_id?.ToString();
                    appl.can_add_funding = value.can_add_funding.ToString();
                    appl.label = value.label?.ToString();

                    appl.display_docs = "n";
                    if (appl_id != 0 && appl_id.ToString().Equals(appl.appl_id))
                        appl.display_docs = "y";

                    if ((ic.Equals("ca", StringComparison.InvariantCultureIgnoreCase) || ic.Equals("nci", StringComparison.InvariantCultureIgnoreCase)) &&
                        appl.appl_type_code.Equals("3") &&
                        (appl.support_year.ToLower().Contains("s") || appl.support_year.ToLower().Contains("w"))
                    )
                    {
                        appl.can_rename_label = "y";
                    }
                    else
                    {
                        appl.can_rename_label = "n";
                    }

                    bool foundSoftDeletedYear = appl.support_year?.IndexOf("d", StringComparison.OrdinalIgnoreCase) != -1;
                    if (!foundSoftDeletedYear)
                    {
                        // it's not soft deleted, so include it here
                        applList.Add(appl);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(appl.appl_id) && (appl.appl_id.ToString().Equals(appl_id.ToString(), StringComparison.InvariantCultureIgnoreCase)))
                            searchApplIdIsSoftDeleted = true;
                    }
                }
                else if (value.tag == 3)
                {
                    var doc = new doclayer();
                    //doc.appl_id = value.appl_id.ToString();
                    //doc.docs_count = value.docs_count.ToString();

                    docList.Add(doc);
                }
            }

            searchByStrViewModel.grantlayerproperty = grantList;
            searchByStrViewModel.doclayerproperty = docList;
            searchByStrViewModel.appllayerproperty = applList;


            if (searchByStrViewModel.grantlayerproperty != null)
            {
                searchByStrViewModel.grantlayer = searchByStrViewModel.grantlayerproperty;
                searchByStrViewModel.appllayer = applList;
                searchByStrViewModel.ApplCount = searchByStrViewModel.appllayer.Count;
                searchByStrViewModel.appllayer_All = searchByStrViewModel.appllayerproperty;
                searchByStrViewModel.doclayer = searchByStrViewModel.doclayerproperty;
                searchByStrViewModel.DocCount = searchByStrViewModel.doclayer.Count;

                if (loadPagination)
                {
                    // show pagination
                    searchByStrViewModel.Pagination = await _eGrantRepository.LoadPaginationAsync(
                        aSearchString,
                        aIC,
                        aOperator,
                        string.Empty);
                }
            }
            else
            {
                searchByStrViewModel.Message = "No data found for the search";
                searchByStrViewModel.grantlayer = null;
            }

            if (isGrant || isStr)
            {
                PopulateGrantAndStringViews(true, grantList, applList);
            }

            // every appl with > 1 person from IRDB will be in the response
            var mpi_info = await GetAllMPIInfo(applList.Select(al => al.appl_id).ToList());
            PopulateMPIIntoGrants(grantList, applList, mpi_info);

            appllayerproperty = applList;
            searchByStrViewModel.appllayer = appllayerproperty;
            searchByStrViewModel.grantList = grantList;

            return searchByStrViewModel;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isGrant"></param>
        /// <param name="grantList"></param>
        /// <param name="applList"></param>
        private async Task<List<GrantLayer>> PopulateGrantAndStringViews(bool isGrant, List<GrantLayer> aGrantList, List<ApplLayerObject> aApplList)
        {
            if (isGrant)
            {
                foreach (var grant in aGrantList)
                {
                    foreach (var appl in aApplList)
                    {
                        if (grant.grant_id == appl.grant_id)
                        {


                            if (string.Equals(appl.appl_type_code, "4") || string.Equals(appl.appl_type_code, "3")
                                                                        || string.Equals(appl.appl_type_code, "6")
                                                                        || string.Equals(appl.appl_type_code, "8")
                                                                        || appl.deleted_by_impac.ToUpper() == "Y"
                                                                        || Convert.ToInt32(appl.appl_id) < 0)
                            {
                                continue;
                            }

                            if (string.Equals(appl.appl_type_code, "1") || string.Equals(appl.appl_type_code, "2")
                                                                        || string.Equals(appl.appl_type_code, "5")
                                                                        || string.Equals(appl.appl_type_code, "7")
                                                                        || string.Equals(appl.appl_type_code, "9"))
                            {
                                List<GrantAndStringViewsDto> grantAndStringViews = await _eGrantRepository.GetGrantAndStringViews(Convert.ToInt32(appl.appl_id));

                                foreach (var item in grantAndStringViews)
                                {
                                    grant.SelectedProjectName = item.project_title;
                                    grant.SelectedOrganizationName = item.org_name?.ToString();
                                    grant.SelectedGrantPiEmail = item.current_pi_email_address?.ToString();
                                    grant.SelectedGrantPiName = item.first_name?.ToString() + " " + item.last_name?.ToString();
                                }
                            }

                            break;
                        }
                    }
                }
            }
            return aGrantList;
        }

        /// <summary>
        /// Gets the MPI info for the icon
        /// </summary>
        /// <param name="appl_ids"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, List<PersonContact>>> GetAllMPIInfo(List<string> appl_ids)
        {
            var results = new Dictionary<string, List<PersonContact>>();

            if (appl_ids == null || appl_ids.Count == 0)
                return results;

            //using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            //{
            // note that Ingrid learned retrieving email interferes with the ability of the query to return all the MPIs
            //var sql = "DECLARE @TSQL varchar(8000);" +
            //    "SELECT @TSQL = 'SELECT APPL_ID, First_Name, Last_name, Role_Type_Code  FROM OPENQUERY(IRDB,''select e.appl_id, d.person_id, d.first_name, d.last_name, d.mi_name src_mi_name, c.email_addr , e.role_type_code, c.addr_type_code from person_involvements_mv e join persons_secure d on d.person_id = e.person_id left outer join person_addresses_mv c on d.person_id = c.person_id and c.addr_type_code in (''''HOM'''') and c.preferred_addr_code = ''''Y'''' where e.role_type_code in (''''PI'''', ''''MPI'''',''''CPI'''') and appl_id in ( INSERT_APPL_IDs_HERE) and d.person_id = e.person_id '')';" +
            //    "EXEC (@TSQL)";
            //var applsParam = string.Join(",", appl_ids);
            //sql = sql.Replace("INSERT_APPL_IDs_HERE", applsParam);

            //Dictionary<string, List<ApplicantDto>> applicants = await _eGrantRepository.GetAllMPIInfo(appl_ids);

            List<PersonInvolvement> personInvolvements = await _eGrantRepository.GetAllMPIInfo(appl_ids);

            foreach(PersonInvolvement personInvolvement in personInvolvements)
            {
                PersonContact person = new PersonContact
                {
                    appl_id = (personInvolvement.Appl_Id == null) ? string.Empty : personInvolvement.Appl_Id.ToString(),
                    first_name = (personInvolvement.First_Name == null) ? string.Empty : (string)personInvolvement.First_Name,
                    last_name = (personInvolvement.Last_Name == null) ? string.Empty : (string)personInvolvement.Last_Name,
                    was_PI_that_year = (personInvolvement.Role_Type_Code != null && (string)personInvolvement.Role_Type_Code.ToLower() == "pi")
                };
                results.TryAdd(person.appl_id, new List<PersonContact>());
                results[person.appl_id].Add(person);
            }

            //foreach (var kvp in applicants)
            //{
            //    string applId = kvp.Key;
            //    List<ApplicantDto> contacts = kvp.Value;
            //    foreach (PersonContact contact in contacts)
            //    {
            //        var person = new PersonContact
            //        {
            //            appl_id = contact.appl_id,
            //            first_name = contact.first_name,
            //            last_name = contact.last_name,
            //            was_PI_that_year = contact.was_PI_that_year != null && ((string)contact.was_PI_that_year).ToLower() == "pi"
            //        };
            //    }
            //    if (!results.ContainsKey(person.appl_id))
            //    {
            //        results.Add(person.appl_id, new List<PersonContact> { person });
            //    }
            //    else
            //    {
            //        results[person.appl_id].Add(person);
            //    }
            //}

            //using (var cmd = new SqlCommand(sql, conn))
            //{
            //    cmd.CommandType = CommandType.Text;

            //    conn.Open();
            //    var rdr = cmd.ExecuteReader();

            //    while (rdr.Read())
            //    {
            //        var person = new PersonContact
            //        {
            //            appl_id = (rdr[0] == DBNull.Value) ? string.Empty : rdr[0].ToString(),
            //            first_name = (rdr[1] == DBNull.Value) ? string.Empty : (string)rdr[1],
            //            last_name = (rdr[2] == DBNull.Value) ? string.Empty : (string)rdr[2],
            //            was_PI_that_year = (rdr[3] == DBNull.Value || ((string)rdr[3]).ToLower() != "pi") ? false : true
            //        };
            //        if (!results.ContainsKey(person.appl_id))
            //        {
            //            results.Add(person.appl_id, new List<PersonContact> { person });
            //        }
            //        else
            //        {
            //            results[person.appl_id].Add(person);
            //        }
            //    }
            //}
            //}

            // prune out the ones that have duplicates
            //var deleteTheseKeys = new List<string>();
            //foreach (var key in results.Keys)
            //{
            //    if (results[key].Count <= 1)
            //    {
            //        deleteTheseKeys.Add(key);
            //    }
            //}
            //foreach (var keyToDelete in deleteTheseKeys)
            //{
            //    results.Remove(keyToDelete);
            //}

            foreach (var key in results.Where(kvp => kvp.Value.Count <= 1).Select(kvp => kvp.Key).ToList())
            {
                results.Remove(key);
            }

            return results;
        }

        private static void PopulateMPIIntoGrants(List<GrantLayer> grantList, List<ApplLayerObject> applList, Dictionary<string, List<PersonContact>> mpiInfo)
        {
            var applLookup = applList.GroupBy(a => a.grant_id)
                                     .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var grant in grantList)
            {
                var piListThisGrant = new List<PersonContact>();
                var alreadyAddedGrantNames = new HashSet<string>();

                if (!applLookup.TryGetValue(grant.grant_id, out var applsThisGrant))
                    continue;

                foreach (var appl in applsThisGrant)
                {
                    var piListThisAppl = new List<PersonContact>();
                    var alreadyAddedApplNames = new HashSet<string>();

                    if (!mpiInfo.TryGetValue(appl.appl_id, out var contacts))
                        continue;

                    foreach (var contact in contacts)
                    {
                        var nameKey = $"{contact.first_name},{contact.last_name}";

                        if (alreadyAddedGrantNames.Add(nameKey))
                            piListThisGrant.Add(contact);

                        if (alreadyAddedApplNames.Add(nameKey))
                            piListThisAppl.Add(contact);
                    }

                    appl.MPIContacts = piListThisAppl;
                }

                grant.MPIContacts = piListThisGrant;
            }
        }
    }
}