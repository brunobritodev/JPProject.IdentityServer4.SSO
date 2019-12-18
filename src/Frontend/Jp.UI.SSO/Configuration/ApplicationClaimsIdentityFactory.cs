using IdentityModel;
using JPProject.Domain.Core.StringUtils;
using JPProject.Sso.Infra.Identity.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jp.UI.SSO.Configuration
{
    public class ApplicationClaimsIdentityFactory : UserClaimsPrincipalFactory<UserIdentity>
    {
        public ApplicationClaimsIdentityFactory(UserManager<UserIdentity> userManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(UserIdentity user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            var claims = new List<Claim>();
            if (user.Birthdate.HasValue)
                claims.Add(new Claim(JwtClaimTypes.BirthDate, user.Birthdate.Value.ToString("yyyy-MM-dd")));

            if (user.Name.IsPresent())
                claims.Add(new Claim(JwtClaimTypes.GivenName, user.Name));
            else
                claims.Add(new Claim(JwtClaimTypes.GivenName, user.UserName));

            var roles = await UserManager.GetRolesAsync(user);

            claims.AddRange(roles.Select(s => new Claim(JwtClaimTypes.Role, s)));
            identity.AddClaims(claims);
            return identity;
        }
    }
}
