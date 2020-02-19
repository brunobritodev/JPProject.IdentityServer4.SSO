using IdentityServer4;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Contrib.AspNetCore.Testing.Builder;
using IdentityServer4.Contrib.AspNetCore.Testing.Configuration;
using IdentityServer4.Contrib.AspNetCore.Testing.Services;
using IdentityServer4.Models;
using Jp.Api.Management;
using Jp.Database.Context;
using JPProject.Api.Management.Tests.Infra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace JPProject.Api.Management.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<StartupTest>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            ConfigureIdentityServer();

            builder.ConfigureServices(services =>
            {
                // Add a database context (AppDbContext) using an in-memory database for testing.
                void DatabaseOptions(DbContextOptionsBuilder opt) => opt.UseInMemoryDatabase("JpTests").EnableSensitiveDataLogging();


                services.AddDbContext<SsoContext>(DatabaseOptions);
                services
                    .ConfigureJpAdmin<AspNetUserTest>()
                    .AddEventStore<SsoContext>()
                    .AddAdminContext(DatabaseOptions);

                services.PostConfigureAll<IdentityServerAuthenticationOptions>(options =>
                {
                    options.Authority = "http://localhost";
                    options.JwtBackChannelHandler = IdentityServerClient.IdentityServer.CreateHandler();
                });


            }).UseStartup<StartupTest>();

        }

        private void ConfigureIdentityServer()
        {

            var clientConfiguration = new ClientConfiguration("TestClient", "MySecret");

            var client = new Client
            {
                ClientId = clientConfiguration.Id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(clientConfiguration.Secret.Sha256())
                },
                AllowedScopes = new[] { "jp_api.user", "jp_api.is4" },
                AllowedGrantTypes = new[] { GrantType.ClientCredentials, GrantType.ResourceOwnerPassword },
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 7200
            };

            var webHostBuilder = new IdentityServerHostBuilder()
                .AddClients(client)
                .AddApiResources(new ApiResource
                {
                    Name = "jp_api",
                    DisplayName = "JP API",
                    Description = "OAuth2 Server Management Api",
                    ApiSecrets = { new Secret("Q&tGrEQMypEk.XxPU:%bWDZMdpZeJiyMwpLv4F7d**w9x:7KuJ#fy,E8KPHpKz++".Sha256()) },

                    UserClaims =
                                    {
                                        IdentityServerConstants.StandardScopes.OpenId,
                                        IdentityServerConstants.StandardScopes.Profile,
                                        IdentityServerConstants.StandardScopes.Email,
                                        "is4-rights",
                                        "username",
                                        "roles"
                                    },

                    Scopes =
                                    {
                                        new Scope()
                                        {
                                            Name = "jp_api.user",
                                            DisplayName = "User Management - Full access",
                                            Description = "Full access to User Management",
                                            Required = true
                                        },
                                        new Scope()
                                        {
                                            Name = "jp_api.is4",
                                            DisplayName = "OAuth2 Server",
                                            Description = "Manage mode to IS4",
                                            Required = true
                                        }
                                    }
                })
                .UseResourceOwnerPasswordValidator(new SimpleResourceOwnerPasswordValidator())
                .CreateWebHostBuider();

            IdentityServerClient = new IdentityServerProxy(webHostBuilder);


        }

        public IdentityServerProxy IdentityServerClient { get; set; }
    }
}
