using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Services;
using System.Net;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.WsFederation.Validation;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Rsk.IdentityServer4.WsFederation.Endpoints.Results;

namespace IdentityServer4.WsFederation
{
    public class WsFederationEndpoint : IEndpointHandler
    {
        private readonly MetadataResponseGenerator metadata;
        private readonly SignInValidator signinValidator;
        private readonly SignInResponseGenerator generator;
        private readonly IUserSession sessionService;
        private readonly ILogger<WsFederationEndpoint> logger;

        public WsFederationEndpoint(
            MetadataResponseGenerator metadata, 
            SignInValidator signinValidator,
            SignInResponseGenerator generator,
            IUserSession sessionService,
            ILogger<WsFederationEndpoint> logger)
        {
            this.metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            this.signinValidator = signinValidator ?? throw new ArgumentNullException(nameof(signinValidator));
            this.generator = generator ?? throw new ArgumentNullException(nameof(generator));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            if (context.Request.Method != HttpMethods.Get) return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);

            // no parameters = metadata request
            if (!context.Request.QueryString.HasValue)
            {
                logger.LogDebug("Start WS-Federation metadata request");

                var entity = await metadata.GenerateAsync(context.Request.GetEncodedUrl());
                return new MetadataResult(entity);
            }

            var url = context.Request.GetEncodedUrl();
            logger.LogDebug("Start WS-Federation request: {url}", url);

            if (WSFederationMessage.TryCreateFromUri(new Uri(url), out var message))
            {
                if (message is SignInRequestMessage signin)
                {
                    return await ProcessSignInAsync(context, signin);
                }

                if (message is SignOutRequestMessage signout)
                {
                    return ProcessSignOutAsync(signout);
                }
            }

            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }
        
        private async Task<IEndpointResult> ProcessSignInAsync(HttpContext context, SignInRequestMessage signin)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                logger.LogDebug("User in WS-Federation signin request: {subjectId}", context.User.GetSubjectId());
            }
            else
            {
                logger.LogDebug("No user present in WS-Federation signin request");
            }

            // validate request
            var result = await signinValidator.ValidateAsync(signin, context.User);

            if (result.IsError)
            {
                throw new Exception(result.Error);
            }

            if (result.SignInRequired)
            {
                var wsFedRequest = context.Request.QueryString.Value;
                return new WsFederationLoginPageResult(wsFedRequest);
            }

            // create protocol response
            var responseMessage = await generator.GenerateResponseAsync(result);
            await sessionService.AddClientIdAsync(result.Client.ClientId);

            return new SignInResult(responseMessage);
        }

        private IEndpointResult ProcessSignOutAsync(SignOutRequestMessage signout)
        {
            // TODO: Handle signout message validation & user session?)
            return new WsFederationSignoutResult();
        }
    }
}