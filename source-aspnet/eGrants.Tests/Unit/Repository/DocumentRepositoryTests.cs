using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eGrants.DAL;
using eGrants.Repositories;
using eGrants.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace eGrants.Tests.Unit.Repository
{
    public class DocumentRepositoryTests
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ServiceProvider _serviceProvider;

        public DocumentRepositoryTests()
        {
            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            _serviceProvider = services.BuildServiceProvider();
            _scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        [Fact]
        public async Task loadFormerAppls_ReturnsExpectedResults()
        {
            // Arrange
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            context.Grants.Add(new Grants
            {
                grant_id = 1,
                serial_num = 123,
                admin_phs_org_code = "ORG001",
                arra_flag = "false",
                close_out_date = DateTime.Now.AddYears(-1).ToString(),
                destruction_reason = "Expired",
                fda_flag = "false",
                former_admin_phs_org_code = "ORG002",
                future_admin_phs_org_code = "ORG003",
                grant_close_date = DateTime.Now.AddMonths(-6).ToString(),
                is_tobacco = "false",
                mechanism_code = "R01",
                paperless = "true",
                stop_sign = "false",
                to_be_destroyed = "false"
            });

            var supp1 = new IMPP_Admin_Supplements_WIP { Serial_num = 123, Former_num = 100, Former_appl_id = 200 };
            var supp2 = new IMPP_Admin_Supplements_WIP { Serial_num = 123, Former_num = 101, Former_appl_id = 201 };

            //context.Grants.Add(grant);
            context.adminSupplementsWIP.AddRange(supp1, supp2);
            await context.SaveChangesAsync();

            var repository = new DocumentRepository(context, _scopeFactory);

            // Act
            var result = await repository.loadFormerAppls(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.former_num == "100" && r.former_appl_id == "200");
            Assert.Contains(result, r => r.former_num == "101" && r.former_appl_id == "201");
        }
    }
}
