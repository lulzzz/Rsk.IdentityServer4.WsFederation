﻿using System;
using System.Collections.Generic;
using IdentityServer4.Services;
using IdentityServer4.WsFederation;
using IdentityServer4.WsFederation.Stores;
using IdentityServer4.WsFederation.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Rsk.IdentityServer4.WsFederation.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IIdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddWsFederation(this IIdentityServerBuilder builder)
        {
            builder.Services.AddTransient<MetadataResponseGenerator>();
            builder.Services.AddTransient<SignInResponseGenerator>();
            builder.Services.AddTransient<SignInValidator>();
            builder.Services.AddTransient<IReturnUrlParser, WsFederationReturnUrlParser>();
            builder.Services.TryAddTransient<IRelyingPartyStore, NoRelyingPartyStore>();

            builder.Services.AddSingleton(
                resolver => resolver.GetRequiredService<IOptions<WsFederationOptions>>().Value);

            return builder;
        }

        public static IIdentityServerBuilder AddWsFederation(this IIdentityServerBuilder builder, Action<WsFederationOptions> setupAction)
        {
            builder.Services.Configure(setupAction);
            return builder.AddWsFederation();
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

    }
}