using System;
using Jp.Database.Context;
using JPProject.EntityFrameworkCore.Context;
using JPProject.Sso.Domain.Interfaces;
using JPProject.Sso.Infra.Data.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jp.Database
{
    /// <summary>
    /// SqlServer configuration
    /// </summary>
    public static class ContextConfiguration
    {

        /// <summary>
        /// ASP.NET Identity Context config
        /// </summary>
        public static ISsoConfigurationBuilder SsoStore(this ISsoConfigurationBuilder builder, Action<DbContextOptionsBuilder> databaseConfig)
        {
            builder.Services.AddSsoContext(databaseConfig);
            // Add a DbContext to store Keys. SigningCredentials and DataProtectionKeys
            builder.Services.AddDbContext<AspNetGeneralContext>(databaseConfig);
            return builder;
        }

        /// <summary>
        /// Eventstore config
        /// </summary>
        public static ISsoConfigurationBuilder EventStore(this ISsoConfigurationBuilder services, Action<DbContextOptionsBuilder> databaseConfig)
        {
            services.Services.AddDbContext<EventStoreContext>(databaseConfig);
            return services;
        }

        /// <summary>
        /// IdentityServer4 context config
        /// </summary>
        public static IIdentityServerBuilder OAuth2Store(this IIdentityServerBuilder builder, Action<DbContextOptionsBuilder> databaseConfig)
        {
            builder.AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = databaseConfig;
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = databaseConfig;
                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    //options.TokenCleanupInterval = 15; // frequency in seconds to cleanup stale grants. 15 is useful during debugging
                }).AddConfigurationStoreCache();

            return builder;
        }

        public static ISsoConfigurationBuilder PersistKeys(this ISsoConfigurationBuilder services, Action<DbContextOptionsBuilder> databaseConfig)
        {
            services.Services.AddDbContext<AspNetGeneralContext>(databaseConfig);
            return services;
        }

    }
}
