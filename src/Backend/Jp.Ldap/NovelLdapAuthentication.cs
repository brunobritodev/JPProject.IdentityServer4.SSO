using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using JPProject.Domain.Core.Util;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.Application.ViewModels.UserViewModels;
using JPProject.Sso.Domain.ViewModels.Settings;
using Novell.Directory.Ldap;
using LdapConnection = Novell.Directory.Ldap.LdapConnection;
using LdapException = Novell.Directory.Ldap.LdapException;

namespace Jp.Ldap
{
    public class NovelLdapAuthentication : ILdapAuthentication
    {
        private readonly LdapSettings _settings;

        public NovelLdapAuthentication(LdapSettings settings)
        {
            _settings = settings;
        }

        public UserViewModel Login(string username, string password)
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
            //Connect function will create a socket connection to the server
            ldapConn.Connect(_settings.Address, _settings.PortNumber);

            //Bind function will Bind the user object Credentials to the Server
            ldapConn.Bind(tempDomainName.ToString(), password);


            var uservm = new UserViewModel() { UserName = username, Name = username };
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
                nextEntry = lsc.Next();
                var attributeSet = nextEntry.GetAttributeSet();
                System.Collections.IEnumerator ienum = attributeSet.GetEnumerator();
                while (ienum.MoveNext())
                {
                    var attribute = (LdapAttribute)ienum.Current;
                    var attributeName = attribute.Name;
                    var attributeVal = attribute.StringValue;

                    uservm.CustomClaims.Add(new Claim(attributeName, attributeVal));
                }
            }

            return uservm;
        }
    }
}