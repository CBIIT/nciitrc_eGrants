using eGrants.DTOs;
using eGrants.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace eGrants.DAL
{
    // Represents the Entity Framework Core database context for the application
    public class AppDbContext : DbContext
    {
        // Constructor that passes configuration options to the base DbContext class
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<eGrantsSearchResults> eGrantsSearchResults { get; set; }
        public DbSet<Grants> Grants { get; set; }

        public DbSet<Categories> Categories { get; set; }

        public DbSet<Pagination> LoadPaginationResults { get; set; }

        public DbSet<FilterSearchResult> FilterSearchResults { get; set; }

        public DbSet<GrantDataYears> GrantDataYears { get; set; }

        public DbSet<VwAppl> VwAppls { get; set; }
        public DbSet<VwGrant> VwGrants { get; set; }

        public DbSet<doclayer> DocLayers { get; set; }

        public DbSet<PersonInvolvement> PersonInvolvements { get; set; }

        public DbSet<InstFileFindOrgDTO> InstFileFindOrgDTO { get; set; }

        public DbSet<InsitutionalOrgNameIndex> InstitutionalOrgNameIndices { get; set; }

        public DbSet<InstFileLoadOrgDocListDTO> InstFileLoadOrgDocListDTO { get; set; }  

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<eGrantsSearchResults>().HasNoKey();
            modelBuilder.Entity<Grants>().HasNoKey();
            modelBuilder.Entity<Categories>().HasNoKey();
            modelBuilder.Entity<Pagination>().HasNoKey();
            modelBuilder.Entity<FilterSearchResult>().HasNoKey();
            modelBuilder.Entity<GrantDataYears>().HasNoKey();
            modelBuilder.Entity<VwAppl>().HasNoKey().ToView("vw_appls");
            modelBuilder.Entity<VwGrant>().HasNoKey().ToView("vw_grants");
            modelBuilder.Entity<PersonInvolvement>().HasNoKey();
            modelBuilder.Entity<doclayer>().HasNoKey();
            modelBuilder.Entity<InstFileFindOrgDTO>().HasNoKey();
            modelBuilder.Entity<InsitutionalOrgNameIndex>().HasNoKey();
            modelBuilder.Entity<InstFileLoadOrgDocListDTO>().HasNoKey();
        }
    }
}
