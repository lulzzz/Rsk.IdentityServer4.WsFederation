using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.WsFederation.Endpoints.Results
{
    public class WsFederationSignoutResult : IEndpointResult
    {
        private IdentityServerOptions options;

        public WsFederationSignoutResult() { }
        internal WsFederationSignoutResult(IdentityServerOptions options)
        {
            this.options = options;
        }

        void Init(HttpContext context)
        {
            options = options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
        }

        public Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            var redirect = options.UserInteraction.LogoutUrl;
            if (redirect.IsLocalUrl())
            {
                redirect = context.GetIdentityServerRelativeUrl(redirect);
            }
            
            context.Response.SetNoCache();
            context.Response.Redirect(redirect);
            return Task.CompletedTask;
        }
    }
}