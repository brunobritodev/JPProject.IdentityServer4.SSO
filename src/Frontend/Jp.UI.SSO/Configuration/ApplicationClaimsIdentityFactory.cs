using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using JPProject.Domain.Core.StringUtils;
using JPProject.Sso.AspNetIdentity.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jp.UI.SSO.Configuration
{
    public class SsoProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<UserIdentity> _claimsFactory;
        private readonly UserManager<UserIdentity> _userManager;

        public SsoProfileService(UserManager<UserIdentity> userManager,
            IUserClaimsPrincipalFactory<UserIdentity> claimsFactory)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
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

            if (_userManager.SupportsUserRole)
            {
                var roles = await _userManager.GetRolesAsync(user);
                claims.AddRange(roles.Select(s => new Claim(JwtClaimTypes.Role, s)));
            }

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }

    }
}
