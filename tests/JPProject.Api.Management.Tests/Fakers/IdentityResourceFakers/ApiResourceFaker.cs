using Bogus;
using IdentityServer4.Models;

namespace JPProject.Api.Management.Tests.Fakers.IdentityResourceFakers
{
    public class ApiResourceFaker
    {
        public static Faker<ApiResource> GenerateApiResource()
        {
            return new Faker<ApiResource>()
                .RuleFor(a => a.Enabled, f => f.Random.Bool())
                .RuleFor(a => a.Name, f => f.Internet.DomainName())
                .RuleFor(a => a.DisplayName, f => f.Lorem.Paragraph())
                .RuleFor(a => a.Description, f => f.Lorem.Paragraph());

        }
    }
}