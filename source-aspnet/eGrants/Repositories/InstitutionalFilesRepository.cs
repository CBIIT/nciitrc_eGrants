using System.Data;
using System.Security.Cryptography;

using eGrants.DAL;
using eGrants.DTOs;
using eGrants.Models;
using eGrants.Repositories.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eGrants.Repositories
{
    public class InstitutionalFilesRepository : IInstitutionalFilesRepository
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // Constructor injects the application's database context
        public InstitutionalFilesRepository(AppDbContext context, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task<InstFileFindOrgDTO> FindOrg(int orgId, string orgName = "")
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Execute the pagination stored procedure and return the results.
                var results = await context.InstFileFindOrgDTO
                .FromSqlRaw("EXEC dbo.sp_web_egrants_institutional_file_find_org @org_id = {0}, @org_name = {1}", orgId, orgName.Replace("'", "''"))
                .ToListAsync();

                return results.SingleOrDefault();
            }
        }

        public async Task<List<InsitutionalOrgNameIndex>> LoadOrgNameCharacterIndices()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                return await context.InstitutionalOrgNameIndices
                    .FromSqlRaw("SELECT index_id, character_index, index_seq FROM character_index order by index_seq")
                    .ToListAsync();
            }
        }

        public async Task<List<InstFileLoadOrgDocListDTO>> LoadOrgDocList(int orgId)
        {

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Execute the pagination stored procedure and return the results.
                return await context.InstFileLoadOrgDocListDTO
                .FromSqlRaw("EXEC dbo.sp_web_egrants_inst_files_show_docs @org_id = {0}", orgId)
                .ToListAsync();
            }
        }
    }
}
