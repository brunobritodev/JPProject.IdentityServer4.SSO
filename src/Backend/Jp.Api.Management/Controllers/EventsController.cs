using JPProject.Admin.Application.Interfaces;
using JPProject.Domain.Core.Bus;
using JPProject.Domain.Core.Interfaces;
using JPProject.Domain.Core.Notifications;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.Application.EventSourcedNormalizers;
using JPProject.Sso.Application.Interfaces;
using JPProject.Sso.Application.ViewModels.EventsViewModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jp.Api.Management.Controllers
{

    [Route("events"), Authorize(Policy = "Default")]
    public class EventsController : ApiController
    {
        private readonly IEventStoreAppService _eventStoreAppService;
        private readonly IIdentityServerEventStore _identityServerEventStore;
        private readonly ISystemUser _user;

        public EventsController(
            INotificationHandler<DomainNotification> notifications,
            IMediatorHandler mediator,
            IEventStoreAppService eventStoreAppService,
            IIdentityServerEventStore identityServerEventStore,
            ISystemUser user) : base(notifications, mediator)
        {
            _eventStoreAppService = eventStoreAppService;
            _identityServerEventStore = identityServerEventStore;
            _user = user;
        }

        /// <summary>
        /// Get a list of events by some aggregate
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("")]
        public ActionResult<ListOf<EventHistoryData>> ShowLogs([FromQuery] SearchEventByAggregate query)
        {
            var clients = _eventStoreAppService.GetEvents(query);

            if (!User.IsInRole("Administrator") && !User.HasClaim(c => c.Type == "is4-manager") && query.Aggregate != _user.Username)
            {
                foreach (var client in clients.Collection)
                {
                    client.MarkAsSensitiveData();
                }
            }

            return ResponseGet(clients);
        }

        /// <summary>
        /// Get a list aggregates
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("aggregates")]
        public async Task<ActionResult<IEnumerable<EventSelector>>> ListAggreggates()
        {
            var is4Aggregates = await _identityServerEventStore.ListAggregates();
            var ssoAggregates = await _eventStoreAppService.ListAggregates();

            return ResponseGet(is4Aggregates.Concat(ssoAggregates));
        }


    }
}
