﻿using System;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.WsFederation.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Rsk.IdentityServer4.WsFederation.Endpoints.Results
{
    internal class WsFederationLoginPageResult : IEndpointResult
    {
        private readonly string request;
        private IdentityServerOptions options;
        private WsFederationOptions wsFedOptions;

        public WsFederationLoginPageResult(string request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        internal WsFederationLoginPageResult(string returnId, IdentityServerOptions options, WsFederationOptions wsFedOptions) : this(returnId)
        {
            this.options = options;
            this.wsFedOptions = wsFedOptions;
        }

        private void Init(HttpContext context)
        {
            options = options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
            wsFedOptions = wsFedOptions ?? context.RequestServices.GetRequiredService<WsFederationOptions>();
        }

        public Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            var returnUrl = context.GetIdentityServerBasePath().EnsureTrailingSlash() 
                            + wsFedOptions.WsFederationEndpoint.RemoveTrailingSlash()
                            + request;

            var loginUrl = options.UserInteraction.LoginUrl;
            if (!loginUrl.IsLocalUrl())
            {
                // convert the relative redirect path to absolute if redirecting to a different server
                returnUrl = context.GetIdentityServerOrigin().EnsureTrailingSlash() + returnUrl.RemoveLeadingSlash();
            }

            var url = loginUrl.AddQueryString(options.UserInteraction.LoginReturnUrlParameter, returnUrl);
            context.Response.RedirectToAbsoluteUrl(url);

            return Task.FromResult(0);
        }
    }
}