using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eGrants.Repositories
{
    public class CommonRepository : ICommonRepository
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CommonRepository(AppDbContext context, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
        }

        // Retrieves amdin codes grants from the database asynchronously
        public async Task<List<AdminCodes>> LoadAdminCodes()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.Grants.Select(p => new AdminCodes
                {
                    admin_phs_org_code = p.admin_phs_org_code,
                    profile = p.admin_phs_org_code == "ca" ? "NCI" : null
                }).Distinct().OrderBy(p => p.admin_phs_org_code).ToListAsync();
            }
        }
    }
}
