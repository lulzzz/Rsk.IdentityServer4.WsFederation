using System;

namespace IdentityServer4.WsFederation.Hosting
{
    internal class WsFederationEndpointMapping
    {
        public WsFederationEndpointName Endpoint { get; set; }
        public Type Handler { get; set; }
    }
}