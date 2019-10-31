using Jp.Api.Management.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jp.Api.Management
{
    public class StartupTest : Startup
    {
        public StartupTest(IConfiguration configuration) : base(configuration)
        {
        }

        public override void ConfigureSso(IServiceCollection services)
        {
            services.ConfigureDefaultSettings();
        }
    }
}