using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Jp.UI.SSO.Util;
using JPProject.Sso.AspNetIdentity.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jp.UI.SSO.Configuration
{
    public class SsoProfileService : IProfileService
    {
        protected UserManager<UserIdentity> UserManager;
        private readonly ILogger<DefaultProfileService> Logger;
        private readonly IUserClaimsPrincipalFactory<UserIdentity> _claimsFactory;
        private readonly IResourceStore _resourceStore;

        public SsoProfileService(
            UserManager<UserIdentity> userManager,
            ILogger<DefaultProfileService> logger,
            IUserClaimsPrincipalFactory<UserIdentity> claimsFactory,
            IResourceStore resourceStore)
        {
            UserManager = userManager;
            Logger = logger;
            _claimsFactory = claimsFactory;
            // IResourceStore will be cached in production, while IResourceRepo won't
            _resourceStore = resourceStore;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // Get user data
            var user = await UserManager.GetUserAsync(context.Subject);

            // Load user claims from ASP.NET Identity
            var principal = await _claimsFactory.CreateAsync(user);

            var claimsUser = principal.Claims.ToList();
            var subjectClaims = context.Subject.Claims.ToList();

            // Merge ASP.NET Identity claims
            subjectClaims.Merge(claimsUser);
            
            subjectClaims.AddIfDontExist(new Claim("username", user.UserName));
            subjectClaims.AddIfDontExist(new Claim(JwtClaimTypes.Name, user.UserName));

            if (subjectClaims.All(a => a.Type != JwtClaimTypes.Role))
            {
                var roles = await UserManager.GetRolesAsync(user);
                subjectClaims.AddRange(roles.Select(s => new Claim(JwtClaimTypes.Role, s)));
            }

            // Sometimes IdentityResources are specified at UserClaims in ProtectedResource. Then we include all related claims to RequestedClaims
            var resources = await _resourceStore.GetAllResourcesAsync();
            var usersClaimsToGoWithin = GetIdentityResourcesToIncludeInRequestedClaims(context, resources);
            usersClaimsToGoWithin.Merge(context.RequestedClaimTypes);
            context.RequestedClaimTypes = usersClaimsToGoWithin;

            context.LogProfileRequest(Logger);
            context.AddRequestedClaims(subjectClaims);
            context.LogIssuedClaims(Logger);
        }

        private static List<string> GetIdentityResourcesToIncludeInRequestedClaims(ProfileDataRequestContext context,
            Resources resources)
        {
            var usersClaimsToGoWithin = new List<string>();
            foreach (var contextRequestedClaimType in context.RequestedClaimTypes)
            {
                if (resources.IdentityResources.Any(a => a.Name == contextRequestedClaimType))
                {
                    usersClaimsToGoWithin.AddRange(resources.IdentityResources
                        .FirstOrDefault(f => f.Name == contextRequestedClaimType).UserClaims);
                }
            }

            return usersClaimsToGoWithin;
        }


        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await UserManager.GetUserAsync(context.Subject);

            // In case admin is accidentally blocked.
            var isBlocked = user.LockoutEnabled && user.LockoutEnd.GetValueOrDefault(DateTimeOffset.UtcNow.UtcDateTime.Date) > DateTimeOffset.UtcNow.UtcDateTime;
            if (isBlocked)
                isBlocked = await UserManager.IsInRoleAsync(user, "Administrator");

            context.IsActive = !isBlocked;
        }
    }
}