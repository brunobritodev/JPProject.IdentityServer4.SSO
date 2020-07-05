using Jp.Api.Management.ViewModel;
using Jp.Ldap;
using Jp.Ldap.Test;
using JPProject.Domain.Core.Bus;
using JPProject.Domain.Core.Interfaces;
using JPProject.Domain.Core.Notifications;
using JPProject.Sso.Application.Interfaces;
using JPProject.Sso.Application.ViewModels;
using JPProject.Sso.Domain.ViewModels.Settings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jp.Api.Management.Controllers
{
    [Route("global-configuration"), Authorize(Policy = "Default")]
    public class GlobalConfigurationController : ApiController
    {
        private readonly IGlobalConfigurationAppService _globalConfigurationSettingsAppService;
        private readonly ISystemUser _systemUser;

        public GlobalConfigurationController(
            INotificationHandler<DomainNotification> notifications,
            IMediatorHandler mediator,
            IGlobalConfigurationAppService globalConfigurationSettingsAppService,
            ISystemUser systemUser) : base(notifications, mediator)
        {
            _globalConfigurationSettingsAppService = globalConfigurationSettingsAppService;
            _systemUser = systemUser;
        }

        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<ConfigurationViewModel>>> List()
        {
            return ResponseGet(result: await _globalConfigurationSettingsAppService.ListSettings());
        }

        [HttpGet("public-settings"), AllowAnonymous]
        public async Task<ActionResult<PublicSettings>> PublicSettings()
        {
            return ResponseGet(result: await _globalConfigurationSettingsAppService.GetPublicSettings());
        }

        [HttpPut("")]
        public async Task<ActionResult> Update([FromBody] IEnumerable<ConfigurationViewModel> settings)
        {
            await _globalConfigurationSettingsAppService.UpdateSettings(settings);
            return ResponsePutPatch();
        }

        [HttpGet("ldap-test")]
        public ActionResult<LdapConnectionResult> TestLdapSettings([FromQuery] LdapSettingsTestQuery query)
        {
            var ldapTest = new NovelLdapTestConnection(query.Get());
            return ResponseGet(ldapTest.Test(query.Username, query.Password));
        }

    }
}
