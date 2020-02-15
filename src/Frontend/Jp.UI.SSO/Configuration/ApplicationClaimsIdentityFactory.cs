using IdentityModel;
using JPProject.Domain.Core.StringUtils;
using JPProject.Sso.Infra.Identity.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jp.UI.SSO.Configuration
{
    public class ApplicationClaimsIdentityFactory : UserClaimsPrincipalFactory<UserIdentity>
    {
        private readonly IOptions<IdentityOptions> _optionsAccessor;

        public ApplicationClaimsIdentityFactory(
            UserManager<UserIdentity> userManager,
            IOptions<IdentityOptions> optionsAccessor
            ) : base(userManager, optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(UserIdentity user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            var claims = new List<Claim>();

            claims.Add(new Claim("username", user.UserName));

            // Email is loaded default by IdentityServer4
            //claims.Add(new Claim(JwtClaimTypes.Email, user.Email, ClaimValueTypes.Email));

            if (user.Birthdate.HasValue)
                claims.Add(new Claim(JwtClaimTypes.BirthDate, user.Birthdate.Value.ToString(CultureInfo.CurrentCulture), ClaimValueTypes.Date));

            if (user.Name.IsPresent())
                claims.Add(new Claim(JwtClaimTypes.Name, user.Name));

            if (user.Picture.IsPresent())
                claims.Add(new Claim(JwtClaimTypes.Picture, user.Picture));

            if (user.SocialNumber.IsPresent())
                claims.Add(new Claim("social_number", user.SocialNumber));

            var roles = await UserManager.GetRolesAsync(user);

            claims.AddRange(roles.Select(s => new Claim(JwtClaimTypes.Role, s)));
            identity.AddClaims(claims);
            return identity;
        }
    }
}
