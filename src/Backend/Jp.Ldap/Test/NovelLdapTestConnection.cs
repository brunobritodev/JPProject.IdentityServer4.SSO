using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Text;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.Domain.ViewModels.Settings;
using Novell.Directory.Ldap;
using LdapConnection = Novell.Directory.Ldap.LdapConnection;
using LdapException = Novell.Directory.Ldap.LdapException;

namespace Jp.Ldap.Test
{
    public class NovelLdapTestConnection : ILdapTestConnection
    {
        private readonly LdapSettings _settings;

        public NovelLdapTestConnection(LdapSettings settings)
        {
            _settings = settings;
        }

        public LdapConnectionResult Test(string username, string password)
        {
            // Creating an LdapConnection instance 
            var ldapConn = new LdapConnection();
            var tempDomainName = new StringBuilder(100);

            if (!string.IsNullOrEmpty(_settings.DomainName))
            {
                tempDomainName.Append(_settings.DomainName);
                tempDomainName.Append('\\');
            }

            tempDomainName.Append(username);
            try
            {
                //Connect function will create a socket connection to the server
                ldapConn.Connect(_settings.Address, _settings.PortNumber);

                //Bind function will Bind the user object Credentials to the Server
                ldapConn.Bind(tempDomainName.ToString(), password);
            }
            catch (Exception e)
            {
                return new LdapConnectionResult(false, e.Message, "Login");
            }

            // Searches in the Marketing container and return all child entries just below this
            //container i.e. Single level search

            var claims = new List<ClaimViewModel>();
            try
            {
                var cons = ldapConn.SearchConstraints;
                cons.ReferralFollowing = true;
                ldapConn.Constraints = cons;

                var attributes = _settings.Attributes?.Trim() == "" ? null : _settings.Attributes?.Split(",").Select(s => s.Trim());
                var lsc = ldapConn.Search(_settings.DistinguishedName,
                    (int)Enum.Parse<SearchScope>(_settings.SearchScope),
                $"(sAMAccountName={username})",
                    attributes?.ToArray(),
                false,
               (LdapSearchConstraints)null);

                while (lsc.HasMore())
                {
                    LdapEntry nextEntry = null;
                    try
                    {
                        nextEntry = lsc.Next();
                    }
                    catch (LdapException e)
                    {
                        ldapConn.Disconnect();
                        return new LdapConnectionResult(false, e.Message, "Search Error");
                    }
                    var attributeSet = nextEntry.GetAttributeSet();
                    System.Collections.IEnumerator ienum = attributeSet.GetEnumerator();
                    while (ienum.MoveNext())
                    {
                        var attribute = (LdapAttribute)ienum.Current;
                        var attributeName = attribute.Name;
                        var attributeVal = attribute.StringValue;

                        claims.Add(new ClaimViewModel(attributeName, attributeVal));
                    }
                }
            }
            catch (Exception e)
            {
                ldapConn.Disconnect();
                return new LdapConnectionResult(false, e.Message, "Search Error");
            }

            ldapConn.Disconnect();
            return new LdapConnectionResult(true, claims.OrderBy(b => b.Type).ToList());
        }
    }
}
