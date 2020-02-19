using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4.Contrib.AspNetCore.Testing.Configuration;
using JPProject.Admin.Application.ViewModels;
using JPProject.Admin.Application.ViewModels.ApiResouceViewModels;
using JPProject.Api.Management.Tests.Fakers.IdentityResourceFakers;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Xunit;

namespace JPProject.Api.Management.Tests.Controller
{
    public class ApiResourceTests : IClassFixture<CustomWebApplicationFactory>
    {
        public CustomWebApplicationFactory Factory { get; }
        private readonly HttpClient _client;
        private TokenResponse _token;

        public ApiResourceTests(CustomWebApplicationFactory factory)
        {
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
        public async Task ShouldGetAllApiResources()
        {
            await Login();
            _client.SetBearerToken(_token.AccessToken);

            var command = ApiResourceFaker.GenerateApiResource().Generate();

            var response = await _client.PostAsync("/api-resources", new StringContent(command.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            response.EnsureSuccessStatusCode();
            response.Headers.Location.PathAndQuery.Should().Contain("/api-resources");
            // Get content
            response = await _client.GetAsync("/api-resources");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var resources = content.FromJson<IEnumerable<ApiResourceListViewModel>>();

            resources.Should().HaveCountGreaterThan(0);
        }


        [Fact]
        public async Task ShouldAddIdentityResources()
        {
            await Login();
            _client.SetBearerToken(_token.AccessToken);

            var command = ApiResourceFaker.GenerateApiResource().Generate();

            var response = await _client.PostAsync("/api-resources", new StringContent(command.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            response.EnsureSuccessStatusCode();
            response.Headers.Location.PathAndQuery.Should().Contain("/api-resources");

        }

        [Fact]
        public async Task ShouldHaveDefaultScope()
        {
            await Login();
            _client.SetBearerToken(_token.AccessToken);

            var command = ApiResourceFaker.GenerateApiResource().Generate();

            var response = await _client.PostAsync("/api-resources", new StringContent(command.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            response.EnsureSuccessStatusCode();
            response.Headers.Location.PathAndQuery.Should().Contain("/api-resources");

            // Get content
            response = await _client.GetAsync($"/api-resources/{command.Name}/scopes");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var scope = content.FromJson<IEnumerable<Scope>>();
            scope.Should().HaveCountGreaterOrEqualTo(1);
        }


        [Fact]
        public async Task ShouldNotAddIdentityResourcesWithoutValidBearer()
        {
            _client.SetBearerToken(Guid.NewGuid().ToString());

            var command = IdentityResourceFaker.GenerateIdentiyResource().Generate();

            var response = await _client.PostAsync("/identity-resources", new StringContent(command.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        }
    }
}