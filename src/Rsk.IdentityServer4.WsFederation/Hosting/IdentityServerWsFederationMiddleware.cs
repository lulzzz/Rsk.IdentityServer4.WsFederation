using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.WsFederation.Hosting
{
    internal class IdentityServerWsFederationMiddleware
    {

        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public IdentityServerWsFederationMiddleware(RequestDelegate next, ILogger<IdentityServerWsFederationMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context, IWsFederationEndpointRouter router)
        {
            try
            {
                var endpoint = router.Find(context);
                if (endpoint != null)
                {
                    logger.LogInformation("Invoking IdentityServer endpoint: {endpointType} for {url}", endpoint.GetType().FullName, context.Request.Path.ToString());
                    
                    var result = await endpoint.ProcessAsync(context);

                    if (result != null)
                    {
                        logger.LogTrace("Invoking result: {type}", result.GetType().FullName);
                        await result.ExecuteAsync(context);
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical("Unhandled exception: {exception}", ex.ToString());
                throw;
            }

            await next(context);
        }
    }

}