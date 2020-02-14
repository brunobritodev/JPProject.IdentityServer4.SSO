
using Jwks.Manager;
using Jwks.Manager.Store.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jp.Database.Context
{
    public class AspNetGeneralContext : DbContext, IDataProtectionKeyContext, ISecurityKeyContext
    {
        public AspNetGeneralContext(DbContextOptions<AspNetGeneralContext> options)
            : base((DbContextOptions)options)
        {
        }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public DbSet<SecurityKeyWithPrivate> SecurityKeys { get; set; }
    }
}