using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace eGrants.Tests.Unit.Repository
{
    /// <summary>
    /// Unit tests for the EF Core LINQ methods of <see cref="DocumentRepository"/>.
    ///
    /// These exercise the query logic against the EF Core in-memory provider, so they run
    /// fast and require no live database. The repository's raw ADO.NET stored-procedure
    /// methods (LoadDocs, DocModify, GetDocID, report_doc_error, LoadDocsUnidentified) open
    /// a real SqlConnection and therefore cannot be covered here; they need integration
    /// tests against a real database.
    /// </summary>
    public class DocumentRepositoryTests
    {
        // A test-only context that gives the otherwise-keyless views (VwCategories,
        // categories_subcat_lookup) primary keys so they can be seeded with the in-memory
        // provider via Add + SaveChanges. Keys are static configuration (no per-instance
        // closures), so EF Core's cached model stays correct across tests; data is isolated
        // by using a distinct in-memory database name per test.
        private sealed class TestAppDbContext : AppDbContext
        {
            public TestAppDbContext(DbContextOptions<AppDbContext> options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<VwCategories>().HasKey(c => c.category_id);
                modelBuilder.Entity<categories_subcat_lookup>()
                    .HasKey(c => new { c.parent_category_id, c.sub_category_name });
            }
        }

        // Builds a scope factory whose scopes each resolve an in-memory AppDbContext seeded
        // with the provided view data, matching how DocumentRepository obtains its context.
        private static IServiceScopeFactory CreateScopeFactory(
            string databaseName,
            List<VwCategories> categories = null,
            List<categories_subcat_lookup> subCategories = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var services = new ServiceCollection();
            services.AddScoped<AppDbContext>(_ => new TestAppDbContext(options));

            var provider = services.BuildServiceProvider();
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (categories is not null)
                {
                    context.VwCategories.AddRange(categories);
                }

                if (subCategories is not null)
                {
                    context.CategoriesSubcatLookup.AddRange(subCategories);
                }

                context.SaveChanges();
            }

            return scopeFactory;
        }

        private static DocumentRepository CreateRepository(IServiceScopeFactory scopeFactory)
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // The first constructor argument is only used by the raw-ADO.NET methods, which
            // these tests do not call; the LINQ methods resolve their context from the scope
            // factory.
            return new DocumentRepository(context, scopeFactory);
        }

        [Fact]
        public async Task LoadCategories_ReturnsOnlyUploadableCategoriesForIc_OrderedByName()
        {
            var categories = new List<VwCategories>
            {
                new VwCategories { category_id = 3, category_name = "Zeta", ic = "NCI", can_upload = "yes", impac_doc_type_code = "T", package = "P3", input_type = "text", input_constraint = "none" },
                new VwCategories { category_id = 1, category_name = "Alpha", ic = "NCI", can_upload = "yes", impac_doc_type_code = "T", package = "P1", input_type = "text", input_constraint = "none" },
                // Excluded: can_upload is not "yes".
                new VwCategories { category_id = 2, category_name = "Beta", ic = "NCI", can_upload = "no", impac_doc_type_code = "T", package = "P2", input_type = "text", input_constraint = "none" },
                // Excluded: different IC.
                new VwCategories { category_id = 4, category_name = "Other", ic = "NHLBI", can_upload = "yes", impac_doc_type_code = "T", package = "P4", input_type = "text", input_constraint = "none" },
            };

            var scopeFactory = CreateScopeFactory(nameof(LoadCategories_ReturnsOnlyUploadableCategoriesForIc_OrderedByName), categories);
            var repository = CreateRepository(scopeFactory);

            var result = await repository.LoadCategories("NCI");

            Assert.Equal(2, result.Count);
            Assert.Equal(new[] { "Alpha", "Zeta" }, result.Select(c => c.category_name).ToArray());
            Assert.Equal(1, result[0].category_id);
            Assert.Equal("P1", result[0].package);
        }

        [Fact]
        public async Task LoadCategories_WithNoMatchingIc_ReturnsEmptyList()
        {
            var categories = new List<VwCategories>
            {
                new VwCategories { category_id = 1, category_name = "Alpha", ic = "NCI", can_upload = "yes", impac_doc_type_code = "T" },
            };

            var scopeFactory = CreateScopeFactory(nameof(LoadCategories_WithNoMatchingIc_ReturnsEmptyList), categories);
            var repository = CreateRepository(scopeFactory);

            var result = await repository.LoadCategories("NHLBI");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMaxCategoryId_ReturnsHighestCategoryIdForIc()
        {
            var categories = new List<VwCategories>
            {
                new VwCategories { category_id = 5, category_name = "A", ic = "NCI", can_upload = "yes", impac_doc_type_code = "T" },
                new VwCategories { category_id = 12, category_name = "B", ic = "NCI", can_upload = "no", impac_doc_type_code = "T" },
                new VwCategories { category_id = 99, category_name = "C", ic = "NHLBI", can_upload = "yes", impac_doc_type_code = "T" },
            };

            var scopeFactory = CreateScopeFactory(nameof(GetMaxCategoryId_ReturnsHighestCategoryIdForIc), categories);
            var repository = CreateRepository(scopeFactory);

            var result = await repository.GetMaxCategoryId("NCI");

            Assert.Equal(12, result);
        }

        [Fact]
        public async Task GetMaxCategoryId_WithNoCategoriesForIc_ReturnsZero()
        {
            var categories = new List<VwCategories>
            {
                new VwCategories { category_id = 5, category_name = "A", ic = "NCI", can_upload = "yes", impac_doc_type_code = "T" },
            };

            var scopeFactory = CreateScopeFactory(nameof(GetMaxCategoryId_WithNoCategoriesForIc_ReturnsZero), categories);
            var repository = CreateRepository(scopeFactory);

            // No rows for this IC: the MaxAsync throws internally and the method returns 0.
            var result = await repository.GetMaxCategoryId("UNKNOWN");

            Assert.Equal(0, result);
        }

        [Fact]
        public async Task LoadSubCategoryList_ReturnsAllSubCategories()
        {
            var subCategories = new List<categories_subcat_lookup>
            {
                new categories_subcat_lookup { parent_category_id = 1, sub_category_name = "Sub A" },
                new categories_subcat_lookup { parent_category_id = 2, sub_category_name = "Sub B" },
            };

            var scopeFactory = CreateScopeFactory(nameof(LoadSubCategoryList_ReturnsAllSubCategories), subCategories: subCategories);
            var repository = CreateRepository(scopeFactory);

            var result = await repository.LoadSubCategoryList();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, s => s.parent_category_id == 1 && s.sub_category_name == "Sub A");
            Assert.Contains(result, s => s.parent_category_id == 2 && s.sub_category_name == "Sub B");
        }

        [Fact]
        public async Task LoadSubCategoryList_WithNoRows_ReturnsEmptyList()
        {
            var scopeFactory = CreateScopeFactory(nameof(LoadSubCategoryList_WithNoRows_ReturnsEmptyList));
            var repository = CreateRepository(scopeFactory);

            var result = await repository.LoadSubCategoryList();

            Assert.Empty(result);
        }
    }
}
