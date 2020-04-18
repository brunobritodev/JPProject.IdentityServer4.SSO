using Bogus;
using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4.Contrib.AspNetCore.Testing.Configuration;
using IdentityServer4.Models;
using JPProject.Admin.Application.ViewModels.ClientsViewModels;
using JPProject.Api.Management.Tests.Fakers.ClientFakers;
using JPProject.Domain.Core.ViewModels;
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
    public class ClientControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly ITestOutputHelper _output;
        public CustomWebApplicationFactory Factory { get; }
        private readonly HttpClient _client;
        private TokenResponse _token;
        private Faker _faker;

        public ClientControllerTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
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
        public async Task ShouldGetAllClients()
        {
            await Login();
            var httpResponse = await _client.GetAsync("/clients");

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
        }

        [Fact]
        public async Task ShouldSaveClient()
        {
            await Login();

            var client = ClientViewModelFaker.GenerateSaveClient().Generate();

            var httpResponse = await _client.PostAsync("/clients", new StringContent(client.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; }
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/clients");
        }

        [Fact]
        public async Task ShouldGetClient()
        {
            await Login();
            var client = ClientViewModelFaker.GenerateSaveClient().Generate();

            var httpResponse = await _client.PostAsync("/clients", new StringContent(client.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; throw; };
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/clients");

            httpResponse = await _client.GetAsync($"/clients/{client.ClientId}");

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };

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

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/clients");

            httpResponse = await _client.DeleteAsync($"/clients/{client.ClientId}");

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
        }

        [Fact]
        public async Task ShouldUpdateClientId()
        {
            await Login();
            var newClient = ClientViewModelFaker.GenerateSaveClient().Generate();

            // Create one
            var httpResponse = await _client.PostAsync("/clients", new StringContent(newClient.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/clients");

            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var client = stringResponse.FromJson<Client>();

            // Update it
            var oldClientId = client.ClientId;
            client.AllowedGrantTypes = new[] { GrantType.ClientCredentials };
            client.ClientId = "newclient";
            httpResponse = await _client.PutAsync($"/clients/{oldClientId}", new StringContent(client.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));
            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };

            // Get updated
            httpResponse = await _client.GetAsync($"/clients/{client.ClientId}");
            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };

            // Deserialize and examine results.
            stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var clients = stringResponse.FromJson<Client>();

            clients.Should().NotBeNull();
            clients.ClientId.Should().Be(client.ClientId);
        }

        [Fact]
        public async Task ShouldAddNewClientSecret()
        {
            await Login();

            var newClient = await AddClient();

            var newSecret = ClientViewModelFaker.GenerateSaveClientSecret(newClient.ClientId).Generate();

            // Create one
            var httpResponse = await _client.PostAsync($"/clients/{newClient.ClientId}/secrets", new StringContent(newSecret.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/secrets");

        }


        [Fact]
        public async Task ShouldListClientSecret()
        {
            await Login();

            var newClient = await AddClient();

            var newSecret = ClientViewModelFaker.GenerateSaveClientSecret(newClient.ClientId).Generate();

            // Create one
            var httpResponse = await _client.PostAsync($"/clients/{newClient.ClientId}/secrets", new StringContent(newSecret.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/secrets");

            httpResponse = await _client.GetAsync($"/clients/{newClient.ClientId}/secrets");

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var secrets = stringResponse.FromJson<IEnumerable<Secret>>();

            secrets.Should().HaveCountGreaterThan(0);
        }


        [Fact]
        public async Task ShouldDeleteClientSecret()
        {
            await Login();

            var newClient = await AddClient();

            var newSecret = ClientViewModelFaker.GenerateSaveClientSecret(newClient.ClientId).Generate();

            // Create one
            var httpResponse = await _client.PostAsync($"/clients/{newClient.ClientId}/secrets", new StringContent(newSecret.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/secrets");

            httpResponse = await _client.GetAsync($"/clients/{newClient.ClientId}/secrets");

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var secrets = stringResponse.FromJson<IEnumerable<Secret>>();


            httpResponse = await _client.DeleteAsync($"/clients/{newClient.ClientId}/secrets?type={secrets.First().Type.UrlEncode()}&value={Uri.EscapeDataString(secrets.First().Value.UrlEncode())}");
            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
        }


        [Fact]
        public async Task ShouldAddNewClientProperty()
        {
            await Login();

            var newClient = await AddClient();

            var newProperty = ClientViewModelFaker.GenerateSaveProperty(newClient.ClientId).Generate();

            // Create one
            var httpResponse = await _client.PostAsync($"/clients/{newClient.ClientId}/properties", new StringContent(newProperty.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/properties");

        }


        [Fact]
        public async Task ShouldListClientProperties()
        {
            await Login();

            var newClient = await AddClient();

            var newProperty = ClientViewModelFaker.GenerateSaveProperty(newClient.ClientId).Generate();

            // Create one
            var httpResponse = await _client.PostAsync($"/clients/{newClient.ClientId}/properties", new StringContent(newProperty.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/properties");

            httpResponse = await _client.GetAsync($"/clients/{newClient.ClientId}/properties");

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var secrets = stringResponse.FromJson<IEnumerable<ClientPropertyViewModel>>();

            secrets.Should().HaveCountGreaterThan(0);
        }


        [Fact]
        public async Task ShouldDeleteClientProperties()
        {
            await Login();

            var newClient = await AddClient();

            var newProperty = ClientViewModelFaker.GenerateSaveProperty(newClient.ClientId).Generate();

            // Create one
            var httpResponse = await _client.PostAsync($"/clients/{newClient.ClientId}/properties", new StringContent(newProperty.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/properties");

            httpResponse = await _client.GetAsync($"/clients/{newClient.ClientId}/properties");

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var properties = stringResponse.FromJson<IEnumerable<ClientPropertyViewModel>>();

            httpResponse = await _client.DeleteAsync($"/clients/{newClient.ClientId}/properties/{properties.First().Key}");
            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
        }




        [Fact]
        public async Task ShouldAddNewClientClaim()
        {
            await Login();

            var newClient = await AddClient();

            var claim = ClientViewModelFaker.GenerateSaveClaim(newClient.ClientId).Generate();

            // Create one
            var httpResponse = await _client.PostAsync($"/clients/{newClient.ClientId}/claims", new StringContent(claim.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/claims");

        }


        [Fact]
        public async Task ShouldListClientClaims()
        {
            await Login();

            var newClient = await AddClient();

            var claim = ClientViewModelFaker.GenerateSaveClaim(newClient.ClientId).Generate();

            // Create one
            var httpResponse = await _client.PostAsync($"/clients/{newClient.ClientId}/claims", new StringContent(claim.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/claims");

            httpResponse = await _client.GetAsync($"/clients/{newClient.ClientId}/claims");

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var claims = stringResponse.FromJson<IEnumerable<ClaimViewModel>>();

            claims.Should().HaveCountGreaterOrEqualTo(1);
        }


        [Fact]
        public async Task ShouldDeleteClientClaims()
        {
            await Login();

            var newClient = await AddClient();

            var claim = ClientViewModelFaker.GenerateSaveClaim(newClient.ClientId).Generate();

            // Create one
            var httpResponse = await _client.PostAsync($"/clients/{newClient.ClientId}/claims", new StringContent(claim.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/claims");

            httpResponse = await _client.GetAsync($"/clients/{newClient.ClientId}/claims");

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var claims = stringResponse.FromJson<IEnumerable<ClaimViewModel>>();

            claims.Should().HaveCountGreaterThan(0);

            httpResponse = await _client.DeleteAsync($"/clients/{newClient.ClientId}/claims?type={claims.First().Type.UrlEncode()}&value={claims.First().Value.UrlEncode()}");
            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
        }



        private async Task<SaveClientViewModel> AddClient()
        {
            var newClient = ClientViewModelFaker.GenerateSaveClient().Generate();
            // Create one
            var httpResponse = await _client.PostAsync("/clients", new StringContent(newClient.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json));

            try { httpResponse.EnsureSuccessStatusCode(); } catch { _output.WriteLine(await httpResponse.Content.ReadAsStringAsync()); throw; };
            httpResponse.Headers.Location.Should().NotBeNull();
            httpResponse.Headers.Location.PathAndQuery.Should().Contain("/clients");
            return newClient;
        }
    }
}
