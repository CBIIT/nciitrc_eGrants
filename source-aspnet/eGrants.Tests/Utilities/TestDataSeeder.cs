using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eGrants.Models;
using eGrants.Tests.Infrastructure;
using eGrants.ViewModels;

namespace eGrants.Tests.Utilities
{
    public static class TestDataSeeder
    {
        public static async Task SeedTestDataAsync(TestDbContext context)
        {
            var testSupplement = new supplement
            {
                tag = 1,
                id = 1001,
                grant_id = 123,
                serial_num = 456,
                full_grant_num = "FGN-2025-001",
                former_appl_id = 789,
                supp_appl_id = 321,
                support_year = 5,
                suffix_code = "A1",
                former_num = "App1",
                submitted_date = DateTime.UtcNow,
                date_of_submitted = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                category_name = "Health",
                sub_category_name = "Mental Health",
                status = "Submitted",
                url = "http://example.com/supplement/1001",
                moved_date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                moved_by = "admin",
                accession_number = 999,
                admin_phs_org_code = "PHSO123"
            };

            context.Supplements.Add(testSupplement);

            // Seed former_appls data
            var testFormerAppl = new former_appls
            {
                former_num = "App1",
                former_appl_id = "1"
            };

            context.FormerAppls.Add(testFormerAppl);

            var testEgrantsSearchResults = new eGrantsSearchResults
            {
                tag = 1,
                parent = 0,
                grant_id = 123456,
                label = "Cancer Research Grant",
                serial_num = "CA123456",
                admin_phs_org_code = "NCI",
                former_grant_num = "FG123456",
                latest_full_grant_num = "1R01CA123456-01",
                all_activity_code = "R01",
                project_title = "Genomic Analysis of Tumor Progression",
                org_id = 98765,
                org_name = "Stanford University",
                pi_name = "Dr. Jane Doe",
                current_pi_name = "Dr. Jane Doe",
                current_pi_email_address = "jdoe@stanford.edu",
                current_pd_name = "Dr. John Smith",
                current_pd_email_address = "jsmith@stanford.edu",
                current_spec_name = "Dr. Emily White",
                current_spec_email_address = "ewhite@nih.gov",
                current_bo_email_address = "grants@stanford.edu",
                prog_class_code = "PC02",
                sv_url = "https://grants.nih.gov/view/1R01CA123456-01",
                arra_flag = "N",
                fda_flag = "Y",
                stop_flag = "N",
                ms_flag = "Y",
                od_flag = "N",
                ds_flag = "Y",
                adm_supp = 1,
                institutional_flag1 = 1,
                institutional_flag2 = 0,
                inst_flag1_url = "https://stanford.edu/grants/flag1",
                appl_id = 1001,
                full_grant_num = "1R01CA123456-01",
                project_title_2 = "Tumor Progression Study",
                appl_type_code = "1",
                deleted_by_impac = null,
                doc_count = 12,
                closeout_notcount = 2,
                competing = "Yes",
                fsr_count = 3,
                frc_destroyed = 0,
                appl_fda_flag = "Y",
                appl_ms_flag = "Y",
                appl_od_flag = "N",
                appl_ds_flag = "Y",
                closeout_flag = "N",
                irppr_id = 5555,
                can_add_doc = "Y",
                can_add_funding = "Y",
                docs_count = 12,
                is_current_pi = 1,
                specific_year_pi_name = "Dr. Jane Doe",
                specific_year_pi_email_address = "jdoe@stanford.edu",
                specific_year_project_name = "Tumor Progression Study",
                specific_year_org_name = "Stanford University",
                specific_year_full_grant_num = "1R01CA123456-01",
                specific_year_institution1 = 1,
                specific_year_institution2 = 0,
                support_year = "2023"
            };

            context.eGrantsSearchResults.Add(testEgrantsSearchResults);

            await context.SaveChangesAsync();
        }
    }

}
