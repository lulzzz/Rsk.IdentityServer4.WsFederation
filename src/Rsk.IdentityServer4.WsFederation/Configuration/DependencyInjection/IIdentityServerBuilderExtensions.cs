using System;
using System.Collections.Generic;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Services;
using IdentityServer4.WsFederation;
using IdentityServer4.WsFederation.Hosting;
using IdentityServer4.WsFederation.Stores;
using IdentityServer4.WsFederation.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rsk.IdentityServer4.WsFederation;
using Rsk.IdentityServer4.WsFederation.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IIdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddWsFederation(this IIdentityServerBuilder builder, Action<WsFederationOptions> optionsAction = null)
        {
            var options = new WsFederationOptions();
            optionsAction?.Invoke(options);
            builder.Services.AddSingleton(options);

            var endpointPathToNameMap = new Dictionary<string, WsFederationEndpointName>
            {
                {options.WsFederationEndpoint, WsFederationEndpointName.WsFed}
            };

            builder.Services.AddSingleton<IWsFederationEndpointRouter>(resolver =>
                new WsFederationEndpointRouter(endpointPathToNameMap,
                    resolver.GetServices<WsFederationEndpointMapping>(),
                    resolver.GetRequiredService<ILogger<WsFederationEndpointRouter>>()));

            builder.AddWsFederationEndpoint<WsFederationEndpoint>(WsFederationEndpointName.WsFed);

            builder.Services.AddTransient<MetadataResponseGenerator>();
            builder.Services.AddTransient<SignInResponseGenerator>();
            builder.Services.AddTransient<SignInValidator>();
            builder.Services.AddTransient<IReturnUrlParser, WsFederationReturnUrlParser>();
            builder.Services.TryAddTransient<IRelyingPartyStore, NoRelyingPartyStore>();

            builder.Services.AddSingleton(
                resolver => resolver.GetRequiredService<IOptions<WsFederationOptions>>().Value);

            return builder;
        }

        public static IIdentityServerBuilder AddWsFederation(this IIdentityServerBuilder builder, IConfiguration configuration)
        {
            builder.Services.Configure<WsFederationOptions>(configuration);
            return builder.AddWsFederation();
        }

        public static IIdentityServerBuilder AddInMemoryRelyingParties(this IIdentityServerBuilder builder, IEnumerable<RelyingParty> relyingParties)
        {
            builder.Services.AddSingleton(relyingParties);
            builder.Services.AddSingleton<IRelyingPartyStore, InMemoryRelyingPartyStore>();

            return builder;
        }

        internal static IIdentityServerBuilder AddWsFederationEndpoint<T>(this IIdentityServerBuilder builder, WsFederationEndpointName endpoint)
            where T : class, IEndpointHandler
        {
            builder.Services.AddTransient<T>();
            builder.Services.AddSingleton(new WsFederationEndpointMapping { Endpoint = endpoint, Handler = typeof(T) });

            return builder;
        }
    }
}