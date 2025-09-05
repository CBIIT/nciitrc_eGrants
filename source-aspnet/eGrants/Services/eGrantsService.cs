using eGrants.DTOs;
using eGrants.Functions;
using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

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
        //public async Task<List<eGrantsSearchResults>> GetEgrantsByStrAsync(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator)
        //{
        //    // Placeholder for potential business logic before fetching data
        //    return await _eGrantRepository.GetEgrantsByStrAsync(aSearchString, aGrantId, aPackage, aApplId, aCurrentPage, aBrowser, aIC, aOperator);
        //}

        public async Task<eGrantsSearchByStrViewModel> GetEgrantsByStrAsync(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator, string aMode)
        {
            eGrantsSearchByStrViewModel searchByStrViewModel = new eGrantsSearchByStrViewModel();
            //eGrantSearchDTO searchDTO = new eGrantSearchDTO();
            if (string.IsNullOrEmpty(aSearchString))
            {
                searchByStrViewModel.Message = "No data found for the search";
                searchByStrViewModel.grantLayer = null;
            }
            else
            {
                searchByStrViewModel.Str = aSearchString;
                searchByStrViewModel.Mode = aMode;
                searchByStrViewModel.CurrentTab = 1;
                searchByStrViewModel.CurrentPage = 1;
                searchByStrViewModel.SearchStyle = "by_str";

                Exception exceptionKeeper = null;
                bool completed = false;
                for (int i = 0; i < MAX_RETRIES; ++i)
                {
                    try
                    {
                        completed = true;
                        string ic = "NCI";
                        var result = await _eGrantRepository.GetEgrantsByStrAsync(aSearchString, aGrantId, aPackage, aApplId, aCurrentPage, aBrowser, aIC, aOperator);
                        //var result = await _context.Call_sp_web_egrants_Async(str, 0, string.Empty, 0, 0, "Chrome", "NCI", "hooverrl");
                        if (result != null)
                        {
                            searchByStrViewModel.SearchResults = result;
                            //searchByStrViewModel = new eGrantsSearchByStrViewModel
                            //{
                            //    SearchResults = result,
                            //    tag = result.tag,
                            //    grant_id = p.grant_id,
                            //    FullGrantNumber = null,  //added because not pulled from database
                            //    former_grant_num = p.former_grant_num,
                            //    grant_num = null,
                            //    SelectedProjectName = null, //added because not pulled from database
                            //    project_title = p.project_title,
                            //    latest_full_grant_num = p.latest_full_grant_num,
                            //    current_pi_name = p.current_pi_name,
                            //    SelectedGrantPiName = null, //added because not pulled from database
                            //    SelectedOrganizationName = null, //added because not pulled from database
                            //    org_name = p.org_name,
                            //    current_pd_email_address = p.current_pd_email_address,
                            //    current_pd_name = p.current_pd_name,
                            //    prog_class_code = p.prog_class_code,
                            //    current_spec_email_address = p.current_spec_email_address,
                            //    current_spec_name = p.current_spec_name,
                            //    SelectedGrantPiEmail = null, //added because not pulled from database
                            //    MPIContacts = new List<PersonContact>(), //added because not pulled from database
                            //    institutional_flag1 = Convert.ToBoolean(p.institutional_flag1)
                            //};
                        }

                        int appl_id = 0;
                        var grantList = new List<GrantLayer>();
                        var applList = new List<ApplLayerObject>();
                        bool searchApplIdIsSoftDeleted = false;
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
                                grant.adm_supp = value.adm_supp.ToString();

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

                        appllayerproperty = applList;
                        searchByStrViewModel.appllayer = appllayerproperty;
                        searchByStrViewModel.grantList = grantList;
                        //searchByStrViewModel.grantlayerproperty = grantList;

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
    }
}
