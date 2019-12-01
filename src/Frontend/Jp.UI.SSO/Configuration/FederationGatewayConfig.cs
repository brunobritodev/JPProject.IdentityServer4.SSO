using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jp.UI.SSO.Configuration
{
    public static class SocialIntegrationConfig
    {
        public static IServiceCollection AddFederationGateway(this IServiceCollection services,
            IConfiguration configuration)
        {
            var authBuilder = services.AddAuthentication();


            if (configuration.GetSection("ExternalLogin:Google").Exists())
            {
                authBuilder.AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = configuration.GetValue<string>("ExternalLogin:Google:ClientId");
                    options.ClientSecret = configuration.GetValue<string>("ExternalLogin:Google:ClientSecret");
                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = context =>
                        {
                            if (context.User.GetRawText().Contains("picture"))
                                context.Identity.AddClaim(new Claim(JwtClaimTypes.Picture, context.User.GetProperty("picture").GetString()));
                            return Task.CompletedTask;
                        }
                    };
                });
            }

            if (configuration.GetSection("ExternalLogin:Facebook").Exists())
            {
                authBuilder.AddFacebook("Facebook", options =>
                    {
                        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                        options.ClientId = configuration.GetValue<string>("ExternalLogin:Facebook:ClientId");
                        options.ClientSecret = configuration.GetValue<string>("ExternalLogin:Facebook:ClientSecret");
                        options.Fields.Add("picture");
                        options.Events = new OAuthEvents
                        {
                            OnCreatingTicket = context =>
                            {
                                if (context.User.GetRawText().Contains("picture"))
                                    context.Identity.AddClaim(new Claim(JwtClaimTypes.Picture, context.User.GetProperty("picture").GetProperty("data").GetProperty("url").GetString()));
                                return Task.CompletedTask;
                            }
                        };
                    });
            }

            return services;
        }
    }
}
