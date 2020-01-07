using IdentityServer4.EntityFramework.Mappers;
using JPProject.EntityFrameworkCore.Context;
using JPProject.EntityFrameworkCore.MigrationHelper;
using JPProject.Sso.Domain.Models;
using JPProject.Sso.Infra.Data.Context;
using JPProject.Sso.Infra.Identity.Models.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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
                var ssoContext = scope.ServiceProvider.GetRequiredService<ApplicationSsoContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserIdentity>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                await DbHealthChecker.TestConnection(ssoContext);

                await ssoContext.Database.MigrateAsync();
                await scope.ServiceProvider.GetRequiredService<EventStoreContext>().Database.MigrateAsync();


                await EnsureSeedIdentityServerData(ssoContext, configuration);
                await EnsureSeedIdentityData(userManager, roleManager, configuration);
                await EnsureSeedGlobalConfigurationData(ssoContext, configuration, env);
            }
        }

        private static async Task EnsureSeedGlobalConfigurationData(ApplicationSsoContext context,
            IConfiguration configuration, IWebHostEnvironment env)
        {
            if (!context.GlobalConfigurationSettings.Any())
            {
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Smtp:Server", configuration.GetSection("EmailConfiguration:SmtpServer").Value, false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Smtp:Port", configuration.GetSection("EmailConfiguration:SmtpPort").Value, false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Smtp:UseSsl", configuration.GetSection("EmailConfiguration:UseSsl").Value, false, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Smtp:Username", configuration.GetSection("EmailConfiguration:SmtpUsername").Value, true, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("Smtp:Password", configuration.GetSection("EmailConfiguration:SmtpPassword").Value, true, false));
                await context.GlobalConfigurationSettings.AddAsync(new GlobalConfigurationSettings("SendEmail", configuration.GetSection("EmailConfiguration:SendEmail").Value, false, false));

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
        }

        /// <summary>
        /// Generate default admin user / role
        /// </summary>
        private static async Task EnsureSeedIdentityData(
            UserManager<UserIdentity> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {

            // Create admin role
            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                var role = new IdentityRole { Name = "Administrator" };

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
        private static async Task EnsureSeedIdentityServerData(ApplicationSsoContext context, IConfiguration configuration)
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
