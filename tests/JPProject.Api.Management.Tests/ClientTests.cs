using FluentAssertions;
using IdentityServer4.Models;
using Jp.Api.Management;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace JPProject.Api.Management.Tests
{
    public class ClientTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private HttpClient _client;

        public ClientTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }
        [Fact]
        public async Task Teste()
        {
            // The endpoint or route of the controller action.
            var httpResponse = await _client.GetAsync("/clients");

            httpResponse.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var clients = JsonConvert.DeserializeObject<IEnumerable<Client>>(stringResponse);

            clients.Should().BeEmpty();
        }
    }
}
