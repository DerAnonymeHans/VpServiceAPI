using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using VpServiceAPI.Tools;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Jobs.Checking;
using VpServiceAPI.Jobs.Notification;
using VpServiceAPI.Jobs.Analysing;
using VpServiceAPI.Jobs.Routines;
using VpServiceAPI.Repositories;
using VpServiceAPI.Jobs.StatExtraction;
using VpServiceAPI.Jobs.StatProviding;
using VpServiceAPI.Middleware;
using Microsoft.AspNetCore.Http;

namespace VpServiceAPI
{
    public sealed class Startup
    {
        private IWebHostEnvironment Env { get; }
        public IConfiguration Configuration { get; }
        readonly string MyCorsPolicy = "_myCorsPolicy";

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization();
            services.AddAuthentication("cookieAuth")
                .AddCookie("cookieAuth", options =>
                {
                    options.LoginPath = "/Login";
                    options.Cookie.Name = "loginHelper";

                });

            services.AddCors(options =>
            {
                options.AddPolicy(name: MyCorsPolicy, policy =>
                    {
                        policy.WithOrigins("https://kepleraner.herokuapp.com", "http://kepleraner.herokuapp.com", "https://kepleraner-test.herokuapp.com", "http://kepleraner-test.herokuapp.com", "http://localhost:3000", "http://localhost:8080");
                        policy.AllowAnyHeader().AllowCredentials().AllowAnyMethod();
                    });
            });

            var dependencyInjector = new DependencyInjector(ref services);
            dependencyInjector.Inject();
            services.AddHttpContextAccessor();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "VpServiceAPI", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VpServiceAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(MyCorsPolicy);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStatisticAuthorizationMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            Tools.AppContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
        }
    }
}
