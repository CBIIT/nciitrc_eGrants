using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eGrants.Models;

using Microsoft.EntityFrameworkCore;

namespace eGrants.Tests.Infrastructure
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<supplement> Supplements { get; set; }

        public DbSet<former_appls> FormerAppls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<former_appls>().HasKey(g => g.former_num);
        }
    }

}
