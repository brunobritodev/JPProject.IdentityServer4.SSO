using Jp.Database.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Security.Cryptography.X509Certificates;
using Jwks.Manager;

namespace Jp.UI.SSO.Configuration
{
    /// <summary>
    /// Impl of adding a signin key for identity server 4,
    /// with an appsetting.json configuration look similar to:
    /// "SigninKeyCredentials": {
    ///     "KeyType": "KeyFile",
    ///     "FileName": "C:\\certificates\\idsv4.pfx",
    ///     "KeyStorePath": ""
    /// }
    /// </summary>
    public static class SigninCredentialExtension
    {
        private const string File = nameof(File);
        private const string Store = nameof(Store);
        private const string Temporary = nameof(Temporary);
        private const string Environment = nameof(Environment);

        private const string FileName = nameof(FileName);
        private const string FilePassword = nameof(FilePassword);
        private const string CertificateThumbprint = nameof(CertificateThumbprint);
        private const string Auto = nameof(Auto);

        public static IIdentityServerBuilder AddSigninCredentialFromConfig(this IIdentityServerBuilder builder,
            IConfigurationSection options, IWebHostEnvironment env)
        {
            string keyType = options.GetValue<string>("Type");

            switch (keyType)
            {
                case Temporary:
                    builder.AddDeveloperSigningCredential(false);
                    break;
                case File:
                    AddCertificateFromFile(builder, options);
                    break;
                case Store:
                    AddCertificateFromStore(builder, options);
                    break;
                case Environment:
                    AddCertificateFromEnvironment(builder);
                    break;
                case Auto:
                    AddAutoCertificateManagement(builder, env);
                    break;
            }
            return builder;
        }

        private static void AddCertificateFromEnvironment(IIdentityServerBuilder builder)
        {
            Log.Information("Key Managemet - Environment Store");
            var file = System.Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
            var password = System.Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password");
            if (System.IO.File.Exists(file))
            {
                builder.AddSigningCredential(new X509Certificate2(file, password, X509KeyStorageFlags.MachineKeySet));
            }
        }

        private static void AddCertificateFromStore(IIdentityServerBuilder builder,
            IConfigurationSection options)
        {
            Log.Information("Key Managemet - Certificate Store");
            var keyIssuer = options.GetValue<string>(CertificateThumbprint);

            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, keyIssuer, false);

            if (certificates.Count > 0)
                builder.AddSigningCredential(certificates[0]);
        }

        private static void AddAutoCertificateManagement(IIdentityServerBuilder builder, IWebHostEnvironment env)
        {
            Log.Information("Key Managemet - Auto");

            builder.Services.AddJwksManager().IdentityServer4AutoJwksManager().PersistKeysToDatabaseStore<SsoContext>();
        }

        private static void AddCertificateFromFile(IIdentityServerBuilder builder,
            IConfigurationSection options)
        {
            Log.Information("Key Managemet - File");
            var keyFileName = options.GetValue<string>(FileName);
            var keyFilePassword = options.GetValue<string>(FilePassword);

            if (System.IO.File.Exists(keyFileName))
            {
                builder.AddSigningCredential(new X509Certificate2(keyFileName, keyFilePassword));
            }
        }
    }
}
