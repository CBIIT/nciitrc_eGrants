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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<eGrantsSearchResults>().HasNoKey();
            modelBuilder.Entity<Grants>().HasNoKey();
            //modelBuilder.Entity<ProjectDto>().HasNoKey(); // Important for stored procedure results
        }



        // DbSet representing the Products table in the database
        // Enables querying and saving instances of Product entities
        //public DbSet<Product> Products { get; set; }
    }
}
