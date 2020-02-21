using Bogus;
using JPProject.Sso.Application.ViewModels.EmailViewModels;
using JPProject.Sso.Domain.Models;

namespace JPProject.Api.Management.Tests.Fakers.EmailFakers
{
    public class EmailFakers
    {
        public static Faker<TemplateViewModel> GenerateTemplate()
        {
            return new Faker<TemplateViewModel>()
                .RuleFor(t => t.Subject, f => f.Lorem.Paragraph())
                .RuleFor(t => t.Content, f => f.Lorem.Paragraph())
                .RuleFor(t => t.Name, f => f.Internet.DomainName())
                .RuleFor(t => t.Username, f => f.Person.UserName)
                .RuleFor(t => t.OldName, f => f.Lorem.Paragraph());
        }
        public static Faker<EmailViewModel> GenerateEmail()
        {
            var sender = GenerateSender().Generate();
            return new Faker<EmailViewModel>()
                .RuleFor(t => t.Subject, f => f.Lorem.Paragraph())
                .RuleFor(t => t.Content, f => f.Lorem.Paragraph())
                .RuleFor(t => t.Username, f => f.Person.UserName)
                .RuleFor(t => t.Type, f => f.PickRandom<EmailType>())
                .RuleFor(t => t.Sender, f => sender);
        }

        public static Faker<Sender> GenerateSender()
        {
            return new Faker<Sender>().CustomInstantiator(s => new Sender(s.Person.Email, s.Person.FullName));
        }
    }
}
