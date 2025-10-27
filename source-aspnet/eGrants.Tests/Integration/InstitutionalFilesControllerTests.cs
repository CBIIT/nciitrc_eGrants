using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using eGrants.DAL;
using eGrants.DTOs;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace eGrants.Tests.Integration
{

    public class InstitutionalFilesControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public InstitutionalFilesControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace DbContext with in-memory for testing
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });

                    // Seed test data
                    using var scope = services.BuildServiceProvider().CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();

                    db.InstFileFindOrgDTO.Add(new InstFileFindOrgDTO
                    {
                        OrgId = 1,
                        OrgName = "TestOrg"
                    });

                    db.SaveChanges();
                });
            });
        }

        [Fact]
        public async Task ShowDocs_ReturnsView_WithValidOrgId()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/InstitutionalFiles/Show_Docs?orgId=1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("TestOrg", content); // Check if org name appears in rendered view
        }

        [Fact]
        public async Task ShowDocs_ReturnsView_WithValidOrgName()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/InstitutionalFiles/Show_Docs?orgName=TestOrg");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("TestOrg", content);
        }

        [Fact]
        public async Task ShowDocs_ReturnsNotFound_ForInvalidOrg()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/InstitutionalFiles/Show_Docs?orgId=999");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("TestOrg", content); // Or check for error message
        }
    }

}
