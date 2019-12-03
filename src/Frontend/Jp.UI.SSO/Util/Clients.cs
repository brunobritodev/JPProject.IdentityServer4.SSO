using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Jp.UI.SSO.Util

{
    public static class Clients
    {

        public static IEnumerable<Client> GetAdminClient(IConfiguration configuration)
        {

            return new List<Client>
            {
                /*
                 * JP Project ID4 Admin Client
                 */
                new Client
                {

                    ClientId = "IS4-Admin",
                    ClientName = "IS4-Admin",
                    ClientUri = configuration["ApplicationSettings:IS4AdminUi"],
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = new[] {
                        $"{configuration["ApplicationSettings:IS4AdminUi"]}/login-callback",
                        $"{configuration["ApplicationSettings:IS4AdminUi"]}/silent-refresh.html"
                    },
                    AllowedCorsOrigins = { configuration.GetValue<string>("ApplicationSettings:IS4AdminUi")},
                    IdentityTokenLifetime = 3600,
                    LogoUri = "https://jpproject.blob.core.windows.net/images/jplogo.png",
                    AuthorizationCodeLifetime = 3600,
                    PostLogoutRedirectUris = {$"{configuration["ApplicationSettings:IS4AdminUi"]}",},
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "role",
                        "jp_api.is4"
                    }
                },

                /*
                 * User Management Client - OpenID Connect implicit flow client
                 */
                new Client {
                    ClientId = "UserManagementUI",
                    ClientName = "User Management UI",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = true,
                    RedirectUris =new[] {
                        $"{configuration["ApplicationSettings:UserManagementURL"]}/login-callback",
                        $"{configuration["ApplicationSettings:UserManagementURL"]}/silent-refresh.html"
                    },
                    AllowedCorsOrigins = { configuration["ApplicationSettings:UserManagementURL"] },
                    PostLogoutRedirectUris =  { $"{configuration["ApplicationSettings:UserManagementURL"]}" },
                    LogoUri = "https://jpproject.blob.core.windows.net/images/usermanagement.jpg",
                    IdentityTokenLifetime = 3600,
                    AuthorizationCodeLifetime = 3600,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "jp_api.user",
                    }
                },
                /*
                 * Swagger
                 */
                new Client
                {
                    ClientId = "Swagger",
                    ClientName = "Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    LogoUri = "https://jpproject.blob.core.windows.net/images/swagger.png",
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris =
                    {
                        $"{configuration.GetValue<string>("ApplicationSettings:ResourceServerURL")}/swagger/oauth2-redirect.html"
                    },
                    AllowedScopes =
                    {
                        "jp_api.user",
                        "jp_api.is4",
                    }
                }

            };

        }
    }
}