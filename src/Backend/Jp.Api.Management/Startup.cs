using Hellang.Middleware.ProblemDetails;
using Jp.Api.Management.Configuration;
using Jp.Api.Management.Configuration.Authorization;
using Jp.Database.Context;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Jp.Api.Management
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvcCore(options =>
                {

                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                }).AddApiExplorer();


            services.AddProblemDetails(setup =>
            {
                setup.IncludeExceptionDetails = _ => Environment.IsDevelopment();
            });

            // Response compression
            services.AddBrotliCompression();

            // SSO configuration
            ConfigureApi(services);
            // Data protection to persiste keys in database. 
            // It's necessary for Load balance scenarios
            services.AddDataProtection().SetApplicationName("sso").PersistKeysToDbContext<SsoContext>();

            // Cors request
            services.ConfigureCors();

            // Configure policies
            services.AddPolicies();

            // configure auth Server
            services.ConfigureOAuth2Server(Configuration);

            // configure openapi
            services.AddSwagger(Configuration);


            // Adding MediatR for Domain Events and Notifications
            services.AddMediatR(typeof(Startup));

            // .NET Native DI Abstraction
            RegisterServices(services);
        }

        public virtual void ConfigureApi(IServiceCollection services)
        {
            services.ConfigureSsoApi(Configuration);
            // Adding MediatR for Domain Events and Notifications
            services.AddMediatR(typeof(Startup));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseDefaultCors();
            app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseProblemDetails();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SSO Api Management");
                c.OAuthClientId("Swagger");
                c.OAuthClientSecret("swagger");
                c.OAuthAppName("SSO Management Api");
                c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }


        private void RegisterServices(IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // Adding dependencies from another layers (isolated from Presentation)
        }
    }
}
