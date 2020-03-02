using Jp.Database.Context;
using JPProject.AspNet.Core;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.AspNetIdentity.Configuration;
using JPProject.Sso.AspNetIdentity.Models.Identity;
using JPProject.Sso.EntityFramework.Repository.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Threading.Tasks;
using static Microsoft.Extensions.Configuration.ProviderSelector;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Jp.Api.Management.Configuration
{
    public static class SsoApiConfiguration
    {
        public static IServiceCollection ConfigureSsoApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureProviderForContext<SsoContext>(DetectDatabase(configuration));


            //// ASP.NET Identity Configuration
            services
                .AddIdentity<UserIdentity, RoleIdentity>(AccountOptions.NistAccountOptions)
                .AddEntityFrameworkStores<SsoContext>()
                .AddDefaultTokenProviders(); ;

            //// SSO Services
            services
                .ConfigureSso<AspNetUser>()
                .AddSsoContext<SsoContext>()
                .AddDefaultAspNetIdentityServices();

            //// IdentityServer4 Admin services
            services
                .ConfigureJpAdminServices<AspNetUser>()
                .ConfigureJpAdminStorageServices()
                .SetupDefaultIdentityServerContext<SsoContext>();


            services.UpgradePasswordSecurity().UseArgon2<UserIdentity>();

            SetupGeneralAuthorizationSettings(services);

            return services;
        }


        private static void SetupGeneralAuthorizationSettings(IServiceCollection services)
        {
            services.ConfigureApplicationCookie(options =>
            {
                //options.AccessDeniedPath = new PathString("/accounts/access-denied");
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Task.CompletedTask;
                };
            });
        }
        /// <summary>
        /// it's just a tuple. Returns 2 parameters.
        /// Trying to improve readability at ConfigureServices
        /// </summary>
        private static (DatabaseType, string) DetectDatabase(IConfiguration configuration) => (
            configuration.GetValue<DatabaseType>("ApplicationSettings:DatabaseType"),
            configuration.GetConnectionString("SSOConnection"));
    }
}
