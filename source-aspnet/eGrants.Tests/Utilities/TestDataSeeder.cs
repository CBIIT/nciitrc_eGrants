using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eGrants.Models;
using eGrants.Tests.Infrastructure;

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

            await context.SaveChangesAsync();
        }
    }

}
