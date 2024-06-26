﻿using System.Collections.Specialized;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace IdentityServer.Endpoints
{
    internal class AuthorizeEndpoint : EndpointBase
    {
        private readonly IClientStore _clientStore;
        private readonly ILoggerFormater _loggerFormater;
        private readonly IdentityServerOptions _options;
        private readonly IResourceValidator _resourceValidator;
        private readonly IAuthorizeResponseGenerator _generator;

        public AuthorizeEndpoint(
            IClientStore clientStore,
            IdentityServerOptions options,
            ILoggerFormater loggerFormater,
            IResourceValidator resourceValidator,
            IAuthorizeResponseGenerator generator)
        {
            _options = options;
            _clientStore = clientStore;
            _loggerFormater = loggerFormater;
            _resourceValidator = resourceValidator;
            _generator = generator;
        }

        public override async Task<IEndpointResult> HandleAsync(HttpContext context)
        {
            #region Logger Request
            await _loggerFormater.LogRequestAsync();
            #endregion

            #region Challenge
            var result = await context.AuthenticateAsync(_options.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return Challenge();
            }
            #endregion

            #region Validate Request
            NameValueCollection parameters;
            if (HttpMethods.IsGet(context.Request.Method))
            {
                parameters = context.Request.Query.AsNameValueCollection();
            }
            else if (HttpMethods.IsPost(context.Request.Method))
            {
                if (!context.Request.HasFormContentType)
                {
                    return StatusCode(HttpStatusCode.UnsupportedMediaType);
                }
                parameters = context.Request.Form.AsNameValueCollection();
            }
            else
            {
                return MethodNotAllowed();
            }
            #endregion

            #region Validate Client
            var clientId = parameters[OpenIdConnectParameterNames.ClientId];
            if (string.IsNullOrEmpty(clientId))
            {
                return BadRequest(ValidationErrors.InvalidRequest, $"{OpenIdConnectParameterNames.ClientId} is missing");
            }
            var client = await _clientStore.FindClientAsync(clientId);
            if (client == null)
            {
                return BadRequest(ValidationErrors.InvalidClient, $"{OpenIdConnectParameterNames.ClientId} is missing");
            }
            #endregion

            #region GrantType
            if (!client.AllowedGrantTypes.Contains(GrantTypes.AuthorizationCode))
            {
                return BadRequest(ValidationErrors.UnauthorizedClient, $"The client does not allow {GrantTypes.AuthorizationCode}");
            }
            #endregion

            #region RedirectUri
            var redirectUri = parameters[OpenIdConnectParameterNames.RedirectUri];
            if (string.IsNullOrEmpty(redirectUri))
            {
                return BadRequest(ValidationErrors.InvalidRequest, $"{OpenIdConnectParameterNames.RedirectUri} is missing");
            }
            if (!client.AllowedRedirectUris.Any(a => a == redirectUri))
            {
                return BadRequest(ValidationErrors.InvalidGrant, "Not allowed redirectUri");
            }
            redirectUri = WebUtility.UrlDecode(redirectUri);
            #endregion

            #region Validate Resources
            var scope = parameters[OpenIdConnectParameterNames.Scope] ?? string.Empty;
            if (scope.Length > _options.InputLengthRestrictions.Scope)
            {
                return BadRequest(ValidationErrors.InvalidRequest, $"{OpenIdConnectParameterNames.Scope} is too long");
            }
            var scopes = scope.Split(" ").Where(a => !string.IsNullOrWhiteSpace(a));
            var resources = await _resourceValidator.ValidateAsync(client, scopes);
            #endregion

            #region State
            var state = parameters[OpenIdConnectParameterNames.State];
            if (string.IsNullOrEmpty(state))
            {
                return BadRequest(ValidationErrors.InvalidRequest, $"{OpenIdConnectParameterNames.State} is missing");
            }
            #endregion

            #region None
            var none = parameters[OpenIdConnectParameterNames.Nonce];
            if (string.IsNullOrEmpty(none))
            {
                return BadRequest(ValidationErrors.InvalidNone, $"{OpenIdConnectParameterNames.Nonce} is null or empty");
            }
            #endregion

            #region ResponseType
            var responseType = parameters[OpenIdConnectParameterNames.ResponseType];
            if (string.IsNullOrEmpty(responseType))
            {
                return BadRequest(ValidationErrors.InvalidRequest, $"{OpenIdConnectParameterNames.ResponseType} is null or empty");
            }
            #endregion

            #region Response
            var request = new AuthorizeGeneratorRequest(parameters, client, resources, result.Principal);
            var authorizationCode = await _generator.GenerateAsync(request);
            var redirectUrl = CreateRedirectUrl(authorizationCode);
            return Redirect(redirectUrl);
            #endregion
        }

        private string CreateRedirectUrl(AuthorizationCode authorizationCode)
        {
            var buffer = new StringBuilder();
            buffer.Append(authorizationCode.RedirectUri);
            buffer.AppendFormat("?{0}={1}", OpenIdConnectParameterNames.Code, authorizationCode.Code);
            buffer.AppendFormat("&{0}={1}", OpenIdConnectParameterNames.State, authorizationCode.State);
            return buffer.ToString();
        }
    }
}
