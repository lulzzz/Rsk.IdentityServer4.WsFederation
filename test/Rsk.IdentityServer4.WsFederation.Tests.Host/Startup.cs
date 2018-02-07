using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using IdentityServer4.WsFederation;
using IdentityServer4.WsFederation.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Rsk.IdentityServer4.WsFederation.Tests.Host
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var identityResources = new List<IdentityResource> {new IdentityResources.OpenId(), new IdentityResources.Profile(), new IdentityResources.Email()};
            var wsFedClient = new Client
            {
                ClientId = "urn:wsfedrp",
                ClientName = "WS-Federation Relying Party",
                ProtocolType = IdentityServerConstants.ProtocolTypes.WsFederation,
                RedirectUris = {"http://localhost:5001"},
                IdentityTokenLifetime = 36000, // saml token lifetime
                AllowedScopes = {"openid", "profile", "email"}
            };
            var wsFedRelyingParty = new RelyingParty
            {
                Realm = "urn:wsfedrp",
                TokenType = WsFederationConstants.TokenTypes.Saml2TokenProfile11
            };
            var user = new TestUser
            {
                SubjectId = "4a52a9acf0f94662a6c53a6ee38553d2",
                Username = "scott",
                Password = "scott",
                Claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Email, "info@rocksolidknowledge.com"),
                    new Claim(JwtClaimTypes.GivenName, "scott"),
                    new Claim(JwtClaimTypes.FamilyName, "brady")
                }
            };

            services.AddIdentityServer()
                .AddInMemoryClients(new List<Client> {wsFedClient})
                .AddInMemoryApiResources(new List<ApiResource>())
                .AddInMemoryIdentityResources(identityResources)
                .AddTestUsers(new List<TestUser> {user})
                .AddWsFederation()
                .AddInMemoryRelyingParties(new List<RelyingParty> {wsFedRelyingParty})
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
