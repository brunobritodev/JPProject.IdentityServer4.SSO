using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
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
                    options.SignInScheme = IdentityConstants.ExternalScheme;
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
                        options.SignInScheme = IdentityConstants.ExternalScheme;
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

            if (configuration.GetSection("ExternalLogin:OpenIdConnect").Exists())
            {
                authBuilder
                    .AddCookie((options) => {})
                    .AddOpenIdConnect("OpenIdConnect", options =>
                    {
                        options.SignInScheme = IdentityConstants.ExternalScheme; //ExternalCookieAuthenticationScheme Cannot Handle Large Cookies Update according to : https://github.com/IdentityServer/IdentityServer4.Templates/pull/23/files

                        options.Authority = configuration.GetValue<string>("ExternalLogin:OpenIdConnect:Authority");
                        options.ClientId = configuration.GetValue<string>("ExternalLogin:OpenIdConnect:ClientId");
                        options.ClientSecret = configuration.GetValue<string>("ExternalLogin:OpenIdConnect:ClientSecret");

                        if (configuration.GetSection("ExternalLogin:OpenIdConnect:CallbackPath").Exists())
                            options.CallbackPath = configuration.GetValue<string>("ExternalLogin:OpenIdConnect:CallbackPath");

                        if (configuration.GetSection("ExternalLogin:OpenIdConnect:ResponseType").Exists())
                            options.ResponseType = configuration.GetValue<string>("ExternalLogin:OpenIdConnect:ResponseType");
                        
                        if (configuration.GetSection("ExternalLogin:OpenIdConnect:GetClaimsFromUserInfoEndpoint").Exists())
                            options.GetClaimsFromUserInfoEndpoint = configuration.GetValue<bool>("ExternalLogin:OpenIdConnect:GetClaimsFromUserInfoEndpoint");
                        
                        if (configuration.GetSection("ExternalLogin:OpenIdConnect:RequireHttpsMetadata").Exists())
                            options.RequireHttpsMetadata = configuration.GetValue<bool>("ExternalLogin:OpenIdConnect:RequireHttpsMetadata");
                        
                        if (configuration.GetSection("ExternalLogin:OpenIdConnect:SaveTokens").Exists())
                            options.SaveTokens = configuration.GetValue<bool>("ExternalLogin:OpenIdConnect:SaveTokens");
                        
                        if (configuration.GetSection("ExternalLogin:OpenIdConnect:SignedOutRedirectUri").Exists())
                            options.SignedOutRedirectUri = configuration.GetValue<string>("ExternalLogin:OpenIdConnect:SignedOutRedirectUri");

                        options.Scope.Clear();
                        options.Scope.Add("openid");
                        if (configuration.GetSection("ExternalLogin:OpenIdConnect:Scopes").Exists())
                        {
                            foreach (var s in configuration.GetValue<string>("ExternalLogin:OpenIdConnect:Scopes").Split(' '))
                            {
                                if (!options.Scope.Contains(s))
                                    options.Scope.Add(s);
                            }
                        }
                    });
            }

            return services;
        }
    }
}
