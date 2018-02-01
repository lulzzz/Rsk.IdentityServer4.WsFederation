using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using IdentityServer4.Models;
using IdentityServer4.Test;
using IdentityServer4.WsFederation.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rsk.IdentityServer4.WsFederation.Configuration;

namespace Rsk.IdentityServer4.WsFederation.Tests.Host
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddIdentityServer()
                .AddInMemoryClients(new List<Client>())
                .AddInMemoryApiResources(new List<ApiResource>())
                .AddInMemoryIdentityResources(new List<IdentityResource>())
                .AddTestUsers(new List<TestUser>())
                .AddWsFederation()
                .AddInMemoryRelyingParties(new List<RelyingParty>())
                .AddSigningCredential(new X509Certificate2("Resources/idsrv3test.pfx", "idsrv3test"));
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            app.UseIdentityServer()
                .UseIdentityServerWsFederationPlugin();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
