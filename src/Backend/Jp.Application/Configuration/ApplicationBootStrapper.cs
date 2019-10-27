using Jp.Application.Interfaces;
using Jp.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Jp.Application.Configuration
{
    internal class ApplicationBootStrapper
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IUserAppService, UserAppService>();
            services.AddScoped<IUserManageAppService, UserManagerAppService>();
            services.AddScoped<IRoleManagerAppService, RoleManagerAppService>();
        }
    }
}
