using IdentityServer4.EntityFramework.Entities;
using Jp.Database.Context;
using JPProject.EntityFrameworkCore.MigrationHelper;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Jp.Api.Management.Configuration
{
    public class DatabaseChecker
    {
        public static async Task EnsureDatabaseIsReady(IServiceScope serviceScope)
        {
            var services = serviceScope.ServiceProvider;
            var ssoContext = services.GetRequiredService<SsoContext>();

            Log.Information("Testing conection with database");
            await DbHealthChecker.TestConnection(ssoContext);
            Log.Information("Connection successfull");


            Log.Information("Check if database contains Client (ConfigurationDbStore) table");
            if (!await DbHealthChecker.CheckTableExists<Client>(ssoContext))
                throw new Exception("IdentityServer4 database doesn't exist or wrong connection string");

            Log.Information("Check if database contains PersistedGrant (PersistedGrantDbStore) table");
            if (!await DbHealthChecker.CheckTableExists<PersistedGrant>(ssoContext))
                throw new Exception("IdentityServer4 database doesn't exist or wrong connection string");
            Log.Information("Checks done");
        }
    }
}
