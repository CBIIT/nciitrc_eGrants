using System.Collections.Generic;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Services;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace eGrants.Tests.Integration
{
    /// <summary>
    /// Integration tests for <see cref="ApplDestructedService"/>.
    ///
    /// Every method on this service uses raw ADO.NET against the live database (inline SQL
    /// and the sp_web_admin_appl_destructed* stored procedures / archival functions), so it
    /// cannot be exercised with the EF Core in-memory provider. These tests therefore run
    /// against the real database and are gated with <see cref="DbFactAttribute"/>, which
    /// auto-skips them when the target server is not reachable.
    ///
    /// They are intentionally scoped to the read-only methods and assert the call succeeds
    /// and returns a materialized (non-null) result, rather than asserting on specific data
    /// that varies by environment.
    /// </summary>
    public class ApplDestructedServiceTests
    {
        private static ApplDestructedService CreateService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(TestDatabase.ConnectionString)
                .Options;

            var context = new AppDbContext(options);

            return new ApplDestructedService(new SessionInfoService(), context);
        }

        [DbFact]
        public void LoadYears_ReturnsNonNullList()
        {
            var service = CreateService();

            List<DestructionYears> result = service.LoadYears();

            Assert.NotNull(result);
        }

        [DbFact]
        public void LoadDescripCodes_ReturnsNonNullList()
        {
            var service = CreateService();

            List<DescripCodes> result = service.LoadDescripCodes();

            Assert.NotNull(result);
        }

        [DbFact]
        public void LoadExceptionCodes_ReturnsNonNullList()
        {
            var service = CreateService();

            List<ExceptionCodes> result = service.LoadExceptionCodes();

            Assert.NotNull(result);
        }

        [DbFact]
        public void LoadSearchInfo_WithArbitraryFilters_ReturnsNonNullList()
        {
            var service = CreateService();

            List<SearchInfo> result = service.LoadSearchInfo(year: 0, status_code: null, exception_code: null, str: null);

            Assert.NotNull(result);
        }

        [DbFact]
        public void CheckPermission_WithUnknownUser_ReturnsNonNullResult()
        {
            var service = CreateService();

            // fn_is_Archival_admin returns a permission string; an unknown user should still
            // produce a defined (non-null) result rather than throwing.
            var result = service.CheckPermission(year: 0, userid: "does-not-exist");

            Assert.NotNull(result);
        }
    }
}
