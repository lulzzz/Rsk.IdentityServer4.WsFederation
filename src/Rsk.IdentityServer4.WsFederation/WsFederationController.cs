// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.WsFederation.Validation;
using IdentityServer4.Configuration;
using IdentityServer4.Services;

namespace IdentityServer4.WsFederation
{
    public class WsFederationController : Controller
    {
        private readonly IUserSession sessionService;
        private readonly SignInResponseGenerator generator;
        private readonly ILogger<WsFederationController> logger;
        private readonly MetadataResponseGenerator metadata;
        private readonly IdentityServerOptions options;
        private readonly SignInValidator signinValidator;

        public WsFederationController(
            MetadataResponseGenerator metadata, 
            SignInValidator signinValidator, 
            IdentityServerOptions options,
            SignInResponseGenerator generator,
            IUserSession sessionService,
            ILogger<WsFederationController> logger)
        {
            this.metadata = metadata;
            this.signinValidator = signinValidator;
            this.options = options;
            this.generator = generator;
            this.sessionService = sessionService;

            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // GET + no parameters = metadata request
            if (!Request.QueryString.HasValue)
            {
                logger.LogDebug("Start WS-Federation metadata request");

                var entity = await metadata.GenerateAsync(Url.Action("Index", "WsFederation", null, Request.Scheme, Request.Host.Value));
                return new MetadataResult(entity);
            }
            
            var url = Url.Action("Index", "WsFederation", null, Request.Scheme, Request.Host.Value) + Request.QueryString;
            logger.LogDebug("Start WS-Federation request: {url}", url);

            if (WSFederationMessage.TryCreateFromUri(new Uri(url), out WSFederationMessage message))
            {
                if (message is SignInRequestMessage signin)
                {
                    return await ProcessSignInAsync(signin, User);
                }

                if (message is SignOutRequestMessage signout)
                {
                    return ProcessSignOutAsync(signout);
                }
            }

            return BadRequest("Invalid WS-Federation request");
        }

        
        private async Task<IActionResult> ProcessSignInAsync(SignInRequestMessage signin, ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                logger.LogDebug("User in WS-Federation signin request: {subjectId}", user.GetSubjectId());
            }
            else
            {
                logger.LogDebug("No user present in WS-Federation signin request");
            }

            // validate request
            var result = await signinValidator.ValidateAsync(signin, user);

            if (result.IsError)
            {
                throw new Exception(result.Error);
            }

            if (result.SignInRequired)
            {
                var returnUrl = Url.Action("Index");
                returnUrl = returnUrl.AddQueryString(Request.QueryString.Value);

                var loginUrl = options.UserInteraction.LoginUrl;
                var url = loginUrl.AddQueryString(options.UserInteraction.LoginReturnUrlParameter, returnUrl);

                return Redirect(url);
            }
            else
            {
                // create protocol response
                var responseMessage = await generator.GenerateResponseAsync(result);
                await sessionService.AddClientIdAsync(result.Client.ClientId);
                
                return new SignInResult(responseMessage);
            }
        }

        private IActionResult ProcessSignOutAsync(SignOutRequestMessage signout)
        {
            return Redirect("~/connect/endsession");
        }
    }
}