using JPProject.Domain.Core.Bus;
using JPProject.Domain.Core.Interfaces;
using JPProject.Domain.Core.Notifications;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.Application.EventSourcedNormalizers;
using JPProject.Sso.Application.Interfaces;
using JPProject.Sso.Application.ViewModels;
using JPProject.Sso.Application.ViewModels.UserViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Jp.Api.Management.Controllers
{
    /// <summary>
    /// A set of services for users to self manage themselves
    /// </summary>
    [Route("accounts"), Authorize(Policy = "UserManagement")]
    public class AccountController : ApiController
    {
        private readonly IUserManageAppService _userAppService;
        private readonly ISystemUser _systemUser;

        public AccountController(
            IUserManageAppService userAppService,
            INotificationHandler<DomainNotification> notifications,
            IMediatorHandler mediator,
            ISystemUser systemUser) : base(notifications, mediator)
        {
            _userAppService = userAppService;
            _systemUser = systemUser;
        }

        /// <summary>
        /// User get his own profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult<UserViewModel>> UserData()
        {
            
            var user = await _userAppService.FindByUsernameAsync(_systemUser.Username);
            return ResponseGet(user);
        }

        /// <summary>
        /// Update profile
        /// </summary>
        [HttpPut("profile")]
        public async Task<ActionResult> UpdateProfile([FromBody] UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            model.UserName = _systemUser.Username;
            await _userAppService.UpdateProfile(model);
            return ResponsePutPatch();
        }

        /// <summary>
        /// Partial update profile
        /// </summary>
        [HttpPatch, Route("profile")]
        public async Task<ActionResult> PartialUpdate([FromBody] JsonPatchDocument<UserViewModel> model)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            var actualUser = await _userAppService.GetUserDetails(_systemUser.Username);
            model.ApplyTo(actualUser);
            await _userAppService.UpdateProfile(actualUser);
            return ResponsePutPatch();
        }

        /// <summary>
        /// Update profile picture
        /// </summary>
        [HttpPut("profile/picture")]
        public async Task<ActionResult<bool>> UpdateProfilePicture([FromBody] ProfilePictureViewModel model)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            model.Username = _systemUser.Username;
            model.Filename = $"{_systemUser.Username}{model.FileType.Replace("image/", ".")}";
            await _userAppService.UpdateProfilePicture(model);
            return ResponsePutPatch();
        }

        /// <summary>
        /// User can auto remove from system.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("")]
        public async Task<ActionResult<bool>> RemoveAccount()
        {
            var model = new RemoveAccountViewModel(_systemUser.Username);
            await _userAppService.RemoveAccount(model);
            return ResponseDelete();
        }

        /// <summary>
        /// User can update his own password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("password")]
        public async Task<ActionResult<bool>> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            model.Username = _systemUser.Username;
            await _userAppService.ChangePassword(model);
            return ResponsePutPatch();
        }

        /// <summary>
        /// Create password for users who don't have one. E.g Ferated registration.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("password")]
        public async Task<ActionResult<bool>> CreatePassword([FromBody] SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            model.Username = _systemUser.Username;
            await _userAppService.CreatePassword(model);
            return Ok();
        }

        /// <summary>
        /// Check if current logged user has a password
        /// When it register by Federation Gateway there is no need to create a password. Then it can update it later.
        /// </summary>
        /// <returns></returns>
        [HttpGet("password")]
        public async Task<ActionResult<bool>> HasPassword()
        {
            return ResponseGet(await _userAppService.HasPassword(_systemUser.Username));
        }

        [HttpGet, Route("logs")]
        public async Task<ActionResult<ListOf<EventHistoryData>>> GetLogs([Range(1, 50)] int? limit = 10, [Range(1, int.MaxValue)] int? offset = 1, string search = null)
        {
            return ResponseGet(await _userAppService.GetEvents(_systemUser.Username, new PagingViewModel(limit ?? 10, offset ?? 0, search)));
        }

        [Route("access-denied"), AllowAnonymous]
        protected ActionResult AccessDenied(string ReturnUrl)
        {
            return Unauthorized();
        }
    }

}