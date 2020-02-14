using AutoMapper;
using AutoMapper.Configuration;
using Jp.Database;
using JPProject.AspNet.Core;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.Application.AutoMapper;
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
            var database = configuration.GetValue<DatabaseType>("ApplicationSettings:DatabaseType");
            var connString = configuration.GetConnectionString("SSOConnection");

            services
                .ConfigureUserIdentity<AspNetUser>()
                .ConfigureContext(database, connString)
                .AddCustomClaimsFactory<ApplicationClaimsIdentityFactory>()
                .ConfigureIdentityServer()

                .AddSigninCredentialFromConfig(configuration.GetSection("CertificateOptions"))
                .ConfigureOAuth2Context(database, connString);

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
