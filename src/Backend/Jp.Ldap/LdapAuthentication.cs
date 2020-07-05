using JPProject.Domain.Core.Util;
using JPProject.Sso.Application.ViewModels.UserViewModels;
using JPProject.Sso.Domain.ViewModels.Settings;
using System;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Jp.Ldap
{
    public class NativeLdapAuthentication : ILdapAuthentication
    {
        private readonly LdapSettings _settings;

        public NativeLdapAuthentication(LdapSettings settings)
        {
            _settings = settings;
        }

        public UserViewModel Login(string username, string password)
        {
            var tempDomainName = new StringBuilder(100);

            if (!string.IsNullOrEmpty(_settings.DomainName))
            {
                tempDomainName.Append(_settings.DomainName);
                tempDomainName.Append('\\');
            }

            tempDomainName.Append(username);
            var credential = new NetworkCredential(tempDomainName.ToString(), password);
            //var dcName = "brunobrito";
            //var dcSufix = "net";

            using var connection = new LdapConnection(_settings.Address);
            if (_settings.AuthType.IsPresent() && Enum.TryParse(typeof(AuthType), _settings.AuthType, out _))
                connection.AuthType = Enum.Parse<AuthType>(_settings.AuthType);

            connection.Bind(credential);
            //var _userStore = $"dc={dcName},dc={dcSufix}";
            var searchFilter = $"(SAMAccountName={username})";

            var attributes = _settings.Attributes?.Trim() == "" ? null : _settings.Attributes?.Split(",").Select(s => s.Trim());
            var searchRequest = new SearchRequest
            (
                _settings.DistinguishedName,
                searchFilter,
                Enum.Parse<SearchScope>(_settings.SearchScope),
                attributes?.ToArray()
            //new string[] { "displayName", "cn", "mail" }
            );
            // null pega todos os campos
            var response = (SearchResponse)connection.SendRequest(searchRequest);
            var uservm = new UserViewModel() { UserName = username, Name = username};
            foreach (SearchResultEntry entry in response.Entries)
            {
                foreach (DirectoryAttribute dirAttr in entry.Attributes.Values)
                {
                    uservm.CustomClaims.Add(new Claim(dirAttr.Name, dirAttr[0].ToString()));
                    Console.WriteLine($"Attribute={dirAttr.Name},Value={dirAttr[0]}");
                }
            }

            return uservm;
        }
    }
}
