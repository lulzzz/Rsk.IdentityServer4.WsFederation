using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;

namespace IdentityServer4.WsFederation.Hosting
{
    internal interface IWsFederationEndpointRouter
    {
        IEndpointHandler Find(HttpContext context);
    }
}