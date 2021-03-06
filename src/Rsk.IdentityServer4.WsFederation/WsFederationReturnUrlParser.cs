﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.IdentityModel.Services;
using System.Net;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.WsFederation.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.WsFederation
{
    public class WsFederationReturnUrlParser : IReturnUrlParser
    {
        private readonly IHttpContextAccessor contextAccessor;
        private readonly ILogger<WsFederationReturnUrlParser> logger;
        private readonly SignInValidator signinValidator;

        public WsFederationReturnUrlParser(
            IHttpContextAccessor contextAccessor,
            SignInValidator signinValidator,
            ILogger<WsFederationReturnUrlParser> logger)
        {
            this.contextAccessor = contextAccessor;
            this.signinValidator = signinValidator;
            this.logger = logger;
        }

        public bool IsValidReturnUrl(string returnUrl)
        {
            if (returnUrl.IsLocalUrl())
            {
                var message = GetSignInRequestMessage(returnUrl);
                if (message != null) return true;

                logger.LogTrace("not a valid WS-Federation return URL");
                return false;                
            }

            return false;
        }

        public async Task<AuthorizationRequest> ParseAsync(string returnUrl)
        {
            var user = contextAccessor.HttpContext.User;

            var signInMessage = GetSignInRequestMessage(returnUrl);
            if (signInMessage == null) return null;

            // call validator
            var result = await signinValidator.ValidateAsync(signInMessage, user);
            if (result.IsError) return null;

            // populate request
            var request = new AuthorizationRequest()
            {
                ClientId = result.Client.ClientId,
                IdP = result.SignInRequestMessage.HomeRealm,
                RedirectUri = result.SignInRequestMessage.Reply
            };

            foreach (var item in result.SignInRequestMessage.Parameters)
            {
                request.Parameters.Add(item.Key, item.Value);
            }

            return request;
        }

        private SignInRequestMessage GetSignInRequestMessage(string returnUrl)
        {
            var decoded = WebUtility.UrlDecode(returnUrl);
            var url = "https://dummy.com" + decoded;

            WSFederationMessage message;
            if (WSFederationMessage.TryCreateFromUri(new Uri(url), out message))
            {
                return message as SignInRequestMessage;
            }

            return null;
        }
    }
}