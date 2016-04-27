using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WsFedAuth.Web.Settings;
using WsFedAuth.Web.Middleware.WsFedAuthentication;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Authentication.Cookies;

namespace WsFedAuth.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }


        public Startup(IHostingEnvironment env)
        {
          
            //build up the configuration
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();


        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            //Set up AppSettings options based on the section added to configuration
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<WsFedSettings>(Configuration.GetSection("WsFed"));

            services.AddScoped<CustomClaimsPrincipal>();

            services.AddWebEncoders();
            services.AddAuthentication();
            
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IOptions<WsFedSettings> wsFedSettings)
        {
            app.UseIISPlatformHandler();
            app.UseStaticFiles();

            //Configure STS Authentication
            app.UseWsFedAuthentication(options =>
            {
                var ws = wsFedSettings.Value;
                options.AuthenticationScheme = ws.AuthenticationScheme;
                options.ClaimsIssuer = ws.ClaimsIssuer;
                options.IdPEndpoint = ws.IdPEndpoint;
                options.IsPersistent = ws.IsPersistent; //this must be set to true for the ExpireTimeSpan option to have any effect.
                options.Realm = ws.Realm;
                options.SigningCertThumbprint = ws.SigningCertThumbprint;
                options.EncryptionCertStoreName = ws.EncryptionCertStoreName;
                options.CookieName = ws.CookieName;
                options.AccessDeniedPath = new PathString(ws.AccessDeniedPath);
                options.SlidingExpiration = ws.SlidingExpiration;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(ws.LoginTimeoutMinutes);

                options.AutomaticAuthenticate = true;
                options.AutomaticChallenge = true;
                options.CookieHttpOnly = true;
                
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
