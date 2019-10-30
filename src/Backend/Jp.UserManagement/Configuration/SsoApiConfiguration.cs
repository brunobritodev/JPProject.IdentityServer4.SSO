using AutoMapper;
using JPProject.Admin.Application.AutoMapper;
using JPProject.AspNet.Core;
using JPProject.Sso.Application.AutoMapper;
using JPProject.Sso.EntityFrameworkCore.MySql.Configuration;
using JPProject.Sso.EntityFrameworkCore.PostgreSQL.Configuration;
using JPProject.Sso.EntityFrameworkCore.Sqlite.Configuration;
using JPProject.Sso.EntityFrameworkCore.SqlServer.Configuration;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jp.Management.Configuration
{
    public static class SsoApiConfiguration
    {
        public static void ConfigureSsoApi(this IServiceCollection services, IConfiguration configuration)
        {
            var database = configuration["ApplicationSettings:DatabaseType"].ToUpper();
            var connString = configuration.GetConnectionString("SSOConnection");
            var ssoBuilder = services.ConfigureUserIdentity<AspNetUser>();
            var builder = services.ConfigureJpAdmin<AspNetUser>();

            switch (database)
            {
                case "MYSQL":
                    ssoBuilder.WithMySql<Startup>(connString).AddEventStoreMySql<Startup>(connString);
                    builder.WithMySql<Startup>(connString);
                    break;
                case "SQLSERVER":
                    ssoBuilder.WithSqlServer<Startup>(connString).AddEventStoreSqlServer<Startup>(connString);
                    builder.WithSqlServer<Startup>(connString);

                    break;
                case "POSTGRESQL":
                    ssoBuilder.WithPostgreSql<Startup>(connString).AddEventStorePostgreSql<Startup>(connString);
                    builder.WithPostgreSql<Startup>(connString);
                    break;
                case "SQLITE":
                    ssoBuilder.WithSqlite<Startup>(connString).AddEventStoreSqlite<Startup>(connString);
                    builder.WithSqlite<Startup>(connString);
                    break;
            }


            var mappings = AdminUiMapperConfiguration.RegisterMappings();
            mappings.AddProfile(SsoMapperConfig.RegisterMappings());

            var automapperConfig = new MapperConfiguration(mappings);


            services.TryAddSingleton(automapperConfig.CreateMapper());
            // Adding MediatR for Domain Events and Notifications
            services.AddMediatR(typeof(Startup));
        }
    }
}
