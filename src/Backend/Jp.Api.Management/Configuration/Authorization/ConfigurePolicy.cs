using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Jp.Api.Management.Configuration.Authorization
{
    public static class ConfigurePolicy
    {
        public static void AddPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin",
                    policy => policy.RequireAssertion(c =>
                        c.User.HasClaim("is4-rights", "manager") ||
                        c.User.IsInRole("Administrator")));

                options.AddPolicy("ReadOnly", policy =>
                    policy.RequireAssertion(context => context.User.IsAuthenticated()));

                options.AddPolicy("UserManagement", policy =>
                    policy.RequireAuthenticatedUser());

                options.AddPolicy("Default",
                    policy => policy.Requirements.Add(new AccountRequirement()));
            });
            services.AddSingleton<IAuthorizationHandler, AccountRequirementHandler>();
        }
    }
}
