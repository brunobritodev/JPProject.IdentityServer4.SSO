using Bogus;
using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4.Contrib.AspNetCore.Testing.Configuration;
using JPProject.Api.Management.Tests.Fakers.UserFakers;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.Application.ViewModels.UserViewModels;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JPProject.Api.Management.Tests.Controller
{
    public class UserAdminControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        public CustomWebApplicationFactory Factory { get; }
        private readonly HttpClient _client;
        private TokenResponse _token;
        private Faker _faker;

        public UserAdminControllerTests(CustomWebApplicationFactory factory)
        {
            Factory = factory;
            _client = Factory.CreateClient();
            _faker = new Faker();
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
        public async Task ShouldListUsers()
        {
            await Login();
            var newUser = UserViewModelFaker.GenerateUserViewModel().Generate();

            var response = await _client.PostAsync("/sign-up", new StringContent(newUser.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            response.EnsureSuccessStatusCode();

            var httpResponse = await _client.GetAsync("/admin/users");
            httpResponse.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var users = stringResponse.FromJson<ListOf<UserListViewModel>>();

            users.Should().NotBeNull();

        }


        [Fact]
        public async Task ShouldRemoveClaim()
        {
            await Login();
            var newUser = UserViewModelFaker.GenerateUserViewModel().Generate();

            var response = await _client.PostAsync("/sign-up", new StringContent(newUser.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            response.EnsureSuccessStatusCode();

            var claim = UserViewModelFaker.GenerateClaim().Generate();
            response = await _client.PostAsync($"/admin/users/{newUser.Username}/claims", new StringContent(claim.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            response.EnsureSuccessStatusCode();

            response = await _client.DeleteAsync($"/admin/users/{newUser.Username}/claims/{claim.Type}");

            response.EnsureSuccessStatusCode();

        }


        [Fact]
        public async Task ShouldRemoveClaimByTypeAndValue()
        {
            await Login();
            var newUser = UserViewModelFaker.GenerateUserViewModel().Generate();

            var response = await _client.PostAsync("/sign-up", new StringContent(newUser.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            response.EnsureSuccessStatusCode();

            var claim = UserViewModelFaker.GenerateClaim().Generate();
            response = await _client.PostAsync($"/admin/users/{newUser.Username}/claims", new StringContent(claim.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            response.EnsureSuccessStatusCode();

            string firstClaimValue = claim.Value;
            claim.Value = _faker.Internet.DomainName();
            response = await _client.PostAsync($"/admin/users/{newUser.Username}/claims", new StringContent(claim.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            response.EnsureSuccessStatusCode();


            response = await _client.DeleteAsync($"/admin/users/{newUser.Username}/claims/{claim.Type}?value={claim.Value}");
            response.EnsureSuccessStatusCode();

            // List all claims
            var claimResponse = await _client.GetAsync($"/admin/users/{newUser.Username}/claims");
            var claims = (await claimResponse.Content.ReadAsStringAsync()).FromJson<IEnumerable<ClaimViewModel>>();

            claims.FirstOrDefault(model => model.Value.Equals(firstClaimValue)).Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldAddClaim()
        {
            await Login();
            var newUser = UserViewModelFaker.GenerateUserViewModel().Generate();

            var response = await _client.PostAsync("/sign-up", new StringContent(newUser.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            response.EnsureSuccessStatusCode();

            var claim = UserViewModelFaker.GenerateClaim().Generate();
            response = await _client.PostAsync($"/admin/users/{newUser.Username}/claims", new StringContent(claim.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            response.EnsureSuccessStatusCode();


        }
    }
}