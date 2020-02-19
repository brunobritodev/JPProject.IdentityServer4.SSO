using Jp.Database;
using JPProject.Domain.Core.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using static Jp.Database.ProviderConfiguration;

namespace Microsoft.Extensions.Configuration
{
    public static class ProviderSelector
    {
        public static IServiceCollection ConfigureProviderForContext<TContext>(
            this IServiceCollection services,
            (DatabaseType, string) options) where TContext : DbContext
        {
            var (database, connString) = options;
            Build(connString);
            return database switch
            {
                DatabaseType.SqlServer => services.PersistStore<TContext>(With.SqlServer),
                DatabaseType.MySql => services.PersistStore<TContext>(With.MySql),
                DatabaseType.Postgre => services.PersistStore<TContext>(With.Postgre),
                DatabaseType.Sqlite => services.PersistStore<TContext>(With.Sqlite),

                _ => throw new ArgumentOutOfRangeException(nameof(database), database, null)
            };
        }

        public static Action<DbContextOptionsBuilder> WithProviderAutoSelection((DatabaseType, string) options)
        {
            var (database, connString) = options;
            Build(connString);
            return database switch
            {
                DatabaseType.SqlServer => With.SqlServer,
                DatabaseType.MySql => With.MySql,
                DatabaseType.Postgre => With.Postgre,
                DatabaseType.Sqlite => With.Sqlite,

                _ => throw new ArgumentOutOfRangeException(nameof(database), database, null)
            };
        }

        public static IIdentityServerBuilder ConfigureContext(this IIdentityServerBuilder builder, (DatabaseType, string) options)
        {
            var (databaseType, connectionString) = options;

            Build(connectionString);
            return databaseType switch
            {
                DatabaseType.SqlServer => builder.OAuth2Store(With.SqlServer),
                DatabaseType.MySql => builder.OAuth2Store(With.MySql),
                DatabaseType.Postgre => builder.OAuth2Store(With.Postgre),
                DatabaseType.Sqlite => builder.OAuth2Store(With.Sqlite),
                _ => builder.AddConfigurationStoreCache()
            };
        }

    }
}