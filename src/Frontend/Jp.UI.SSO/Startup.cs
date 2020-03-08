using IdentityServer4.Services;
using Jp.Database.Context;
using Jp.UI.SSO.Configuration;
using Jp.UI.SSO.Util;
using JPProject.AspNet.Core;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.AspNetIdentity.Configuration;
using JPProject.Sso.AspNetIdentity.Models.Identity;
using JPProject.Sso.EntityFramework.Repository.Configuration;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Serilog;


namespace Jp.UI.SSO
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllersWithViews()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization();
            services.AddRazorPages();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.Secure = CookieSecurePolicy.SameAsRequest;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // The following line enables Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry();

            // Add localization
            services.AddMvcLocalization();

            // Dbcontext config
            services.ConfigureProviderForContext<SsoContext>(DetectDatabase);

            // ASP.NET Identity Configuration
            services
                .AddIdentity<UserIdentity, RoleIdentity>(AccountOptions.NistAccountOptions)
                .AddClaimsPrincipalFactory<ApplicationClaimsIdentityFactory>()
                .AddEntityFrameworkStores<SsoContext>()
                .AddDefaultTokenProviders();

            // Improve Identity password security
            services.UpgradePasswordSecurity().UseArgon2<UserIdentity>();


            // IdentityServer4 Configuration
            services
                .AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddAspNetIdentity<UserIdentity>()
                .ConfigureContext(DetectDatabase, _env)
                .AddProfileService<SsoProfileService>()
                // Configure key material. By default it supports load balance scenarios and have a key managemente close to Key Management from original IdentityServer4
                // Unless you really know what are you doing, change it.
                .SetupKeyMaterial();

            // SSO Configuration
            services
                .ConfigureSso<AspNetUser>()
                .AddSsoContext<SsoContext>()
                // If your ASP.NET Identity has additional fields, you can remove this line and implement IIdentityFactory<TUser> and IRoleFactory<TUser>
                // theses interfaces will able you to intercept Register / Update Flows from User and Roles
                .AddDefaultAspNetIdentityServices();

            // Configure Federation gateway (external logins), such as Facebook, Google etc
            services.AddFederationGateway(Configuration);

            // Adding MediatR for Domain Events and Notifications
            services.AddMediatR(typeof(Startup));

            // .NET Native DI Abstraction
            RegisterServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Only use HTTPS redirect in Production Ambients
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else if (env.IsProduction() && !env.IsBehindReverseProxy(Configuration))
            {
                app.UseHttpsRedirection();
                app.UseHsts(options => options.MaxAge(days: 365));
            }

            app.UseSerilogRequestLogging();
            app.UseSecurityHeaders(env, Configuration);
            app.UseStaticFiles();

            var fordwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            fordwardedHeaderOptions.KnownNetworks.Clear();
            fordwardedHeaderOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(fordwardedHeaderOptions);

            app.UseIdentityServer();
            app.UseLocalization();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        private void RegisterServices(IServiceCollection services)
        {
            // Adding dependencies from another layers (isolated from Presentation)
            services.AddScoped<IEventSink, IdentityServerEventStore>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        /// <summary>
        /// it's just a tuple. Returns 2 parameters.
        /// Trying to improve readability at ConfigureServices
        /// </summary>
        private (DatabaseType, string) DetectDatabase => (
            Configuration.GetValue<DatabaseType>("ApplicationSettings:DatabaseType"),
            Configuration.GetConnectionString("SSOConnection"));
    }


}
