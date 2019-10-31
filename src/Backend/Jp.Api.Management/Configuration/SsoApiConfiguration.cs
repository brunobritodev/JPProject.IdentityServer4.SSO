using AutoMapper;
using AutoMapper.Configuration;
using JPProject.Admin.Application.AutoMapper;
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
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Jp.Api.Management.Configuration
{
    public static class SsoApiConfiguration
    {
        public static IServiceCollection ConfigureSsoApi(this IServiceCollection services, IConfiguration configuration)
        {
            var database = configuration["ApplicationSettings:DatabaseType"].ToUpper();
            var connString = configuration.GetConnectionString("SSOConnection");
            var userIdentityBuilder = services.ConfigureUserIdentity<AspNetUser>();
            var identityServerApiBuilder = services.ConfigureJpAdmin<AspNetUser>();

            switch (database)
            {
                case "MYSQL":
                    userIdentityBuilder.WithMySql<Startup>(connString).AddEventStoreMySql<Startup>(connString, EventStoreMigrationOptions.Get().ShouldMigrate(false));
                    identityServerApiBuilder.WithMySql<Startup>(connString);
                    break;
                case "SQLSERVER":
                    userIdentityBuilder.WithSqlServer<Startup>(connString).AddEventStoreSqlServer<Startup>(connString, EventStoreMigrationOptions.Get().ShouldMigrate(false));
                    identityServerApiBuilder.WithSqlServer<Startup>(connString);

                    break;
                case "POSTGRESQL":
                    userIdentityBuilder.WithPostgreSql<Startup>(connString).AddEventStorePostgreSql<Startup>(connString, EventStoreMigrationOptions.Get().ShouldMigrate(false));
                    identityServerApiBuilder.WithPostgreSql<Startup>(connString);
                    break;
                case "SQLITE":
                    userIdentityBuilder.WithSqlite<Startup>(connString).AddEventStoreSqlite<Startup>(connString, EventStoreMigrationOptions.Get().ShouldMigrate(false));
                    identityServerApiBuilder.WithSqlite<Startup>(connString);
                    break;
            }
            return services;
        }

        public static void ConfigureDefaultSettings(this IServiceCollection services)
        {
            var configurationExpression = new MapperConfigurationExpression();
            AdminUiMapperConfiguration.RegisterMappings().ForEach(p => configurationExpression.AddProfile(p));
            SsoMapperConfig.RegisterMappings().ForEach(p => configurationExpression.AddProfile(p));
            configurationExpression.AddProfile(new CustomMappingProfile());
            var automapperConfig = new MapperConfiguration(configurationExpression);

            services.TryAddSingleton(automapperConfig.CreateMapper());
            // Adding MediatR for Domain Events and Notifications
            services.AddMediatR(typeof(Startup));
        }
    }
}
