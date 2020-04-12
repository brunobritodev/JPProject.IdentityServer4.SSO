﻿using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using IdentityModel;

namespace Jp.Api.Management.Configuration
{
    public static class AuthenticationConfig
    {
        public static void ConfigureOAuth2Server(this IServiceCollection services, IConfiguration configuration)
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(o =>
                    {
                        o.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
                        o.DefaultAuthenticateScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
                    })
                    .AddIdentityServerAuthentication(options =>
                    {
                        options.Authority = configuration.GetValue<string>("ApplicationSettings:Authority");
                        options.RequireHttpsMetadata = false;
                        options.ApiSecret = "Q&tGrEQMypEk.XxPU:%bWDZMdpZeJiyMwpLv4F7d**w9x:7KuJ#fy,E8KPHpKz++";
                        options.ApiName = "jp_api";
                        options.RoleClaimType = JwtClaimTypes.Role;
                        options.NameClaimType = JwtClaimTypes.Name;
                    });
        }
    }
}
