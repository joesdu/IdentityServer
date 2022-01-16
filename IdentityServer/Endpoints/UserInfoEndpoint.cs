﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using IdentityServer.Authentication;

namespace IdentityServer.Endpoints
{
    internal class UserInfoEndpoint : EndpointBase
    {

        private readonly IClientStore _clients;
        private readonly IResourceStore _resources;
        private readonly IUserInfoResponseGenerator _generator;
        private readonly IResourceValidator _resourceValidator;

        public UserInfoEndpoint(
            IClientStore clients,
            IResourceStore resources,
            ITokenValidator tokenValidator,
            IProfileService profileService,
            IResourceValidator resourceValidator,
            IBearerTokenUsageParser bearerTokenUsageParser,
            IUserInfoResponseGenerator generator)
        {
            _clients = clients;
            _resources = resources;
            _generator = generator;
            _resourceValidator = resourceValidator;
        }

        public override async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            var authenticateResult = await context.AuthenticateAsync(IdentityServerAuthenticationDefaults.AuthenticationScheme);
            if (authenticateResult == null || !authenticateResult.Succeeded)
            {
                return Unauthorized(OpenIdConnectTokenErrors.InvalidRequest, "authentication failed");
            }
            var subject = authenticateResult.Principal;
            var sub = subject.GetSubjectId();
            if (string.IsNullOrWhiteSpace(sub))
            {
                return BadRequest(OpenIdConnectTokenErrors.InvalidToken, "Sub claim is missing");
            }
            var clientId = subject.GetClientId();
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return BadRequest(OpenIdConnectTokenErrors.InvalidToken, "ClientId claim is missing");
            }
            var scopes = subject.FindAll(JwtClaimTypes.Scope).Select(s => s.Value).Where(a => !string.IsNullOrWhiteSpace(a));
            var client = await _clients.GetAsync(clientId);
            if (client == null)
            {
                return BadRequest(OpenIdConnectTokenErrors.InvalidToken, "Invalid client");
            }
            var resources = await _resources.FindByScopeAsync(scopes);
            await _resourceValidator.ValidateAsync(resources, scopes);
            var response = await _generator.ProcessAsync(new UserInfoRequest(subject, client, resources));
            return new UserInfoResult(response);
        }
    }
}
