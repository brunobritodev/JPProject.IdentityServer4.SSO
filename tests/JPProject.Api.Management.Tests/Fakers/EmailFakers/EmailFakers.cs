using Bogus;
using JPProject.Sso.Application.ViewModels.EmailViewModels;

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
    }
}
