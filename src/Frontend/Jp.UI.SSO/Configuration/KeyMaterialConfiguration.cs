using Jp.Database.Context;
using Jwks.Manager;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Jp.UI.SSO.Configuration
{
    public static class KeyMaterialConfiguration
    {
        /// <summary>
        /// Setup the Key Material for IdentityServer4
        /// Unless you know what you are doing, change it.
        /// </summary>
        public static IIdentityServerBuilder SetupKeyMaterial(this IIdentityServerBuilder builder)
        {
            // ASP.NET works well into Azure Scale
            // But if you go to Kubernetes, docker swarm or even nginx lb scenarios you'll get problems with Unprocted ticket failed every time a second instance start
            // The same for IdentityServer4 default config. By default it don't support a second instance, because jwks will change.
            // These components prevents it.

            // For more options of key material, look at SigingCredentialExtension
            builder.Services.AddDataProtection().SetApplicationName("sso").PersistKeysToDbContext<SsoContext>();
            
            builder.Services.AddJwksManager(options =>
            {
                // ES256 is the recommended by RFC
                // https://tools.ietf.org/html/rfc7518#section-3.1
                options.Algorithm = Algorithm.ES256;
                options.DaysUntilExpire = 90;
            }).IdentityServer4AutoJwksManager().PersistKeysToDatabaseStore<SsoContext>();
            return builder;

        }
    }
}
