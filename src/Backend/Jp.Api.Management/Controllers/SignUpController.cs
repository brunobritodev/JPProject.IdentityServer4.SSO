using Jp.Api.Management.Interfaces;
using JPProject.Domain.Core.Bus;
using JPProject.Domain.Core.Notifications;
using JPProject.Sso.Application.Interfaces;
using JPProject.Sso.Application.ViewModels.UserViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Jp.Api.Management.Controllers
{
    [Route("sign-up"), AllowAnonymous]
    public class SignUpController : ApiController
    {
        private readonly IUserAppService _userAppService;
        private readonly DomainNotificationHandler _notifications;
        private readonly IMediatorHandler _mediator;
        private readonly IReCaptchaService _reCaptchaService;

        public SignUpController(
            IUserAppService userAppService,
            INotificationHandler<DomainNotification> notifications,
            IMediatorHandler mediator,
            IReCaptchaService reCaptchaService) : base(notifications, mediator)
        {
            _userAppService = userAppService;
            _notifications = (DomainNotificationHandler)notifications;
            _mediator = mediator;
            _reCaptchaService = reCaptchaService;
        }

        [HttpPost, Route("")]
        public async Task<ActionResult<RegisterUserViewModel>> Register([FromBody] RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            if (await _reCaptchaService.IsCaptchaEnabled())
            {
                var captchaSucces = await _reCaptchaService.IsCaptchaPassed();
                if (!captchaSucces)
                {
                    await _mediator.RaiseEvent(new DomainNotification("Recatcha", "ReCaptcha failed"));
                    return BadRequest(new ValidationProblemDetails(_notifications.GetNotificationsByKey()));
                }
            }

            if (model.ContainsFederationGateway())
                await _userAppService.RegisterWithProvider(model);
            else
                await _userAppService.Register(model);

            model.ClearSensitiveData();
            return ResponsePost("UserData", "Account", null, model);
        }


        [HttpGet, Route("check-username/{suggestedUsername}")]
        public async Task<ActionResult<bool>> CheckUsername(string suggestedUsername)
        {
            var exist = await _userAppService.CheckUsername(suggestedUsername);

            return ResponseGet(exist);
        }

        [HttpGet, Route("check-email/{givenEmail}")]
        public async Task<ActionResult<bool>> CheckEmail(string givenEmail)
        {
            var exist = await _userAppService.CheckUsername(givenEmail);

            return ResponseGet(exist);
        }
    }
}