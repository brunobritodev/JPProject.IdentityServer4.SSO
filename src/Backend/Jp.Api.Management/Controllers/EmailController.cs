using JPProject.Domain.Core.Bus;
using JPProject.Domain.Core.Interfaces;
using JPProject.Domain.Core.Notifications;
using JPProject.Sso.Application.Interfaces;
using JPProject.Sso.Application.ViewModels.EmailViewModels;
using JPProject.Sso.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jp.Api.Management.Controllers
{
    [Route("emails"), Authorize(Policy = "Default")]
    public class EmailController : ApiController
    {
        private readonly IEmailAppService _emailAppService;
        private readonly ISystemUser _systemUser;

        public EmailController(
            INotificationHandler<DomainNotification> notifications,
            IMediatorHandler mediator,
            IEmailAppService emailAppService,
            ISystemUser systemUser) : base(notifications, mediator)
        {
            _emailAppService = emailAppService;
            _systemUser = systemUser;
        }

        [HttpGet("{type}")]
        public async Task<ActionResult<EmailViewModel>> GetEmail(EmailType type)
        {
            var email = await _emailAppService.FindByType(type) ?? new EmailViewModel();
            return ResponseGet(email);
        }

        [HttpGet("types")]
        public ActionResult<List<KeyValuePair<int, string>>> ListTypes()
        {
            var data = new List<KeyValuePair<int, string>>();
            foreach (var value in Enum.GetNames(typeof(EmailType)))
            {
                Enum.TryParse(value, out EmailType enumType);

                data.Add(new KeyValuePair<int, string>((int)enumType, value));
            }
            return ResponseGet(data);
        }

        [HttpPut("{type}")]
        public async Task<ActionResult<EmailViewModel>> UpdateEmail(EmailType type, [FromBody] EmailViewModel command)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return ModelStateErrorResponseError();
            }

            command.Type = type;
            command.Username = _systemUser.Username;
            await _emailAppService.SaveEmail(command);
            return ResponsePutPatch();
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
        public async Task<ActionResult> UpdateTemplate(string name, [FromBody] TemplateViewModel command)
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
        public async Task<ActionResult> RemoveTemplate(string name)
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