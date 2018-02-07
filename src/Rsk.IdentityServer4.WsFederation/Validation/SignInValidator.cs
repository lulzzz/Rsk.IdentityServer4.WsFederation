// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Stores;
using System.IdentityModel.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.WsFederation.Stores;
using Rsk.IdentityServer4.WsFederation.Configuration;

namespace IdentityServer4.WsFederation.Validation
{
    public class SignInValidator
    {
        private readonly IClientStore clients;
        private readonly IRelyingPartyStore relyingParties;
        private readonly WsFederationOptions options;

        public SignInValidator(WsFederationOptions options, IClientStore clients, IRelyingPartyStore relyingParties)
        {
            this.options = options;
            this.clients = clients;
            this.relyingParties = relyingParties;
        }

        public async Task<SignInValidationResult> ValidateAsync(SignInRequestMessage message, ClaimsPrincipal user)
        {
            //Logger.Info("Start WS-Federation signin request validation");
            var result = new SignInValidationResult
            {
                SignInRequestMessage = message
            };
            
            // check client
            var client = await clients.FindEnabledClientByIdAsync(message.Realm);

            if (client == null)
            {
                LogError("Client not found: " + message.Realm, result);

                return new SignInValidationResult
                {
                    Error = "invalid_relying_party"
                };
            }
            if (client.ProtocolType != IdentityServerConstants.ProtocolTypes.WsFederation)
            {
                LogError("Client is not configured for WS-Federation", result);

                return new SignInValidationResult
                {
                    Error = "invalid_relying_party"
                };
            }

            result.Client = client;
            result.ReplyUrl = client.RedirectUris.First();

            // check if additional relying party settings exist
            var rp = await relyingParties.FindRelyingPartyByRealm(message.Realm);
            if (rp == null)
            {
                rp = new RelyingParty
                {
                    TokenType = options.DefaultTokenType,
                    SignatureAlgorithm = options.DefaultSignatureAlgorithm,
                    DigestAlgorithm = options.DefaultDigestAlgorithm,
                    SamlNameIdentifierFormat = options.DefaultSamlNameIdentifierFormat,
                    ClaimMapping = options.DefaultClaimMapping
                };
            }

            result.RelyingParty = rp;

            // assign defaults for unconfigured properties
            if (result.RelyingParty.TokenType == null) result.RelyingParty.TokenType = options.DefaultTokenType;
            if (result.RelyingParty.SignatureAlgorithm == null) result.RelyingParty.SignatureAlgorithm = options.DefaultSignatureAlgorithm;
            if (result.RelyingParty.DigestAlgorithm == null) result.RelyingParty.DigestAlgorithm = options.DefaultDigestAlgorithm;
            if (result.RelyingParty.SamlNameIdentifierFormat == null) result.RelyingParty.SamlNameIdentifierFormat = options.DefaultSamlNameIdentifierFormat;
            if (result.RelyingParty.ClaimMapping?.Any() != true) result.RelyingParty.ClaimMapping = options.DefaultClaimMapping;

            if (user == null ||
                user.Identity.IsAuthenticated == false)
            {
                result.SignInRequired = true;
            }

            result.User = user;
            
            LogSuccess(result);
            return result;
        }

        private void LogSuccess(SignInValidationResult result)
        {
            //var log = new SignInValidationLog(result);
            //Logger.InfoFormat("End WS-Federation signin request validation\n{0}", log.ToString());
        }

        private void LogError(string message, SignInValidationResult result)
        {
            //var log = new SignInValidationLog(result);
            //Logger.ErrorFormat("{0}\n{1}", message, log.ToString());
        }
    }
}