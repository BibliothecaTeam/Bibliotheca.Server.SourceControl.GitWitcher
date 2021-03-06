using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Bibliotheca.Server.Mvc.Middleware.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Swashbuckle.Swagger.Model;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient;
using System;
using System.Collections.Generic;
using Bibliotheca.Server.ServiceDiscovery.ServiceClient.Extensions;
using Bibliotheca.Server.SourceControl.GitWitcher.Core.Parameters;
using Bibliotheca.Server.SourceControl.GitWitcher.Core.Services;

namespace Bibliotheca.Server.SourceControl.GitWitcher.Core.Api
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        protected bool UseServiceDiscovery { get; set; } = true;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ApplicationParameters>(Configuration);

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(SecureTokenDefaults.AuthenticationScheme)
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
            });

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.ApiVersionReader = new QueryStringOrHeaderApiVersionReader("api-version");
            });

            services.AddSwaggerGen();
            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Source Control GitWitcher API",
                    Description = "Microservice for Git integration feature for Bibliotheca.",
                    TermsOfService = "None"
                });
            });

            services.AddServiceDiscovery();

            services.AddScoped<IQuestsService, QuestsService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (UseServiceDiscovery)
            {
                var options = GetServiceDiscoveryOptions();
                app.RegisterService(options);
            }

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseExceptionHandler();
            app.UseCors("AllowAllOrigins");

            var secureTokenOptions = new SecureTokenOptions
            {
                SecureToken = Configuration["SecureToken"],
                AuthenticationScheme = SecureTokenDefaults.AuthenticationScheme,
                Realm = SecureTokenDefaults.Realm
            };
            app.UseSecureTokenAuthentication(secureTokenOptions);

            var jwtBearerOptions = new JwtBearerOptions
            {
                Authority = Configuration["OAuthAuthority"],
                Audience = Configuration["OAuthAudience"],
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            };
            app.UseBearerAuthentication(jwtBearerOptions);

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi();
        }

        private ServiceDiscoveryOptions GetServiceDiscoveryOptions()
        {
            var serviceDiscoveryConfiguration = Configuration.GetSection("ServiceDiscovery");

            var tags = new List<string>();
            var tagsSection = serviceDiscoveryConfiguration.GetSection("ServiceTags");
            tagsSection.Bind(tags);

            var options = new ServiceDiscoveryOptions();
            options.ServiceOptions.Id = serviceDiscoveryConfiguration["ServiceId"];
            options.ServiceOptions.Name = serviceDiscoveryConfiguration["ServiceName"];
            options.ServiceOptions.Address = serviceDiscoveryConfiguration["ServiceAddress"];
            options.ServiceOptions.Port = Convert.ToInt32(serviceDiscoveryConfiguration["ServicePort"]);
            options.ServiceOptions.HttpHealthCheck = serviceDiscoveryConfiguration["ServiceHttpHealthCheck"];
            options.ServiceOptions.Tags = tags;
            options.ServerOptions.Address = serviceDiscoveryConfiguration["ServerAddress"];

            return options;
        }
    }
}
