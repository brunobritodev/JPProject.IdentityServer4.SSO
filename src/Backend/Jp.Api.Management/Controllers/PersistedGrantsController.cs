using AspNetCore.IQueryable.Extensions;
using JPProject.Admin.Application.Interfaces;
using JPProject.Admin.Application.ViewModels;
using JPProject.Domain.Core.Bus;
using JPProject.Domain.Core.Notifications;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.AspNetIdentity.Models.Identity;
using JPProject.Sso.Domain.ViewModels.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceStack;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jp.Api.Management.Controllers
{
    [Route("persisted-grants"), Authorize(Policy = "Default")]
    public class PersistedGrantsController : ApiController
    {
        private readonly IPersistedGrantAppService _persistedGrantAppService;
        private readonly UserManager<UserIdentity> _manager;

        public PersistedGrantsController(
            INotificationHandler<DomainNotification> notifications,
            IMediatorHandler mediator,
            IPersistedGrantAppService persistedGrantAppService,
            UserManager<UserIdentity> manager) : base(notifications, mediator)
        {
            _persistedGrantAppService = persistedGrantAppService;
            _manager = manager;
        }

        [HttpGet, Route("")]
        public async Task<ActionResult<ListOf<PersistedGrantViewModel>>> List([Range(1, 50)] int? limit = 10, [Range(1, int.MaxValue)] int? offset = 0)
        {
            var searchPersisted = new PersistedGrantSearch()
            {
                Limit = limit,
                Offset = offset
            };
            var irs = await _persistedGrantAppService.GetPersistedGrants(searchPersisted);
            var usersIds = irs.Collection.Select(s => s.SubjectId).ToArray();

            var search = new UserSearch<string>()
            {
                Id = usersIds,
                Limit = limit,
                Offset = offset
            };
            var users = await _manager.Users.Apply(search).ToListAsync();
            var collection = irs.Collection.ToList();
            foreach (var persistedGrantViewModel in collection)
            {
                var user = users.FirstOrDefault(u => u.Id == persistedGrantViewModel.SubjectId);
                if (user == null) continue;

                persistedGrantViewModel.UpdateUserInfo(user.Email, user.Picture);
            }

            return ResponseGet(new ListOf<PersistedGrantViewModel>(collection, collection.Count));
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
