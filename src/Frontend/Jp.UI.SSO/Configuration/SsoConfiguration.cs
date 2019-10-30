using AutoMapper;
using JPProject.AspNet.Core;
using JPProject.EntityFrameworkCore.Configuration;
using JPProject.Sso.Application.AutoMapper;
using JPProject.Sso.EntityFrameworkCore.MySql.Configuration;
using JPProject.Sso.EntityFrameworkCore.PostgreSQL.Configuration;
using JPProject.Sso.EntityFrameworkCore.Sqlite.Configuration;
using JPProject.Sso.EntityFrameworkCore.SqlServer.Configuration;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jp.UI.SSO.Configuration
{
    public static class SsoConfiguration
    {
        public static void ConfigureSso(this IServiceCollection services, IConfiguration configuration)
        {
            var database = configuration["ApplicationSettings:DatabaseType"].ToUpper();
            var connString = configuration.GetConnectionString("SSOConnection");
            var eventstoreOptions = EventStoreMigrationOptions.Get().ShouldMigrate(false);
            switch (database)
            {
                case "MYSQL":
                    services
                        .ConfigureUserIdentity<AspNetUser>()
                        .WithMySql<Startup>(connString)
                        .AddEventStoreMySql<Startup>(connString, eventstoreOptions)
                        .ConfigureIdentityServer()
                        .WithMySql<Startup>(connString);
                    break;
                case "SQLSERVER":
                    services
                        .ConfigureUserIdentity<AspNetUser>()
                        .WithSqlServer<Startup>(connString)
                        .AddEventStoreSqlServer<Startup>(connString, eventstoreOptions)
                        .ConfigureIdentityServer()
                        .WithSqlServer<Startup>(connString);

                    break;
                case "POSTGRESQL":
                    services
                        .ConfigureUserIdentity<AspNetUser>()
                        .WithPostgreSql<Startup>(connString)
                        .AddEventStorePostgreSql<Startup>(connString, eventstoreOptions)
                        .ConfigureIdentityServer()
                        .WithPostgreSql<Startup>(connString);
                    break;
                case "SQLITE":
                    services
                        .ConfigureUserIdentity<AspNetUser>()
                        .WithSqlite<Startup>(connString)
                        .AddEventStoreSqlite<Startup>(connString, eventstoreOptions)
                        .ConfigureIdentityServer()
                        .WithSqlite<Startup>(connString);
                    break;
            }

            var mappings = SsoMapperConfig.RegisterMappings();
            mappings.AddProfile(SsoMapperConfig.RegisterMappings());

            var automapperConfig = new MapperConfiguration(mappings);


            services.TryAddSingleton(automapperConfig.CreateMapper());
            // Adding MediatR for Domain Events and Notifications
            services.AddMediatR(typeof(Startup));
        }
    }
}
