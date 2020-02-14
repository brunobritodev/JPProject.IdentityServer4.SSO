using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using static Jp.Database.ProviderConfiguration;

namespace Jp.Database
{
    public static class ProviderSelector
    {
        public static ISsoConfigurationBuilder ConfigureContext(
            this ISsoConfigurationBuilder services,
            DatabaseType database,
            string connString)
        {
            Build(connString);
            return database switch
            {
                DatabaseType.SqlServer => services.SsoStore(With.SqlServer)
                                                  .EventStore(With.SqlServer)
                                                  .PersistKeys(With.SqlServer),

                DatabaseType.MySql => services.SsoStore(With.MySql)
                                              .EventStore(With.MySql)
                                              .PersistKeys(With.MySql),

                DatabaseType.Postgre => services.SsoStore(With.Postgre)
                                                .EventStore(With.Postgre)
                                                .PersistKeys(With.Postgre),

                DatabaseType.Sqlite => services.SsoStore(With.Sqlite)
                                               .EventStore(With.Sqlite)
                                               .PersistKeys(With.Sqlite),

                _ => throw new ArgumentOutOfRangeException(nameof(database), database, null)
            };
        }

        public static IIdentityServerBuilder ConfigureOAuth2Context(
            this IIdentityServerBuilder builder,
            DatabaseType database,
            string connString)
        {
            Build(connString);
            return database switch
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