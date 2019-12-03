using AutoMapper;
using AutoMapper.Configuration;
using JPProject.Admin.Application.AutoMapper;
using JPProject.Admin.Database;
using JPProject.AspNet.Core;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.Application.AutoMapper;
using JPProject.Sso.Database;
using JPProject.Sso.Infra.Identity.Models.Identity;
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
            var database = configuration.GetValue<DatabaseType>("ApplicationSettings:DatabaseType");
            var connString = configuration.GetConnectionString("SSOConnection");

            services.ConfigureUserIdentity<AspNetUser>().AddDatabase(database, connString);
            services.ConfigureJpAdmin<AspNetUser>().AddDatabase(database, connString);
            services.UpgradePasswordSecurity().UseArgon2<UserIdentity>();

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
