using IdentityServer4.Extensions;
using Jp.Api.Management.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContext;
        private const string RemoteAddress = "https://www.google.com/recaptcha/api/siteverify";

        public ReCaptchaService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContext)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _httpContext = httpContext;
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
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _configuration["ApplicationSettings:RecaptchaKey"]),
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
