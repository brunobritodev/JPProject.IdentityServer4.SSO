using JPProject.Domain.Core.Bus;
using JPProject.Domain.Core.Interfaces;
using JPProject.Domain.Core.Notifications;
using JPProject.Domain.Core.Util;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.Application.EventSourcedNormalizers;
using JPProject.Sso.Application.Interfaces;
using JPProject.Sso.Application.ViewModels.RoleViewModels;
using JPProject.Sso.Application.ViewModels.UserViewModels;
using JPProject.Sso.Domain.ViewModels.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Jp.Api.Management.Controllers
{
    [Route("admin/users"), Authorize(Policy = "Default")]
    public class UserAdminController : ApiController
    {
        private readonly IUserManageAppService _userManageAppService;
        private readonly ISystemUser _user;
        private readonly IUserAppService _userAppService;

        public UserAdminController(
            INotificationHandler<DomainNotification> notifications,
            IMediatorHandler mediator,
            IUserManageAppService userManageAppService,
            ISystemUser user,
            IUserAppService userAppService) : base(notifications, mediator)
        {

            _userManageAppService = userManageAppService;
            _user = user;
            _userAppService = userAppService;
        }

        /// <summary>
        /// Get a list of users, optionally can filter them
        /// </summary>
        /// <param name="limit">Limit - At least 1 and max 50</param>
        /// <param name="offset">Offset - For pagination</param>
        /// <param name="search">username, e-mail or name</param>
        /// <returns></returns>
        [HttpGet, Route("")]
        public async Task<ActionResult<ListOf<UserListViewModel>>> List([Range(1, 50)] int? limit = 10, [Range(1, int.MaxValue)] int? offset = 0, string search = null)
        {
            var users = await _userManageAppService.SearchUsersByProperties(new UserFindByProperties(search) { Limit = limit, Offset = offset });

            // Truncate data for non admins
            if (!User.IsInRole("Administrator") && !User.HasClaim(c => c.Type == "is4-manager"))
            {
                foreach (var ir in users.Collection)
                {
                    if (_user.Username == ir.UserName)
                        continue;
                    ir.Email = ir.Email?.TruncateEmail();
                    ir.UserName = ir.UserName?.TruncateSensitiveInformation();
                }
            }

            return ResponseGet(users);
        }

        [HttpGet, Route("{username}")]
        public async Task<ActionResult<UserViewModel>> Details(string username)
        {
            var irs = await _userManageAppService.GetUserDetails(username);

            if (!User.IsInRole("Administrator") && !User.HasClaim(c => c.Type == "is4-manager") && _user.Username != username)
            {
                irs.Email = irs.Email?.TruncateEmail();
                irs.UserName = irs.UserName?.TruncateSensitiveInformation();
                irs.PhoneNumber = irs.PhoneNumber?.TruncateSensitiveInformation();
            }

            return ResponseGet(irs);
        }

        [HttpPut, Route("{username}")]
        public async Task<ActionResult> Update(string username, [FromBody] UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }
            model.UserName = username;
            await _userManageAppService.UpdateUser(model);
            return ResponsePutPatch();
        }

        [HttpPatch, Route("{username}")]
        public async Task<ActionResult> PartialUpdate(string username, [FromBody] JsonPatchDocument<UserViewModel> model)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            var actualUser = await _userManageAppService.FindByUsernameAsync(username);
            model.ApplyTo(actualUser);
            await _userManageAppService.UpdateUser(actualUser);
            return ResponsePutPatch();
        }

        [HttpDelete, Route("{username}")]
        public async Task<ActionResult> RemoveAccount(string username)
        {
            var model = new RemoveAccountViewModel(username);
            await _userManageAppService.RemoveAccount(model);
            return ResponseDelete();
        }


        [HttpGet, Route("{username}/claims")]
        public async Task<ActionResult<IEnumerable<ClaimViewModel>>> Claims(string userName)
        {
            var clients = await _userManageAppService.GetClaims(userName);
            return ResponseGet(clients);
        }

        [HttpDelete, Route("{username}/claims/{type}")]
        public async Task<ActionResult> RemoveClaim(string username, string type, string value)
        {
            var model = new RemoveUserClaimViewModel(type, value) { Username = username };
            await _userManageAppService.RemoveClaim(model);
            return ResponseDelete();
        }


        [HttpPost, Route("{username}/claims")]
        public async Task<ActionResult<SaveUserClaimViewModel>> SaveClaim(string username, [FromBody] SaveUserClaimViewModel model)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            model.Username = username;
            await _userManageAppService.SaveClaim(model);
            return ResponsePost(nameof(Claims), new { username }, model);
        }

        [HttpGet, Route("{username}/roles")]
        public async Task<ActionResult<IEnumerable<RoleViewModel>>> Roles(string userName)
        {
            var clients = await _userManageAppService.GetRoles(userName);
            return ResponseGet(clients);
        }

        [HttpPost, Route("{username}/roles")]
        public async Task<ActionResult<SaveUserRoleViewModel>> SaveRole(string username, [FromBody] SaveUserRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            model.Username = username;
            await _userManageAppService.SaveRole(model);
            return ResponsePost(nameof(Roles), new { username }, model);
        }

        [HttpDelete, Route("{username}/roles/{role}")]
        public async Task<ActionResult> RemoveRole(string username, string role)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }
            var model = new RemoveUserRoleViewModel(username, role);
            await _userManageAppService.RemoveRole(model);
            return ResponseDelete();
        }


        [HttpGet, Route("{username}/logins")]
        public async Task<ActionResult<IEnumerable<UserLoginViewModel>>> Logins(string userName)
        {
            var clients = await _userManageAppService.GetLogins(userName);
            return ResponseGet(clients);
        }

        [HttpDelete, Route("{username}/logins")]
        public async Task<ActionResult> RemoveLogin(string username, string loginProvider, string providerKey)
        {
            var model = new RemoveUserLoginViewModel(username, loginProvider, providerKey);
            await _userManageAppService.RemoveLogin(model);
            return ResponseDelete();
        }


        [HttpPut, Route("{username}/password")]
        public async Task<ActionResult> ResetPassword(string username, [FromBody] AdminChangePasswordViewodel model)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            model.Username = username;
            await _userManageAppService.ResetPassword(model);
            return ResponsePutPatch();
        }

        /// <summary>
        /// Get a list of users, optionally can filter them
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="limit">Limit - At least 1 and max 50</param>
        /// <param name="offset">Offset - For pagination</param>
        /// <param name="search">Action, Aggregate or Message</param>
        /// <returns></returns>
        [HttpGet, Route("{username}/logs")]
        public async Task<ActionResult<ListOf<EventHistoryData>>> ShowLogs(string username, [Range(1, 50)] int? limit = 10, [Range(1, int.MaxValue)] int? offset = 1, string search = null)
        {
            var data = new PagingViewModel(limit ?? 10, offset ?? 0, search);
            var clients = await _userManageAppService.GetEvents(username, data);
            return ResponseGet(clients);
        }

        /// <summary>
        /// Create a new user with admin access to some properties
        /// </summary>
        [HttpPost, Route("")]
        public async Task<ActionResult<AdminRegisterUserViewModel>> Register([FromBody] AdminRegisterUserViewModel model)
        {
            // awating v3.0.2 to uncomment this
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            await _userAppService.AdminRegister(model);

            model.ClearSensitiveData();
            return ResponsePost("UserData", "Account", null, model);
        }

    }
}