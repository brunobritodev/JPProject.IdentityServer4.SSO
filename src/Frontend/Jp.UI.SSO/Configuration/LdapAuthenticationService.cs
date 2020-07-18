using JPProject.Sso.Application.ViewModels.UserViewModels;
using Microsoft.Extensions.Options;
using System.DirectoryServices;
using Jp.UI.SSO.Interfaces;
using Jp.UI.SSO.Models;

namespace Jp.UI.SSO.Configuration
{
    public class LdapAuthenticationService : IAuthService
    {
        private const string DisplayNameAttribute = "DisplayName";
        private const string SAMAccountNameAttribute = "SAMAccountName";

        private readonly LdapConfig config;

        public LdapAuthenticationService(IOptions<LdapConfig> config)
        {
            this.config = config.Value;
        }
        public UserViewModel Login(string username, string password)
        {
            try
            {
                using (DirectoryEntry entry = new DirectoryEntry(config.Path, config.UserDomainName + "\\" + username, password))
                {
                    using (DirectorySearcher searcher = new DirectorySearcher(entry))
                    {
                        searcher.Filter = $"({SAMAccountNameAttribute}={username})";
                        searcher.PropertiesToLoad.Add(DisplayNameAttribute);
                        searcher.PropertiesToLoad.Add(SAMAccountNameAttribute);
                        var result = searcher.FindOne();
                        if (result != null)
                        {
                            var displayName = result.Properties[DisplayNameAttribute];
                            var samAccountName = result.Properties[SAMAccountNameAttribute];

                            return new UserViewModel()
                            {
                                Name = displayName == null || displayName.Count <= 0 ? null : displayName[0].ToString(),
                                UserName = samAccountName == null || samAccountName.Count <= 0 ? null : samAccountName[0].ToString()
                            };
                        }
                    }
                }
            }
            catch
            {
                // if we get an error, it means we have a login failure.
                // Log specific exception
            }
            return null;
        }


    }
}
