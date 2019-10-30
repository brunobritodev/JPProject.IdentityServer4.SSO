using AutoMapper;
using JPProject.Sso.Application.AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Jp.Management.Configuration
{
    public static class AutoMapperSetup
    {
        public static void AddAutoMapperSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var mappings = SsoMapperConfig.RegisterMappings();
            mappings.AddProfile<CustomMappingProfile>();
            var automapperConfig = new MapperConfiguration(mappings);

            services.TryAddSingleton(automapperConfig.CreateMapper());
        }
    }
}