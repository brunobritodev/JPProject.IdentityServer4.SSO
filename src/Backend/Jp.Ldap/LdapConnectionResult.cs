using JPProject.Domain.Core.ViewModels;
using System.Collections.Generic;

namespace Jp.Ldap
{
    public class LdapConnectionResult
    {
        public bool Success { get; }
        public List<ClaimViewModel> Claims { get; }
        public string Error { get; }
        public string Step { get; }

        public LdapConnectionResult(bool success, string error, string step)
        {
            Success = success;
            Error = error;
            Step = step;
        }

        public LdapConnectionResult(bool success, List<ClaimViewModel> claims)
        {
            Success = success;
            Claims = claims;
        }
    }
}