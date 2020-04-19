using IdentityServer4.Extensions;
using Jp.Api.Management.Interfaces;
using JPProject.Sso.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Jp.Api.Management.Configuration
{
    public class ReCaptchaService : IReCaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly IGlobalConfigurationAppService _globalConfigurationAppService;
        private readonly IHttpContextAccessor _httpContext;
        private const string RemoteAddress = "https://www.google.com/recaptcha/api/siteverify";

        public ReCaptchaService(IHttpClientFactory httpClientFactory, IGlobalConfigurationAppService globalConfigurationAppService, IHttpContextAccessor httpContext)
        {
            _httpClient = httpClientFactory.CreateClient();
            _globalConfigurationAppService = globalConfigurationAppService;
            _httpContext = httpContext;
        }

        public async Task<bool> IsCaptchaEnabled()
        {
            var settings = await _globalConfigurationAppService.GetPrivateSettings();
            return settings.UseRecaptcha;
        }
        public async Task<bool> IsCaptchaPassed()
        {
            var recaptcha = _httpContext.HttpContext.Request.Headers["recaptcha"];
            if (recaptcha.IsNullOrEmpty())
                return false;

            dynamic response = await GetCaptchaResultDataAsync(recaptcha);
            return response.success == "true";
        }

        public async Task<JObject> GetCaptchaResultDataAsync(string token)
        {
            var settings = await _globalConfigurationAppService.GetPrivateSettings();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", settings.Recaptcha.PrivateKey),
                new KeyValuePair<string, string>("response", token)
            });
            var res = await _httpClient.PostAsync(RemoteAddress, content);
            if (res.StatusCode != HttpStatusCode.OK)
            {
                return JObject.Parse("{success: 'false'}");
            }
            var jsonResult = await res.Content.ReadAsStringAsync();
            return JObject.Parse(jsonResult);
        }
    }
}
