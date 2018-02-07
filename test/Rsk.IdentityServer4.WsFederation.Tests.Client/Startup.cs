using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Rsk.IdentityServer4.WsFederation.Tests.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.Name = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddWsFederation(options =>
                {
                    options.MetadataAddress = "http://localhost:5000/wsfederation";
                    options.RequireHttpsMetadata = false;

                    options.Wtrealm = "urn:wsfedrp";

                    options.CallbackPath = "/";
                    options.SkipUnrecognizedRequests = true;
                });

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
