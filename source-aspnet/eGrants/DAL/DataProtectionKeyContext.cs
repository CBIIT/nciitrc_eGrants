using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eGrants.DAL
{
    /// <summary>
    /// Storage for the ASP.NET Core Data Protection key ring (eGrants-1259).
    /// <para>
    /// Data Protection generates and rotates these keys on its own; nothing in the
    /// application reads or writes them directly. This context exists only so the
    /// framework can persist the key ring to dbo.DataProtectionKeys instead of
    /// holding it per-process, where it would be lost on every app pool recycle.
    /// </para>
    /// <para>
    /// Kept separate from <see cref="AppDbContext"/> because the key table is
    /// infrastructure and has no relationship to any eGrants business entity.
    /// </para>
    /// </summary>
    public class DataProtectionKeyContext : DbContext, IDataProtectionKeyContext
    {
        public DataProtectionKeyContext(DbContextOptions<DataProtectionKeyContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// One row per key. The DataProtectionKey entity is supplied by
        /// Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.
        /// </summary>
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    }
}
