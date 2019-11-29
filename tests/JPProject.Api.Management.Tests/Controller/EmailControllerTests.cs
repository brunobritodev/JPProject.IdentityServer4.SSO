using Bogus;
using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4.Contrib.AspNetCore.Testing.Configuration;
using JPProject.Api.Management.Tests.Fakers.EmailFakers;
using JPProject.Sso.Application.ViewModels.EmailViewModels;
using JPProject.Sso.Domain.Models;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace JPProject.Api.Management.Tests.Controller
{
    public class EmailControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly ITestOutputHelper _output;
        public CustomWebApplicationFactory Factory { get; }
        private readonly HttpClient _client;
        private TokenResponse _token;
        private Faker _faker;

        public EmailControllerTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        {
            _faker = new Faker();
            _output = output;
            Factory = factory;
            _client = Factory.CreateClient();
        }

        private async Task Login()
        {
            _token = await Factory.IdentityServerClient.GetResourceOwnerPasswordAccessTokenAsync(
                new ClientConfiguration("TestClient", "MySecret"),
                new UserLoginConfiguration("user", "password"),
                "jp_api.user", "jp_api.is4");
            // The endpoint or route of the controller action.
            _client.SetBearerToken(_token.AccessToken);
        }

        [Fact]
        public async Task ShouldSaveEmail()
        {
            await Login();

            var email = EmailFakers.GenerateEmail().Generate();

            var httpResponse = await _client.PutAsync($"/emails/{email.Type}", new StringContent(email.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; }
        }

        [Fact]
        public async Task ShouldListAllEmailTypes()
        {
            await Login();

            var httpResponse = await _client.GetAsync($"/emails/types");

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; }

            var content = await httpResponse.Content.ReadAsStringAsync();
            var types = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<KeyValue>>(content);

            types.Select(s => s.Value).Should().Match(enumerable => enumerable.All(a => Enum.IsDefined(typeof(EmailType), a)));
        }

        [Fact]
        public async Task ShouldGetAllTemplates()
        {
            await Login();
            var httpResponse = await _client.GetAsync("/emails/templates");

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
        }

        [Fact]
        public async Task ShouldSaveTemplates()
        {
            await Login();

            var email = EmailFakers.GenerateTemplate().Generate();

            var httpResponse = await _client.PostAsync("/emails/templates", new StringContent(email.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; }
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/emails");
        }

        [Fact]
        public async Task ShouldUpdateTemplates()
        {
            await Login();

            var email = EmailFakers.GenerateTemplate().Generate();

            await Insert(email);
            email.OldName = email.Name;
            email.Name = _faker.Internet.DomainName();

            var httpResponse = await _client.PutAsync($"/emails/templates/{email.OldName}", new StringContent(email.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; }
        }

        [Fact]
        public async Task ShouldRemoveTemplates()
        {
            await Login();

            var email = EmailFakers.GenerateTemplate().Generate();

            await Insert(email);

            var httpResponse = await _client.DeleteAsync($"/emails/templates/{email.Name}");

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; }
        }

        private async Task Insert(TemplateViewModel email)
        {
            var httpResponse = await _client.PostAsync("/emails/templates",
                new StringContent(email.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            try
            {
                httpResponse.EnsureSuccessStatusCode();
            }
            catch
            {
                _output.WriteLine(await httpResponse.Content.ReadAsStringAsync());
                throw;
            }

            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/emails");
        }
        public class KeyValue
        {
            public int Key { get; set; }
            public string Value { get; set; }
        }

    }
}
