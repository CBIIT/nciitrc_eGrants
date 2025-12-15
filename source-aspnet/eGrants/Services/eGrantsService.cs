using System;
using System.Data;
using System.Security.Cryptography.Xml;

using eGrants.DAL;
using eGrants.DTOs;
using eGrants.Functions;
using eGrants.Models;
using eGrants.Repositories;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

namespace eGrants.Services
{
    // Defines a service class that implements the IeGrantService interface
    public class eGrantsService : IeGrantsService
    {
        // Dependency injection of a eGrant repository to access data
        private readonly IeGrantsRepository _eGrantRepository;
        private readonly ILogger<IeGrantsService> _logger;
        private readonly AppDbContext _context;
        const int MAX_RETRIES = 3;

        // Constructor that initializes the repository via dependency injection
        public eGrantsService(IeGrantsRepository eGrantRepository, ILogger<IeGrantsService> logger = null, AppDbContext context = null)
        {
            _eGrantRepository = eGrantRepository;
            _logger = logger;
            _context = context;
        }

        // Asynchronously retrieves a list of eGrants from the repository
        public async Task<eGrantsSearchViewModel> GetEgrantsByStrAsync(string searchString, int grantId, int applId, int currentPage, SessionInfo sessionInfo)
        {
            eGrantsSearchViewModel searchByStrViewModel = new eGrantsSearchViewModel();

            if (string.IsNullOrEmpty(searchString))
            {
                searchByStrViewModel.Message = "No data found for the search";
                searchByStrViewModel.grantlayer = null;
            }
            else
            {
                searchByStrViewModel.Str = searchString;
                searchByStrViewModel.CurrentTab = 1;
                searchByStrViewModel.CurrentPage = 1;
                searchByStrViewModel.SearchStyle = "by_str";

                Exception exceptionKeeper = null;
                bool completed = false;
                // TODO: determine if retries are necessary anymore
                for (int i = 0; i < MAX_RETRIES; ++i)
                {
                    try
                    {
                        searchByStrViewModel = await eGrantsSearchResults(searchString, grantId, "", applId, currentPage, sessionInfo, searchByStrViewModel, true);
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

            if (searchByStrViewModel.grantlayerproperty != null)
            {
                // show pagination
                searchByStrViewModel.Pagination = await LoadPagination(
                        searchString,
                        sessionInfo.Ic,
                        sessionInfo.UserId,
                        string.Empty);
            }
            else
            {
                searchByStrViewModel.Message = "No data found for the search";
                searchByStrViewModel.grantlayer = null;
            }

            return searchByStrViewModel;
        }

        public async Task<eGrantsSearchViewModel> GetEgrantsByPageAsync(
            string searchString,
            int grantId,
            int applId,
            int currentPage,
            int tabNum,
            SessionInfo sessionInfo,
            IDocumentService _documentService)
        {
            var viewModel = new eGrantsSearchViewModel
            {
                Str = searchString,
                CurrentTab = tabNum,
                CurrentPage = currentPage,
                SearchStyle = "by_page"
            };

            // Guard clause for invalid input
            if (string.IsNullOrEmpty(searchString) || currentPage == 0 || tabNum == 0)
            {
                viewModel.Message = "No data found for the search";
                viewModel.grantlayer = null;
                return viewModel;
            }

            try
            {
                viewModel = await eGrantsSearchResults(
                    searchString,
                    grantId,
                    string.Empty,
                    applId,
                    currentPage,
                    sessionInfo,
                    viewModel,
                    true);
            }
            catch (Exception ex)
            {
                // Log exception if needed, but don’t swallow silently
                viewModel.Message = $"Error occurred: {ex.Message}";
                viewModel.grantlayer = null;
            }

            // Load pagination
            viewModel.Pagination = await LoadPagination(
                searchString,
                sessionInfo.Ic,
                sessionInfo.UserId,
                string.Empty);

            // Load unidentified docs only for "qc"
            if (searchString.Equals("qc", StringComparison.OrdinalIgnoreCase))
            {
                viewModel.UnidentifiedDocs = await _documentService.LoadDocsUnidentified(
                    sessionInfo.ImageServerUrl,
                    sessionInfo.UserId);
            }

            return viewModel;
        }


        public async Task<eGrantsSearchViewModel> GetEgrantsByFilterAsync(int fiscalYear, string mechanism, int serialNum, string adminCode, int grantId, int applId, int currentPage, SessionInfo sessionInfo, int tabNum, string package)
        {
            eGrantsSearchViewModel searchByStrViewModel = new eGrantsSearchViewModel();
            package = !string.IsNullOrEmpty(package) ? package : "by_filters";

            bool isEmptySearch = fiscalYear == 0 && string.IsNullOrEmpty(mechanism) && serialNum == 0;
            bool isInvalidTabOrPackage = tabNum == 0 || currentPage == 0 || string.IsNullOrEmpty(package) || package != "by_filters";

            if (isEmptySearch || isInvalidTabOrPackage)
            {
                searchByStrViewModel.Message = "No data found for the search";
                searchByStrViewModel.grantlayer = null;
            }
            else
            {
                // create filters search sql query
                var FilterSearchQuery = await _eGrantRepository.FilterSearchQuery(
                    fiscalYear,
                    mechanism,
                    adminCode,
                    serialNum,
                    currentPage,
                    sessionInfo);

                string filteredQuery = FilterSearchQuery.Select(x => x.Value).FirstOrDefault();

                searchByStrViewModel.SearchStyle = "by_filters";
                searchByStrViewModel.CurrentTab = tabNum;
                searchByStrViewModel.CurrentPage = currentPage > 1 ? currentPage : 1;

                // create return value
                if (fiscalYear != 0)
                {
                    searchByStrViewModel.FilterFY = fiscalYear;
                }
                else
                {
                    searchByStrViewModel.FilterFY = null;
                }

                if (serialNum != 0)
                    searchByStrViewModel.FilterSerialNumber = serialNum;

                searchByStrViewModel.FilterMechanism = mechanism;
                searchByStrViewModel.FilterAdminCode = adminCode;

                searchByStrViewModel = await eGrantsSearchResults(filteredQuery, grantId, package, applId, currentPage, sessionInfo, searchByStrViewModel, true);

                if (searchByStrViewModel.grantlayerproperty != null)
                {
                    searchByStrViewModel.grantlayer = searchByStrViewModel.grantlayerproperty;
                    searchByStrViewModel.appllayer = searchByStrViewModel.appllayerproperty;
                    searchByStrViewModel.ApplCount = searchByStrViewModel.appllayer.Count;
                    searchByStrViewModel.appllayer_All = searchByStrViewModel.appllayerproperty;

                    // show pagination
                    searchByStrViewModel.Pagination = await _eGrantRepository.LoadPaginationAsync(
                        filteredQuery,
                        sessionInfo.Ic,
                        sessionInfo.Browser,
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

        public async Task<eGrantsSearchViewModel> GetEgrantsByApplAsync(int applId, string mode, string str, SessionInfo sessionInfo)
        {
            var searchByApplViewModel = new eGrantsSearchViewModel();
            var isExisting = await CheckApplID(applId);

            if (applId == 0 || isExisting == 0)
            {
                searchByApplViewModel.Message = "No data found for the search";
                searchByApplViewModel.grantlayer = null;
                return searchByApplViewModel;
            }

            searchByApplViewModel.Str = str ?? searchByApplViewModel.Str;
            searchByApplViewModel.Mode = mode;
            searchByApplViewModel.SearchStyle = "by_appl";
            searchByApplViewModel.ApplID = applId;
            searchByApplViewModel.GrantID = await GetGrantID(applId);
            searchByApplViewModel.SelectedCats = searchByApplViewModel.SelectedCategories = "All";
            searchByApplViewModel.SelectedAppls = applId.ToString();

            searchByApplViewModel = await eGrantsSearchResults("", 0, "", applId, 0,
                sessionInfo, searchByApplViewModel, false);

            searchByApplViewModel.grantlayer = searchByApplViewModel.grantlayerproperty;
            searchByApplViewModel.appllayer_All = searchByApplViewModel.appllayerproperty;
            searchByApplViewModel.ApplCount = searchByApplViewModel.appllayer?.Count ?? 0;
            searchByApplViewModel.doclayer = searchByApplViewModel.doclayerproperty;
            searchByApplViewModel.DocCount = searchByApplViewModel.doclayer?.Count ?? 0;

            var thisAppl = searchByApplViewModel.appllayerproperty?.FirstOrDefault(a => a.appl_id == applId.ToString());

            if (thisAppl != null)
            {
                thisAppl.display_docs = "y";
                searchByApplViewModel.yearName = thisAppl.label;
                searchByApplViewModel.appllayer = new List<ApplLayerObject> { thisAppl };
            }

            return searchByApplViewModel;
        }

        public async Task<eGrantsSearchViewModel> GetEgrantsByGrantAsync(string searchString, int grantId, string package, int applId, int currentPage, string categories, string applsList, string years, string mode, SessionInfo sessionInfo)
        {
            eGrantsSearchViewModel eGrantsSearchViewModelList = new eGrantsSearchViewModel();

            var isExisting = await CheckGrantID(grantId);

            years = years == null ? String.Empty : years;

            if (grantId == 0 || isExisting == 0)
            {
                eGrantsSearchViewModelList.Message = "No data found for the search";
                eGrantsSearchViewModelList.grantlayer = null;
            }
            else
            {
                // load data from DB
                eGrantsSearchViewModelList = await eGrantsSearchResults(searchString, grantId, package, applId, currentPage, sessionInfo, eGrantsSearchViewModelList, false);

                eGrantsSearchViewModelList.bygrant = 1;
                eGrantsSearchViewModelList.GrantID = grantId;
                eGrantsSearchViewModelList.Package = package;
                eGrantsSearchViewModelList.Mode = mode;
                eGrantsSearchViewModelList.SearchStyle = "by_grant";
                eGrantsSearchViewModelList.SelectedYears = years;
                eGrantsSearchViewModelList.SelectedCats = categories;

                if (categories == string.Empty || categories == "All" || categories == "all")
                    eGrantsSearchViewModelList.SelectedCategories = "All";
                else if (categories != string.Empty && categories != "All" && categories != "all")
                    eGrantsSearchViewModelList.SelectedCategories = await GetCategoryNameById(categories ?? "");

                eGrantsSearchViewModelList.grantlayer = eGrantsSearchViewModelList.grantlayerproperty;
                eGrantsSearchViewModelList.appllayer_All = eGrantsSearchViewModelList.appllayerproperty;
                eGrantsSearchViewModelList.appllayer = eGrantsSearchViewModelList.appllayerproperty;
                eGrantsSearchViewModelList.ApplCount = eGrantsSearchViewModelList.appllayer.Count;
                eGrantsSearchViewModelList.doclayer = eGrantsSearchViewModelList.doclayerproperty;
                eGrantsSearchViewModelList.DocCount = eGrantsSearchViewModelList.doclayer.Count;

                // set appls_lis for searching by flag_type
                if (package != string.Empty && package != "All" && package != "all")
                {
                    var filterSearchResult = await GetApplsList(grantId, package);
                    applsList = filterSearchResult.Select(x => x.Value).FirstOrDefault();
                }

                // set appls_lis for searching by years
                if (years != string.Empty)
                {
                    if (years == "all" || years == "All")
                        applsList = "All";
                    else
                    {
                        var filterSearchResult = await GetApplsList(grantId, null, years);
                        applsList = filterSearchResult.Select(x => x.Value).FirstOrDefault();
                    }
                }

                eGrantsSearchViewModelList.SelectedAppls = applsList;

                // reset appllayer and limit show appls if appls_list with search parameters
                if (applsList != null && !applsList.Equals("All", StringComparison.InvariantCultureIgnoreCase))
                {
                    var appllist = new List<ApplLayerObject>();

                    // for more than one appl
                    if (applsList.IndexOf(',') > 1)
                    {
                        var app = applsList.Split(',').ToList();

                        foreach (var appl in eGrantsSearchViewModelList.appllayer)
                        {
                            if (app.Any(n => n == appl.appl_id))
                            {
                                appl.display_docs = "y";
                                appllist.Add(appl);
                            }
                        }

                        eGrantsSearchViewModelList.appllayer = appllist;
                    }

                    // for only one appl
                    else
                    {
                        var app = applsList.Split().ToList();

                        foreach (var appl in eGrantsSearchViewModelList.appllayer)
                            if (app.Any(n => n == appl.appl_id))
                            {
                                appl.display_docs = "y";
                                appllist.Add(appl);
                            }
                        eGrantsSearchViewModelList.appllayer = appllist;
                    }
                }
                else if (applsList != null && applsList.Equals("All", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var appl in eGrantsSearchViewModelList.appllayer)
                    {
                        appl.display_docs = "y";
                    }
                }
            }

            eGrantsSearchViewModelList.Mode = mode;

            return eGrantsSearchViewModelList;
        }

        public async Task<List<Pagination>> LoadPagination(string searchString, string ic, string userId, string package = null)
        {
            return await _eGrantRepository.LoadPaginationAsync(searchString, ic, userId, package);
        }

        public async Task<List<FilterSearchResult>> FilterSearchQuery(int fiscalYear, string mechanism, string adminCode, int serialnum, int pageNum, SessionInfo sessionInfo)
        {
            return await _eGrantRepository.FilterSearchQuery(fiscalYear, mechanism, adminCode, serialnum, pageNum, sessionInfo);
        }

        public async Task<List<GrantDataYears>> GetYearList(string fiscalYear, string mechanism, string adminCode, string serialNumber)
        {
            return await _eGrantRepository.GetYearList(fiscalYear, mechanism, adminCode, serialNumber);
        }

        public async Task<int> CheckGrantID(int grantId)
        {
            return await _eGrantRepository.CheckGrantID(grantId);
        }

        public async Task<int?> GetGrantID(int applId)
        {
            return await _eGrantRepository.GetGrantID(applId);
        }


        public async Task<int> CheckApplID(int grantId)
        {
            return await _eGrantRepository.CheckApplID(grantId);
        }

        public async Task<string> GetCategoryNameById(string categories)
        {
            return await _eGrantRepository.GetCategoryNameById(categories);
        }

        public async Task<List<FilterSearchResult>> GetApplsList(int grantId, string flagType = null, string years = null)
        {
            return await _eGrantRepository.GetApplsList(grantId, flagType, years);
        }

        public async Task<eGrantsSearchViewModel> eGrantsSearchResults(string searchString, int grantId, string package, int applId, int currentPage, SessionInfo sessionInfo, eGrantsSearchViewModel searchByStrViewModel, Boolean loadPagination)
        {
            bool isGrant = false;
            bool isStr = false;
            bool isAppl = false;
            bool searchApplIdIsSoftDeleted = false;     // bail if true

            if (grantId != 0)
            {
                isGrant = true;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                isStr = true;
            }

            if (applId != 0)
            {
                isAppl = true;
            }

            var result = await _eGrantRepository.GetSearchResultsAsync(searchString, grantId, package, applId, currentPage, sessionInfo);

            searchByStrViewModel.SearchResults = result;

            if (searchByStrViewModel.SearchResults == null)
                return new eGrantsSearchViewModel();

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

                    string orgname = value?.org_name?.ToString() ?? string.Empty;
                    grant.org_name = orgname;
                    long orgId = !string.IsNullOrWhiteSpace(value.org_id.ToString()) ? Convert.ToInt64(value.org_id) : -1;
                    grant.OrgId = orgId;
                    grant.OrgNameToolTip = orgname;
                    grant.OrgFullName = orgname;

                    grant.serial_num = value?.serial_num?.ToString() ?? string.Empty;
                    grant.grant_num = string.Concat(value.admin_phs_org_code + Convert.ToInt32(value.serial_num).ToString("000000"));
                    grant.former_grant_num = value.former_grant_num?.ToString() ?? string.Empty;
                    grant.latest_full_grant_num = value?.latest_full_grant_num?.ToString() ?? string.Empty;
                    grant.admin_phs_org_code = value?.admin_phs_org_code?.ToString() ?? string.Empty;
                    string projTitle = value?.project_title?.ToString() ?? string.Empty;
                    grant.project_title = projTitle.Truncate(60, "...");
                    grant.pi_name = value?.pi_name?.ToString() ?? string.Empty;
                    grant.prog_class_code = value.prog_class_code?.ToString() ?? string.Empty;
                    grant.all_activity_code = value?.all_activity_code?.ToString() ?? string.Empty;
                    grant.current_pi_name = value?.current_pi_name?.ToString() ?? string.Empty;
                    grant.current_pi_email_address = value?.current_pi_email_address?.ToString() ?? string.Empty;
                    grant.current_pd_name = value.current_pd_name?.ToString() ?? string.Empty;
                    grant.current_pd_email_address = value.current_pd_email_address?.ToString() ?? string.Empty;
                    grant.current_spec_name = value.current_spec_name?.ToString() ?? string.Empty;
                    grant.current_spec_email_address = value.current_spec_email_address?.ToString() ?? string.Empty;
                    grant.current_bo_email_address = value.current_bo_email_address?.ToString() ?? string.Empty;
                    grant.sv_url = value.sv_url?.ToString() ?? string.Empty;
                    grant.arra_flag = value?.arra_flag?.ToString() ?? string.Empty;
                    grant.fda_flag = value?.fda_flag?.ToString() ?? string.Empty;
                    grant.stop_flag = value?.stop_flag?.ToString() ?? string.Empty;
                    grant.ms_flag = value?.ms_flag?.ToString() ?? string.Empty;
                    grant.od_flag = value?.od_flag?.ToString() ?? string.Empty;
                    grant.ds_flag = value?.ds_flag?.ToString() ?? string.Empty;
                    grant.adm_supp = value.adm_supp.ToString() ?? string.Empty;

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

                    if ((sessionInfo.Ic.Equals("ca", StringComparison.InvariantCultureIgnoreCase) || sessionInfo.Ic.Equals("nci", StringComparison.InvariantCultureIgnoreCase)) &&
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
                        searchString,
                        sessionInfo.Ic,
                        sessionInfo.UserId,
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

        public async Task<List<supplement>> GetSupplements(string act, int grantId, int supportYear, string suffixCode, string docidStr, int formerApplId, string ic, string userId)
        {
            return await _eGrantRepository.GetSupplements(act, grantId, supportYear, suffixCode, docidStr, formerApplId, ic, userId);
        }

        public async Task<List<string>> GetCategoryList(int grantId, string years)
        {
            try
            {
                return await _eGrantRepository.GetCategoryList(grantId, years);
            }
            catch (Exception ex)
            {
                // Optional: log the error here using your logging framework
                _logger.LogError(ex, "Error retrieving category list for GrantId: {grantId}, Years: {years}", grantId, years);
                throw;
            }
        }

        public async Task<List<string>> LoadDataAutocomplete(string type, string term, string mechanism = null, string fy = null, string adminCode = null, string serialNum = null)
        {
            var sql_query = string.Empty;
            if (type == "mechanism")
                sql_query = "sp_web_egrants_load_data_autocomplete_mechanism";

            if (type == "serialnum")
                sql_query = "sp_web_egrants_load_data_autocomplete_serialnum";

            if (type == "fy")
                sql_query = "sp_web_egrants_load_data_autocomplete_fy";

            try
            {
                return await _eGrantRepository.LoadDataAutocomplete(sql_query, term, mechanism, fy, adminCode, serialNum);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing LoadDataAutocomplete for type '{Type}' and term '{Term}'", type, term);
                throw;
            }
        }

        private async Task<List<GrantLayer>> PopulateGrantAndStringViews(bool isGrant, List<GrantLayer> grantList, List<ApplLayerObject> applList)
        {
            if (isGrant)
            {
                foreach (var grant in grantList)
                {
                    foreach (var appl in applList)
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
            return grantList;
        }

        /// <summary>
        /// Gets the MPI info for the icon
        /// </summary>
        /// <param name="appl_ids"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, List<PersonContact>>> GetAllMPIInfo(List<string> applIds)
        {
            var results = new Dictionary<string, List<PersonContact>>();

            if (applIds == null || applIds.Count == 0)
                return results;

            List<PersonInvolvement> personInvolvements = await _eGrantRepository.GetAllMPIInfo(applIds);

            foreach (PersonInvolvement personInvolvement in personInvolvements)
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

        // to load appls by appl_id
        /// <summary>
        /// The load appls_by_applid.
        /// </summary>
        /// <param name="applId">
        /// The appl_id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public async Task<List<VwApplDTO>> LoadApplsByApplid(int? applId)
        {
            return await _eGrantRepository.LoadApplsByApplid(applId);
        }

        public async Task<List<string>> GetAllApplsListAsync(string adminCode, string serialNum)
        {
            var yearList = new List<string>();

            if (!int.TryParse(serialNum, out int parsedSerialNum))
            {
                return yearList;
            }

            using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
            {
                return await _context.VwAppls
                     .Where(a => a.admin_phs_org_code == adminCode && a.serial_num == serialNum)
                     .OrderByDescending(a => a.support_year)
                     .Select(a => $"{a.full_grant_num}:{a.appl_id}")
                     .ToListAsync();
            }
        }

        public async Task<List<ImpacDocs>> LoadImpacDocs(string act, int appl_id)
        {
            var list = new List<ImpacDocs>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await using var cmd = new SqlCommand("sp_web_egrants_impac_docs", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = appl_id;

                await conn.OpenAsync();

                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    list.Add(new ImpacDocs
                    {
                        tag = rdr["tag"]?.ToString(),
                        appl_id = rdr["appl_id"]?.ToString(),
                        full_grant_num = rdr["full_grant_num"]?.ToString(),
                        accepted_date = rdr["accepted_date"]?.ToString(),
                        category_name = rdr["category_name"]?.ToString(),
                        created_date = rdr["created_date"]?.ToString(),
                        url = rdr["url"]?.ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "Error loading ImpacDocs with act={Act} and appl_id={ApplId}",
                    act, appl_id);
                throw; // rethrow so caller can handle if needed
            }

            return list;
        }

        public async Task<eGrantsSearchViewModel> GetEgrantsByQCAsync(
            string searchString,
            int grantId,
            string package, // Consider removing if unused
            int applId,
            int currentPage,
            SessionInfo sessionInfo,
            IDocumentService documentService)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return new eGrantsSearchViewModel
                {
                    Str = searchString,
                    CurrentTab = 1,
                    CurrentPage = currentPage,
                    SearchStyle = "by_qc",
                    Message = "No data found for the search",
                    grantlayer = null
                };
            }

            var viewModel = new eGrantsSearchViewModel
            {
                Str = searchString,
                CurrentTab = 1,
                CurrentPage = currentPage,
                SearchStyle = "by_qc"
            };

            try
            {
                viewModel = await eGrantsSearchResults(
                    searchString,
                    grantId,
                    string.Empty,
                    applId,
                    currentPage,
                    sessionInfo,
                    viewModel,
                    true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Proper logging (replace with ILogger or your logging framework)
                // _logger.LogError(ex, "Error during search execution");

                viewModel.Message = $"Error occurred: {ex.Message}";
                viewModel.grantlayer = null;
            }

            // Run independent tasks concurrently
            var paginationTask = LoadPagination(
                searchString,
                sessionInfo.Ic,
                sessionInfo.UserId,
                string.Empty);

            var unidentifiedDocsTask = documentService.LoadDocsUnidentified(
                sessionInfo.ImageServerUrl,
                sessionInfo.UserId);

            await Task.WhenAll(paginationTask, unidentifiedDocsTask).ConfigureAwait(false);

            viewModel.Pagination = paginationTask.Result;
            viewModel.UnidentifiedDocs = unidentifiedDocsTask.Result;

            return viewModel;
        }

    }
}