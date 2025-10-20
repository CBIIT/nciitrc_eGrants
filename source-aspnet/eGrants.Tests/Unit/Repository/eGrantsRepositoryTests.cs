using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories;

using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using Moq;

namespace eGrants.Tests.Unit.Repository
{
    public class eGrantsRepositoryTests
    {
        //[Fact]
        //public async Task GetSupplements_ReturnsExpectedResults()
        //{
        //    // Arrange
        //    var expectedSupplements = new List<supplement>
        //    {
        //        new supplement
        //        {
        //            tag = 1,
        //            id = 101,
        //            grant_id = 2023,
        //            serial_num = 555,
        //            full_grant_num = "GR123456",
        //            former_appl_id = 1001,
        //            supp_appl_id = 1002,
        //            support_year = 23,
        //            suffix_code = "A1",
        //            former_num = "FN789",
        //            submitted_date = DateTime.UtcNow,
        //            date_of_submitted = "2025-10-17",
        //            category_name = "Health",
        //            sub_category_name = "Nutrition",
        //            status = "Approved",
        //            url = "http://example.com/supplement/101",
        //            moved_date = "2025-10-01",
        //            moved_by = "admin",
        //            accession_number = 987654,
        //            admin_phs_org_code = "HHS"
        //        },
        //        new supplement
        //        {
        //            tag = 2,
        //            id = 102,
        //            grant_id = 2024,
        //            serial_num = 556,
        //            full_grant_num = "GR654321",
        //            former_appl_id = 1003,
        //            supp_appl_id = 1004,
        //            support_year = 23,
        //            suffix_code = "B2",
        //            former_num = "FN456",
        //            submitted_date = DateTime.UtcNow.AddDays(-10),
        //            date_of_submitted = "2025-10-07",
        //            category_name = "Science",
        //            sub_category_name = "Biotech",
        //            status = "Pending",
        //            url = "http://example.com/supplement/102",
        //            moved_date = "2025-09-30",
        //            moved_by = "reviewer",
        //            accession_number = 123456,
        //            admin_phs_org_code = "NIH"
        //        }
        //    }.AsQueryable();


        //    var mockSet = new Mock<DbSet<supplement>>();
        //    mockSet.As<IQueryable<supplement>>().Setup(m => m.Provider).Returns(expectedSupplements.Provider);
        //    mockSet.As<IQueryable<supplement>>().Setup(m => m.Expression).Returns(expectedSupplements.Expression);
        //    mockSet.As<IQueryable<supplement>>().Setup(m => m.ElementType).Returns(expectedSupplements.ElementType);
        //    mockSet.As<IQueryable<supplement>>().Setup(m => m.GetEnumerator()).Returns(expectedSupplements.GetEnumerator());

        //    mockSet.Setup(m => m.ToListAsync(It.IsAny<CancellationToken>()))
        //           .ReturnsAsync(expectedSupplements.ToList());

        //    var mockContext = new Mock<AppDbContext>();
        //    mockContext.Setup(c => c.supplements).Returns(mockSet.Object);

        //    var mockProvider = new Mock<IServiceProvider>();
        //    mockProvider.Setup(p => p.GetService(typeof(AppDbContext))).Returns(mockContext.Object);

        //    var mockScope = new Mock<IServiceScope>();
        //    mockScope.Setup(s => s.ServiceProvider).Returns(mockProvider.Object);

        //    var mockScopeFactory = new Mock<IServiceScopeFactory>();
        //    mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

        //    var repository = new eGrantsRepository(mockContext.Object,mockScopeFactory.Object); // Replace with your actual repo class

        //    // Act
        //    var result = await repository.GetSupplements("act", 1, 23, "A", "doc123", 0, "IC", "user1");

        //    // Assert
        //    Assert.Equal(2, result.Count);
        //    Assert.Contains(result, s => s.category_name == "Health");
        //    Assert.Contains(result, s => s.category_name == "Science");
        //}

        #region GetSupplements Tests
        [Fact]
        public async Task GetSupplements_ReturnsExpectedResults()
        {
            // Arrange: create service collection and configure InMemory DB
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            var serviceProvider = services.BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            var repository = new TestSupplementRepository(context, scopeFactory);
            var result = await repository.GetSupplements("act", 1, 2023, "A", "doc123", 0, "IC", "user1");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, s => s.status == "Approved");
        }
    }
    public class TestSupplementRepository : eGrantsRepository
    {
        public TestSupplementRepository(AppDbContext context, IServiceScopeFactory scopeFactory) : base(context, scopeFactory) { }

        public override Task<List<supplement>> GetSupplements(string act, int grantId, int supportYear, string suffixCode, string docidStr, int formerApplId, string ic, string userId)
        {
            var mockData = new List<supplement>
            {
                new supplement { id = 101, full_grant_num = "GR123456", status = "Approved" },
                new supplement { id = 102, full_grant_num = "GR654321", status = "Pending" }
            };

            return Task.FromResult(mockData);
        }
    }
    #endregion
}
