using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4.Contrib.AspNetCore.Testing.Configuration;
using IdentityServer4.Models;
using JPProject.Api.Management.Tests.Fakers.ClientFakers;
using ServiceStack;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JPProject.Api.Management.Tests.Controller
{
    public class ClientControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        public CustomWebApplicationFactory Factory { get; }
        private readonly HttpClient _client;
        private TokenResponse _token;

        public ClientControllerTests(CustomWebApplicationFactory factory)
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
        public async Task ShouldGetAllClients()
        {
            await Login();
            var httpResponse = await _client.GetAsync("/clients");

            httpResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ShouldSaveClient()
        {
            await Login();

            var client = ClientViewModelFaker.GenerateSaveClient().Generate();

            var httpResponse = await _client.PostAsync("/clients", new StringContent(client.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            httpResponse.EnsureSuccessStatusCode();
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/clients");
        }

        [Fact]
        public async Task ShouldGetClient()
        {
            await Login();
            var client = ClientViewModelFaker.GenerateSaveClient().Generate();

            var httpResponse = await _client.PostAsync("/clients", new StringContent(client.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            httpResponse.EnsureSuccessStatusCode();
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/clients");

            httpResponse = await _client.GetAsync($"/clients/{client.ClientId}");

            httpResponse.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var clients = stringResponse.FromJson<Client>();

            clients.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldDeleteClient()
        {
            await Login();
            var client = ClientViewModelFaker.GenerateSaveClient().Generate();

            var httpResponse = await _client.PostAsync("/clients", new StringContent(client.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            httpResponse.EnsureSuccessStatusCode();
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/clients");

            httpResponse = await _client.DeleteAsync($"/clients/{client.ClientId}");

            httpResponse.EnsureSuccessStatusCode();
        }


        [Fact]
        public async Task ShouldUpdateClientId()
        {
            await Login();
            var newClient = ClientViewModelFaker.GenerateSaveClient().Generate();

            // Create one
            var httpResponse = await _client.PostAsync("/clients", new StringContent(newClient.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            httpResponse.EnsureSuccessStatusCode();
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/clients");

            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var client = stringResponse.FromJson<Client>();

            // Update it
            var oldClientId = client.ClientId;
            client.ClientId = "newclient";
            httpResponse = await _client.PutAsync($"/clients/{oldClientId}", new StringContent(client.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            httpResponse.EnsureSuccessStatusCode();

            // Get updated
            httpResponse = await _client.GetAsync($"/clients/{client.ClientId}");
            httpResponse.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var clients = stringResponse.FromJson<Client>();

            clients.Should().NotBeNull();
            clients.ClientId.Should().Be(client.ClientId);
        }
    }
}
