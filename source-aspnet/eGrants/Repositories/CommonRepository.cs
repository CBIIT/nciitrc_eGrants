using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eGrants.Repositories
{
    public class CommonRepository : ICommonRepository
    {
        private readonly AppDbContext _context;

        public CommonRepository(AppDbContext context)
        {
            _context = context;
        }

        // Retrieves amdin codes grants from the database asynchronously
        public async Task<List<AdminCodes>> LoadAdminCodes()
        {
            //List<AdminCodes> test = await _context.Grants.Select(p => new AdminCodes
            //{
            //    admin_phs_org_code = p.admin_phs_org_code,
            //    profile = p.admin_phs_org_code == "ca" ? "NCI" : null
            //}).Distinct().OrderBy(p => p.admin_phs_org_code).ToListAsync();

            //return test;

            return await _context.Grants.Select(p => new AdminCodes
            {
                admin_phs_org_code = p.admin_phs_org_code,
                profile = p.admin_phs_org_code == "ca" ? "NCI" : null
            }).Distinct().OrderBy(p => p.admin_phs_org_code).ToListAsync();
        }

    }
}
