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

        //public DbSet<person_involvements_mv> PersonInvolvements { get; set; }
        //public DbSet<Person> Persons { get; set; }
        //public DbSet<PersonAddress> PersonAddresses { get; set; }

        public DbSet<PersonInvolvement> PersonInvolvements { get; set; }

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

            //modelBuilder.Entity<person_involvements_mv>()
            //    .HasKey(pi => new { pi.ApplId, pi.PersonId });

            //modelBuilder.Entity<Person>()
            //    .HasKey(p => p.PersonId);

            //modelBuilder.Entity<PersonAddress>()
            //    .HasKey(pa => new { pa.PersonId, pa.AddrTypeCode });

            //// Relationships
            //modelBuilder.Entity<person_involvements_mv>()
            //    .HasOne(pi => pi.Person)
            //    .WithMany(p => p.PersonInvolvements)
            //    .HasForeignKey(pi => pi.PersonId);

            //modelBuilder.Entity<Person>()
            //    .HasMany(p => p.Addresses)
            //    .WithOne()
            //    .HasForeignKey(pa => pa.PersonId);

            //modelBuilder.Entity<ProjectDto>().HasNoKey(); // Important for stored procedure results
        }



        // DbSet representing the Products table in the database
        // Enables querying and saving instances of Product entities
        //public DbSet<Product> Products { get; set; }
    }
}
