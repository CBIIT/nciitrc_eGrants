using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eGrants.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // Constructor injects the application's database context
        public DocumentRepository(AppDbContext context, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
        }

        // Execute the stored procedure 'sp_web_egrants_search_by_appl_id' with the provided parameters.
        // This retrieves document layer records filtered by application ID, search type, category list, IC, and user ID.
        // The results are materialized into a list of 'doclayer' objects.
        public List<doclayer> LoadDocs(int aApplId, string aSearchType, string aCategoryList, string aIc, string aUserId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return context.DocLayers
                .FromSqlRaw("EXEC dbo.sp_web_egrants_search_by_appl_id @appl_id = {0}, @search_type = {1}, @category_list = {2}, @ic = {3}, @operator = {4}", aApplId, aSearchType, aCategoryList, aIc, aUserId)
                .ToList();
            }
        }

        public virtual async Task<List<former_appls>> loadFormerAppls(int grantId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.adminSupplementsWIP
                    .Where(supp => supp.Serial_num == context.Grants
                        .Where(g => g.grant_id == grantId)
                        .Select(g => g.serial_num)
                        .FirstOrDefault())
                    .Select(supp => new former_appls
                    {
                        former_num = supp.Former_num.ToString(),
                        former_appl_id = supp.Former_appl_id.ToString()
                    })
                    .Distinct()
                    .ToListAsync();
            }
        }
    }
}
