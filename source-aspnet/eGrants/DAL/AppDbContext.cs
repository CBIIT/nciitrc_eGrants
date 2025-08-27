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

        public DbSet<eGrantsSearchResults> eGrantResultsByStr { get; set; }
        //public DbSet<ProjectDto> ProjectDtos { get; set; }

        //public async Task<List<eGrantsSearchResults>> Call_sp_web_egrants_Async(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator)
        //{
        //    return await Results
        //        .FromSqlRaw("EXEC dbo.sp_web_egrants @str = {0}, @grant_id = {1}, @package = {2}, @appl_id = {3}, @current_page = {4}, @browser = {5}, @ic = {6}, @operator = {7}", aSearchString, aGrantId, aPackage, aApplId, aCurrentPage, aBrowser, aIC, aOperator)
        //        .ToListAsync();
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<eGrantsSearchResults>().HasNoKey();
            //modelBuilder.Entity<ProjectDto>().HasNoKey(); // Important for stored procedure results
        }



        // DbSet representing the Products table in the database
        // Enables querying and saving instances of Product entities
        //public DbSet<Product> Products { get; set; }
    }
}
