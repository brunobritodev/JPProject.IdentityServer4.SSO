using JPProject.Admin.Application.Interfaces;
using JPProject.Admin.Application.ViewModels;
using JPProject.Domain.Core.Bus;
using JPProject.Domain.Core.Notifications;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.Application.Extensions;
using JPProject.Sso.Application.Interfaces;
using JPProject.Sso.Domain.ViewModels.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack;

namespace Jp.Api.Management.Controllers
{
    [Route("persisted-grants"), Authorize(Policy = "Default")]
    public class PersistedGrantsController : ApiController
    {
        private readonly IPersistedGrantAppService _persistedGrantAppService;
        private readonly IUserManageAppService _userAppService;

        public PersistedGrantsController(
            INotificationHandler<DomainNotification> notifications,
            IMediatorHandler mediator,
            IPersistedGrantAppService persistedGrantAppService,
            IUserManageAppService userAppService) : base(notifications, mediator)
        {
            _persistedGrantAppService = persistedGrantAppService;
            _userAppService = userAppService;
        }

        [HttpGet, Route("")]
        public async Task<ActionResult<ListOfPersistedGrantViewModel>> List([Range(1, 50)] int? limit = 10, [Range(1, int.MaxValue)] int? offset = 0)
        {
            var irs = await _persistedGrantAppService.GetPersistedGrants(new PagingViewModel(limit ?? 10, offset ?? 0));
            var usersIds = irs.PersistedGrants.Select(s => s.SubjectId).ToArray();

            var search = new UserSearch<string>()
            {
                Id = usersIds,
                Limit = limit,
                Offset = offset
            };
            var users = await _userAppService.SearchUsers(search);

            foreach (var persistedGrantViewModel in irs.PersistedGrants)
            {
                var user = users.Collection.WithId(persistedGrantViewModel.SubjectId);
                if (user == null) continue;

                persistedGrantViewModel.Email = user.Email;
                persistedGrantViewModel.Picture = user.Picture;
            }

            return ResponseGet(irs);
        }

        [HttpDelete, Route("{id}")]
        public async Task<ActionResult> Remove(string id)
        {
            var model = new RemovePersistedGrantViewModel(id.FromBase64UrlSafe().FromUtf8Bytes());
            await _persistedGrantAppService.Remove(model);
            return ResponseDelete();
        }


    }
}
