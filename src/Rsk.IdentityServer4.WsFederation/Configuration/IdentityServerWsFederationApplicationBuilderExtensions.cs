using System;
using System.Security.Cryptography;
using IdentityServer4.Services;
using IdentityServer4.WsFederation.Hosting;
using IdentityServer4.WsFederation.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Rsk.IdentityServer4.WsFederation.Configuration
{
    public static class IdentityServerWsFederationApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseIdentityServerWsFederationPlugin(this IApplicationBuilder app)
        {
            app.Validate();

            app.UseMiddleware<IdentityServerWsFederationMiddleware>();

            return app;
        }

        internal static void Validate(this IApplicationBuilder app)
        {
            var loggerFactory = app.ApplicationServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            var logger = loggerFactory.CreateLogger("Rsk.IdentityServer4.WsFederation.Startup");

            var scopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var store = serviceProvider.GetService<IRelyingPartyStore>();
                if (store == null)
                {
                    const string error = "No storage mechanism for WS-Federation relying parties specified. Use the 'AddInMemoryRelyingParties' extension method to register a development version.";
                    logger.LogCritical(error);
                    throw new InvalidOperationException(error);
                }

                var keyService = serviceProvider.GetService<IKeyMaterialService>();
                var signingKey = (keyService.GetSigningCredentialsAsync().Result)?.Key as X509SecurityKey;

                try
                {
                    var key = signingKey?.PrivateKey;
                }
                catch (CryptographicException)
                {
                    const string error = "Invalid signing key type. Unable to find private key.";
                    logger.LogCritical(error);
                    throw new InvalidOperationException(error);
                }
            }
        }
    }
}