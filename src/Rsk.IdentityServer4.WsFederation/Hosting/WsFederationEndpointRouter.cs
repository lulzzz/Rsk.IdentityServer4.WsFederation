using System;
using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.WsFederation.Hosting
{
    internal class WsFederationEndpointRouter : IWsFederationEndpointRouter
    {
        private readonly Dictionary<string, WsFederationEndpointName> pathToNameMap;
        private readonly IEnumerable<WsFederationEndpointMapping> mappings;
        private readonly ILogger<WsFederationEndpointRouter> logger;

        public WsFederationEndpointRouter(Dictionary<string, WsFederationEndpointName> pathToNameMap, IEnumerable<WsFederationEndpointMapping> mappings, ILogger<WsFederationEndpointRouter> logger)
        {
            this.pathToNameMap = pathToNameMap;
            this.mappings = mappings;
            this.logger = logger;
        }

        public IEndpointHandler Find(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            foreach (var key in pathToNameMap.Keys)
            {
                var path = key.EnsureLeadingSlash();
                if (context.Request.Path.StartsWithSegments(path))
                {
                    var endpointName = pathToNameMap[key];
                    logger.LogDebug("Request path {path} matched to endpoint type {endpoint}", context.Request.Path,
                        endpointName);

                    return GetEndpoint(endpointName, context);
                }
            }

            logger.LogTrace("No endpoint entry found for request path: {path}", context.Request.Path);

            return null;
        }

        private IEndpointHandler GetEndpoint(WsFederationEndpointName endpointName, HttpContext context)
        {
            var mapping = mappings.LastOrDefault(x => x.Endpoint == endpointName);
            if (mapping != null)
            {
                logger.LogDebug("Mapping found for endpoint: {endpoint}, creating handler: {endpointHandler}",
                    endpointName, mapping.Handler.FullName);
                return context.RequestServices.GetService(mapping.Handler) as IEndpointHandler;
            }

            logger.LogError("No mapping found for endpoint: {endpoint}", endpointName);

            return null;
        }
    }
}