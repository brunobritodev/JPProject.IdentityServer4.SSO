using AutoMapper;
using AutoMapper.Configuration;
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
                        .WithMySql<Startup>(connString)
                        .AddSigninCredentialFromConfig(configuration.GetSection("CertificateOptions"));
                    break;
                case "SQLSERVER":
                    services
                        .ConfigureUserIdentity<AspNetUser>()
                        .WithSqlServer<Startup>(connString)
                        .AddEventStoreSqlServer<Startup>(connString, eventstoreOptions)
                        .ConfigureIdentityServer()
                        .WithSqlServer<Startup>(connString)
                        .AddSigninCredentialFromConfig(configuration.GetSection("CertificateOptions"));
                    break;
                case "POSTGRESQL":
                    services
                        .ConfigureUserIdentity<AspNetUser>()
                        .WithPostgreSql<Startup>(connString)
                        .AddEventStorePostgreSql<Startup>(connString, eventstoreOptions)
                        .ConfigureIdentityServer()
                        .WithPostgreSql<Startup>(connString)
                        .AddSigninCredentialFromConfig(configuration.GetSection("CertificateOptions"));
                    break;
                case "SQLITE":
                    services
                        .ConfigureUserIdentity<AspNetUser>()
                        .WithSqlite<Startup>(connString)
                        .AddEventStoreSqlite<Startup>(connString, eventstoreOptions)
                        .ConfigureIdentityServer()
                        .WithSqlite<Startup>(connString)
                        .AddSigninCredentialFromConfig(configuration.GetSection("CertificateOptions"));
                    break;
            }

            var configurationExpression = new MapperConfigurationExpression();
            SsoMapperConfig.RegisterMappings().ForEach(p => configurationExpression.AddProfile(p));
            configurationExpression.AddProfile(new CustomMappingProfile());
            var automapperConfig = new MapperConfiguration(configurationExpression);

            services.TryAddSingleton(automapperConfig.CreateMapper());
            // Adding MediatR for Domain Events and Notifications
            services.AddMediatR(typeof(Startup));
        }
    }
}
