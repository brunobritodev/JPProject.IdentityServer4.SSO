using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using Jp.Database.Context;
using JPProject.EntityFrameworkCore.MigrationHelper;
using JPProject.Sso.AspNetIdentity.Models.Identity;
using JPProject.Sso.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jp.UI.SSO.Util
{
    public static class DbMigrationHelpers
    {
        /// <summary>
        /// Generate migrations before running this method, you can use command bellow:
        /// Nuget package manager: Add-Migration DbInit -context ApplicationIdentityContext -output Data/Migrations
        /// Dotnet CLI: dotnet ef migrations add DbInit -c ApplicationIdentityContext -o Data/Migrations
        /// </summary>
        public static async Task EnsureSeedData(IServiceScope serviceScope)
        {
            var services = serviceScope.ServiceProvider;
            await EnsureSeedData(services);
        }

        public static async Task EnsureSeedData(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var ssoContext = scope.ServiceProvider.GetRequiredService<SsoContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserIdentity>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RoleIdentity>>();

                await DbHealthChecker.TestConnection(ssoContext);

                if (env.IsDevelopment())
                    ssoContext.Database.EnsureCreated();

                await EnsureSeedIdentityServerData(ssoContext, configuration);
                await EnsureSeedIdentityData(userManager, roleManager, configuration);
                await EnsureSeedGlobalConfigurationData(ssoContext, configuration, env);
            }
        }

        private static async Task EnsureSeedGlobalConfigurationData(SsoContext context, IConfiguration configuration, IWebHostEnvironment env)
        {

            var ssoVersion = context.GlobalConfigurationSettings.FirstOrDefault(w => w.Key == "SSO:Version");
            SsoVersion.Current = new Version(ssoVersion?.Value ?? "3.1.1");

            if (!context.GlobalConfigurationSettings.Any())
            {
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("SSO:Version", "3.1.1", false, true));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("SendEmail", configuration.GetSection("EmailConfiguration:SendEmail").Value, false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("UseStorage", configuration.GetSection("Storage:UseStorage").Value, false, false));

                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Smtp:Server", configuration.GetSection("EmailConfiguration:SmtpServer").Value, false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Smtp:Port", configuration.GetSection("EmailConfiguration:SmtpPort").Value, false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Smtp:UseSsl", configuration.GetSection("EmailConfiguration:UseSsl").Value, false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Smtp:Username", configuration.GetSection("EmailConfiguration:SmtpUsername").Value, true, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Smtp:Password", configuration.GetSection("EmailConfiguration:SmtpPassword").Value, true, false));

                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Storage:Service", configuration.GetSection("Storage:Service").Value, false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Storage:VirtualPath", configuration.GetSection("Storage:VirtualPath").Value, false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Storage:Username", configuration.GetSection("Storage:Username").Value, true, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Storage:Password", configuration.GetSection("Storage:Password").Value, true, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Storage:StorageName", configuration.GetSection("Storage:StorageName").Value, false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Storage:BasePath", configuration.GetSection("Storage:BasePath").Value, false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Storage:PhysicalPath", env.WebRootPath, false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Storage:Region", configuration.GetSection("Storage:Region").Value, false, false));

                await context.SaveChangesAsync();
            }

            if (!context.Emails.Any())
            {
                var newUserEmail = File.ReadAllText(Path.Combine(env.ContentRootPath, @"Assets/templates/new-user-email.html"));
                var resetPasswordEmail = File.ReadAllText(Path.Combine(env.ContentRootPath, @"Assets/templates/reset-password-email.html"));
                var template = File.ReadAllText(Path.Combine(env.ContentRootPath, @"Assets/templates/default-template.html"));

                await context.Emails.AddAsync(new Email(newUserEmail, "Welcome to JP Project - Confirm your e-mail", new Sender("jpteam@jpproject.net", "JP Team"), EmailType.NewUser, null));
                await context.Emails.AddAsync(new Email(newUserEmail, "Welcome to JP Project - Confirm your e-mail", new Sender("jpteam@jpproject.net", "JP Team"), EmailType.NewUserWithoutPassword, null));
                await context.Emails.AddAsync(new Email(resetPasswordEmail, "JP Project - Reset Password", new Sender("jpteam@jpproject.net", "JP Team"), EmailType.RecoverPassword, null));

                await context.Templates.AddRangeAsync(new Template(template, "JP Team", "default-template", Users.GetEmail(configuration)));

                await context.SaveChangesAsync();
            }

            if (SsoVersion.Current <= Version.Parse("3.1.1"))
            {
                var claims = await context.UserClaims.Where(w => w.ClaimType == "username" || w.ClaimType == "email" || w.ClaimType == "picture").ToListAsync();
                context.UserClaims.RemoveRange(claims);

                if (context.Clients.Include(c => c.AllowedGrantTypes).Any(s => s.ClientId == "IS4-Admin" && s.AllowedGrantTypes.Any(a => a.GrantType == "implicit")))
                {
                    var clientAdmin = context.Clients.Include(c => c.AllowedGrantTypes).FirstOrDefault(s => s.ClientId == "IS4-Admin");
                    clientAdmin.RequireClientSecret = false;
                    clientAdmin.AllowedGrantTypes.RemoveAll(a => a.ClientId == clientAdmin.Id);
                    clientAdmin.AllowedGrantTypes.Add(new ClientGrantType()
                    {
                        ClientId = clientAdmin.Id,
                        GrantType = "authorization_code"
                    });
                    context.Update(clientAdmin);
                }
                await context.SaveChangesAsync();
            }


            if (SsoVersion.Current == Version.Parse("3.1.1"))
            {
                ssoVersion = context.GlobalConfigurationSettings.FirstOrDefault(w => w.Key == "SSO:Version");
                ssoVersion.Update("3.2.2", true, false);
                SsoVersion.Current = new Version(ssoVersion.Value);
                await context.SaveChangesAsync();
            }

            if (SsoVersion.Current == Version.Parse("3.2.2"))
            {
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("UseRecaptcha", configuration.GetSection("Recaptcha:UseRecaptcha").Value, false, true));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Recaptcha:SiteKey", configuration.GetSection("Recaptcha:SiteKey").Value, false, true));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Recaptcha:PrivateKey", configuration.GetSection("Recaptcha:PrivateKey").Value, true, false));

                ssoVersion = context.GlobalConfigurationSettings.FirstOrDefault(w => w.Key == "SSO:Version");
                ssoVersion.Update("3.2.3", true, false);
                SsoVersion.Current = new Version(ssoVersion.Value);
                await context.SaveChangesAsync();
            }

            if (SsoVersion.Current == Version.Parse("3.2.3"))
            {
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Ldap:DomainName", "", false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Ldap:DistinguishedName", "", false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Ldap:SearchScope", "", false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Ldap:Attributes", "", false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Ldap:AuthType", "", false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("LoginStrategy", "Identity", false, false));

                ssoVersion = context.GlobalConfigurationSettings.FirstOrDefault(w => w.Key == "SSO:Version");
                ssoVersion.Update("3.2.4", true, false);
                SsoVersion.Current = new Version(ssoVersion.Value);
                await context.SaveChangesAsync();
            }

            if (SsoVersion.Current == Version.Parse("3.2.4"))
            {
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Ldap:FullyQualifiedDomainName", "", false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Ldap:ConnectionLess", "", false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Ldap:PortNumber", "389", false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Ldap:Address", "", false, false));

                ssoVersion = context.GlobalConfigurationSettings.FirstOrDefault(w => w.Key == "SSO:Version");
                ssoVersion.Update("3.2.5", true, false);
                SsoVersion.Current = new Version(ssoVersion.Value);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Generate default admin user / role
        /// </summary>
        private static async Task EnsureSeedIdentityData(
            UserManager<UserIdentity> userManager,
            RoleManager<RoleIdentity> roleManager,
            IConfiguration configuration)
        {

            // Create admin role
            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                var role = new RoleIdentity { Name = "Administrator" };

                await roleManager.CreateAsync(role);
            }

            // Create admin user
            if (await userManager.FindByNameAsync(Users.GetUser(configuration)) != null) return;

            var user = new UserIdentity
            {
                UserName = Users.GetUser(configuration),
                Email = Users.GetEmail(configuration),
                EmailConfirmed = true,
                LockoutEnd = null
            };

            var result = await userManager.CreateAsync(user, Users.GetPassword(configuration));

            if (result.Succeeded)
            {
                await userManager.AddClaimAsync(user, new Claim("is4-rights", "manager"));
                await userManager.AddClaimAsync(user, new Claim("username", Users.GetUser(configuration)));
                await userManager.AddClaimAsync(user, new Claim("email", Users.GetEmail(configuration)));
                await userManager.AddToRoleAsync(user, "Administrator");
            }
        }

        /// <summary>
        /// Generate default clients, identity and api resources
        /// </summary>
        private static async Task EnsureSeedIdentityServerData(SsoContext context, IConfiguration configuration)
        {
            if (!context.Clients.Any())
            {
                foreach (var client in Clients.GetAdminClient(configuration).ToList())
                {
                    await context.Clients.AddAsync(client.ToEntity());
                }

                await context.SaveChangesAsync();
            }

            if (!context.IdentityResources.Any())
            {
                var identityResources = ClientResources.GetIdentityResources().ToList();

                foreach (var resource in identityResources)
                {
                    await context.IdentityResources.AddAsync(resource.ToEntity());
                }

                await context.SaveChangesAsync();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in ClientResources.GetApiResources().ToList())
                {
                    await context.ApiResources.AddAsync(resource.ToEntity());
                }

                await context.SaveChangesAsync();
            }
        }

    }
}
