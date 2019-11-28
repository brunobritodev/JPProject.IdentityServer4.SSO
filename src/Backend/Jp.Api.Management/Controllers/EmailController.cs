using JPProject.Domain.Core.Bus;
using JPProject.Domain.Core.Notifications;
using JPProject.Sso.Application.Interfaces;
using JPProject.Sso.Application.ViewModels.EmailViewModels;
using JPProject.Sso.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jp.Api.Management.Controllers
{
    [Route("emails"), Authorize(Policy = "ReadOnly")]
    public class EmailController : ApiController
    {
        private readonly IEmailAppService _emailAppService;

        public EmailController(
            INotificationHandler<DomainNotification> notifications,
            IMediatorHandler mediator,
            IEmailAppService emailAppService) : base(notifications, mediator)
        {
            _emailAppService = emailAppService;
        }

        [HttpGet("{type:int}")]
        public async Task<ActionResult<EmailViewModel>> GetEmail(EmailType type)
        {
            var email = await _emailAppService.FindByType(type);
            return ResponseGet(email);
        }


        [HttpGet("templates")]
        public async Task<ActionResult<IEnumerable<TemplateViewModel>>> ListTemplates()
        {
            var email = await _emailAppService.ListTemplates();
            return ResponseGet(email);
        }

        [HttpGet("templates/{name}")]
        public async Task<ActionResult<TemplateViewModel>> GetTemplate(string name)
        {
            var email = await _emailAppService.GetTemplate(name);
            return ResponseGet(email);
        }

        [HttpPost("templates")]
        public async Task<ActionResult<TemplateViewModel>> SaveTemplate([FromBody] TemplateViewModel command)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            await _emailAppService.SaveTemplate(command);

            var newTemplate = await _emailAppService.GetTemplate(command.Name);

            return ResponsePost(nameof(GetTemplate), new { name = command.Name }, newTemplate);
        }

        [HttpPut("templates/{name}")]
        public async Task<ActionResult<TemplateViewModel>> UpdateTemplate(string name, [FromBody] TemplateViewModel command)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            command.SetOldName(name);
            await _emailAppService.UpdateTemplate(command);

            return ResponsePutPatch();
        }


        [HttpDelete("templates/{name}")]
        public async Task<ActionResult<TemplateViewModel>> RemoveTemplate(string name)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            await _emailAppService.RemoveTemplate(name);

            return ResponsePutPatch();
        }

    }
}