using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Text;
using JPProject.Domain.Core.Util;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.Domain.ViewModels.Settings;

namespace Jp.Ldap.Test
{
    public class NativeLdapTestConnection : ILdapTestConnection
    {
        private readonly LdapSettings _settings;

        public NativeLdapTestConnection(LdapSettings settings)
        {
            _settings = settings;
        }

        public LdapConnectionResult Test(string username, string password)
        {
            var tempDomainName = new StringBuilder(100);

            if (!string.IsNullOrEmpty(_settings.DomainName))
            {
                tempDomainName.Append(_settings.DomainName);
                tempDomainName.Append('\\');
            }

            tempDomainName.Append(username);
            var credential = new NetworkCredential(tempDomainName.ToString(), password);
            LdapConnection connection;

            try
            {
                connection = new LdapConnection(_settings.Address);
                if (_settings.AuthType.IsPresent() && Enum.TryParse(typeof(AuthType), _settings.AuthType, out _))
                    connection.AuthType = Enum.Parse<AuthType>(_settings.AuthType);

                connection.Bind(credential);
            }
            catch (Exception e)
            {
                return new LdapConnectionResult(false, e.Message, "Login");
            }

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

            var claims = new List<ClaimViewModel>();
            try
            {
                var response = (SearchResponse)connection.SendRequest(searchRequest);
                foreach (SearchResultEntry entry in response.Entries)
                {
                    foreach (DirectoryAttribute dirAttr in entry.Attributes.Values)
                    {
                        claims.Add(new ClaimViewModel(dirAttr.Name, dirAttr[0].ToString()));
                    }
                }
            }
            catch (Exception e)
            {
                return new LdapConnectionResult(false, e.Message, "Search Error");
            }

            return new LdapConnectionResult(true, claims.OrderBy(b => b.Type).ToList());
        }


    }
}
