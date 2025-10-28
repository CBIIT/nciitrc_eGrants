using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eGrants.Models;
using eGrants.Repositories.Interfaces;

using Xunit.Sdk;

namespace eGrants.Tests.Infrastructure
{
    public class TestCommonRepository : ICommonRepository
    {
        private readonly TestDbContext _context;

        public TestCommonRepository(TestDbContext context)
        {
            _context = context;
        }
        public Task<List<AdminCodes>> LoadAdminCodes() => throw new NotImplementedException();
    }
}
