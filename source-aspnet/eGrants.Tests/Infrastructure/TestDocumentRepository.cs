using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eGrants.Models;
using eGrants.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace eGrants.Tests.Infrastructure
{
    public class TestDocumentRepository : IDocumentRepository
    {
        private readonly TestDbContext _context;
        private readonly bool _shouldThrow;

        public TestDocumentRepository(TestDbContext context, bool shouldThrow = false)
        {
            _context = context;
            _shouldThrow = shouldThrow;
        }
        public List<doclayer> LoadDocs(int aApplId, string aSearchType, string aCategoryList, string aIc, string aUserId) => throw new NotImplementedException();
        public async Task<List<former_appls>> loadFormerAppls(int grantId)
        {
            if (_shouldThrow)
                throw new Exception("Document service failed");

            // Adjust filtering logic if needed
            return await _context.FormerAppls
                .Where(f => f.former_num == "App1") // or based on grantId if applicable
                .ToListAsync();
        }
    }
}
