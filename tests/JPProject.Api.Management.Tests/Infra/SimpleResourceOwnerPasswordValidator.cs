using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JPProject.Api.Management.Tests.Infra
{
    public class SimpleResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (context.UserName != "user" || context.Password != "password")
            {
                context.Result =
                    new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Username or password is wrong!");
                return Task.CompletedTask;
            }

            context.Result = new GrantValidationResult(
                Guid.NewGuid().ToString(),
                OidcConstants.AuthenticationMethods.Password,
                new List<Claim>
                {
                    new Claim(JwtClaimTypes.Subject, "user"),
                    new Claim("username", "user"),
                    new Claim("is4-rights", "manager"),
                    new Claim("roles", "Administrator")
                });

            return Task.CompletedTask;
        }
    }
}
