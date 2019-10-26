using Jp.Domain.Core.Events;
using Jp.Domain.Interfaces;
using Jp.Infra.Data.Context;
using Jp.Infra.Data.EventSourcing;
using Jp.Infra.Data.Repository.EventSourcing;
using Jp.Infra.Data.UoW;
using Microsoft.Extensions.DependencyInjection;

namespace Jp.Infra.CrossCutting.IoC
{
    internal class RepositoryBootStrapper
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Infra - Data EventSourcing
            services.AddScoped<IEventStoreRepository, EventStoreRepository>();
            services.AddScoped<IEventStore, SqlEventStore>();
            services.AddScoped<EventStoreContext>();
        }
    }
}
