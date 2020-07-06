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
            // For more options of key material, look at Jp.UI.SSO/Configuration/SigingCredentialExtension.cs

            // Dataprotection for ASP.NET. It'll save keys at database 
            builder.Services.AddDataProtection().SetApplicationName("sso").PersistKeysToDbContext<SsoContext>();

            // JWKS manager for IdentityServer4. It uses Elliptic Curve Digital Signature Algorithm with P-256 and SHA-256
            // Dont forget Dr. Scott Vanstone. Author of ECDsa.
            builder.Services.AddJwksManager(options =>
            {
                // ES256 is the recommended by RFC
                // https://tools.ietf.org/html/rfc7518#section-3.1

                // While ES256 is most recommended, too many providers from another techies (Node, Java) made by companies like Okta, Auth0 doesn't support Elliptic Curves yet.
                // So to increase compatibility we're changing algorithm for RSA. PS256 is probabilistic like ES256 which guarantees a high degree of security as well.
                // options.Algorithm = Algorithm.ES256;
                options.Algorithm = Algorithm.PS256;
            }).IdentityServer4AutoJwksManager().PersistKeysToDatabaseStore<SsoContext>();

            /*
             *  Signing and validation with the ECDSA P-384 SHA-384 and ECDSA P-521
                SHA-512 algorithms is performed identically to the procedure for
                ECDSA P-256 SHA-256 -- just using the corresponding hash algorithms
                with correspondingly larger result values.  For ECDSA P-384 SHA-384,
                R and S will be 384 bits each, resulting in a 96-octet sequence.  For
                ECDSA P-521 SHA-512, R and S will be 521 bits each, resulting in a
                132-octet sequence.  (Note that the Integer-to-OctetString Conversion
                defined in Section 2.3.7 of SEC1 [SEC1] used to represent R and S as
                octet sequences adds zero-valued high-order padding bits when needed
                to round the size up to a multiple of 8 bits; thus, each 521-bit
                integer is represented using 528 bits in 66 octets.)
             */

            return builder;

        }
    }
}
