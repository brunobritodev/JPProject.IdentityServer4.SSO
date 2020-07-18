using JPProject.Sso.Domain.ViewModels.Settings;

namespace Jp.Api.Management.ViewModel
{
    public class LdapSettingsTestQuery
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DomainName { get; set; }
        public string DistinguishedName { get; set; }
        public string Attributes { get; set; }
        public string AuthType { get; set; }
        public string SearchScope { get; set; }
        public string PortNumber { get; set; }
        public string ConnectionLess { get; set; }
        public string FullyQualifiedDomainName { get; set; }
        public string Address { get; set; }
        public LdapSettings Get()
        {
            return new LdapSettings(DomainName, DistinguishedName, Attributes, AuthType, SearchScope, PortNumber, FullyQualifiedDomainName, ConnectionLess, Address);
        }
    }
}
