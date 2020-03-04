using IdentityModel;
using Jp.UI.SSO.Util;
using JPProject.Domain.Core.Util;
using JPProject.Sso.AspNetIdentity.Models.Identity;
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

            claims.AddIfDontExist(new Claim(JwtClaimTypes.Name, user.UserName));

            if (user.Birthdate.HasValue)
                claims.AddIfDontExist(new Claim(JwtClaimTypes.BirthDate, user.Birthdate.Value.ToString("yyyy-MM-dd")));

            if (user.Name.IsPresent())
                claims.AddIfDontExist(new Claim(JwtClaimTypes.GivenName, user.Name));
            else
                claims.AddIfDontExist(new Claim(JwtClaimTypes.GivenName, user.UserName));

            if (user.Picture.IsPresent())
                claims.AddIfDontExist(new Claim(JwtClaimTypes.Picture, user.Picture));

            if (user.SocialNumber.IsPresent())
                claims.AddIfDontExist(new Claim("social_number", user.SocialNumber));

            var roles = await UserManager.GetRolesAsync(user);

            if (identity.Claims.All(c => c.Type != JwtClaimTypes.Role))
                claims.AddRange(roles.Select(s => new Claim(JwtClaimTypes.Role, s)));

            identity.AddClaims(claims);
            return identity;
        }
    }
}