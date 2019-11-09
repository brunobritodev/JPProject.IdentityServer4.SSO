using Bogus;
using JPProject.Sso.Application.ViewModels;
using JPProject.Sso.Application.ViewModels.UserViewModels;

namespace JPProject.Api.Management.Tests.Fakers.UserFakers
{
    public static class UserViewModelFaker
    {
        private static Faker _faker = new Faker();
        public static Faker<RegisterUserViewModel> GenerateUserViewModel()
        {
            var pass = _faker.Internet.Password();
            return new Faker<RegisterUserViewModel>()
                .RuleFor(r => r.Email, f => f.Person.Email)
                .RuleFor(r => r.Password, f => pass)
                .RuleFor(r => r.ConfirmPassword, f => pass)
                .RuleFor(r => r.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(r => r.Name, f => f.Person.FullName)
                .RuleFor(r => r.Username, f => f.Person.UserName)
                .RuleFor(r => r.Picture, f => f.Person.Avatar);
        }
        public static Faker<RegisterUserViewModel> GenerateUserWithProviderViewModel()
        {
            var pass = _faker.Internet.Password();
            return new Faker<RegisterUserViewModel>()
                .RuleFor(r => r.Email, f => f.Person.Email)
                .RuleFor(r => r.Provider, f => f.Company.CompanyName())
                .RuleFor(r => r.ProviderId, f => f.Random.AlphaNumeric(21))
                .RuleFor(r => r.Name, f => f.Person.FullName)
                .RuleFor(r => r.Username, f => f.Person.UserName)
                .RuleFor(r => r.Picture, f => f.Person.Avatar);
        }

        public static Faker<SocialViewModel> GenerateSocialViewModel(string email = null, string username = null)
        {
            return new Faker<SocialViewModel>()
                .RuleFor(r => r.Email, f => email ?? f.Person.Email)
                .RuleFor(r => r.Provider, f => f.Company.CompanyName())
                .RuleFor(r => r.ProviderId, f => f.Random.AlphaNumeric(21))
                .RuleFor(r => r.Name, f => f.Person.FullName)
                .RuleFor(r => r.Username, f => username ?? f.Person.UserName)
                .RuleFor(r => r.Picture, f => f.Person.Avatar);
        }

        public static Faker<SaveUserClaimViewModel> GenerateClaim(string username = null)
        {
            return new Faker<SaveUserClaimViewModel>()
                .RuleFor(s => s.Value, f => f.Lorem.Word())
                .RuleFor(s => s.Type, f => f.Person.Email)
                .RuleFor(s => s.Username, f => username ?? f.Internet.UserName());
        }
    }
}
