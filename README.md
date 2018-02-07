# Rsk.IdentityServer4.WsFederation
Repackage of IdentityServer4.WsFederation using ASP.NET Core dependency inject and middleware

## .NET Support
`Rsk.IdentityServer.WsFederation` currently only supports ASP.NET Core running on the full .NET Framework.

## Configuration
To enable WS-Federation support in IdentityServer 4, add the following to your `Startup.cs`

#### `ConfigureServices` method (after `AddMvc`)
    services.AddIdentityServer()
        // existing registrations
        .AddWsFederation()
        .AddInMemoryRelyingParties(new List<RelyingParty>());

#### `Configure` method (before `UseMvc`)
    app.UseIdentityServer()
       .UseIdentityServerWsFederationPlugin();

### Options
You can configure global settings for the WS-Federation component using `WsFederationOptions`
- `WsFederationEndpoint`: the path to host the WS-Federation component
- `DefaultTokenType`: defaults to `urn:oasis:names:tc:SAML:2.0:assertion`
- `DefaultDigestAlgorithm`: defaults to `http://www.w3.org/2001/04/xmlenc#sha256`
- `DefaultSignatureAlgorithm`: defaults to `http://www.w3.org/2001/04/xmldsig-more#rsa-sha256`
- `DefaultSamlNameIdentifierFormat`: default to `urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified`
- `DefaultClaimMapping`: defaults cover most common profile claim types

### Relying Party
To configure a relying party, you must have a `Client` configured with a `ProtocolType` of `wsfed`, and, optionally, a `RelyingParty`.
Setting a `RelyingParty` allows you to overwrite the global defaults for that individual relying party.
The `Client` and `RelyingParty` must share the same value for `ClientId` and `Realm` respectively.

    var wsFedClient = new Client {
        ClientId = "urn:wsfedrp",
        ClientName = "WS-Federation Relying Party",
        ProtocolType = IdentityServerConstants.ProtocolTypes.WsFederation,
        RedirectUris = {"http://localhost:5001"},
        IdentityTokenLifetime = 36000, // saml token lifetime
        AllowedScopes = {"openid", "profile", "email"}
    };

    var wsFedRelyingParty = new RelyingParty {
        Realm = "urn:wsfedrp",
        TokenType = WsFederationConstants.TokenTypes.Saml2TokenProfile11
    };
