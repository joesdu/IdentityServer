﻿using IdentityServer.Configuration;
using IdentityServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using static IdentityServer.Protocols.OpenIdConnectConstants;

namespace IdentityServer.Services
{
    public class PostBodyClientSecretScheme : ITokenEndpointAuthMethod
    {
        private readonly ILogger _logger;
       
        private readonly IdentityServerOptions _options;
      
        public PostBodyClientSecretScheme(
            IdentityServerOptions options,
            ILogger<PostBodyClientSecretScheme> logger)
        {
            _logger = logger;
            _options = options;
        }

        public string AuthMethod => TokenEndpointAuthenticationMethods.PostBody;

        public async Task<ParsedClientSecret?> ParseAsync(HttpContext context)
        {
            var form = await context.Request.ReadFormAsync();
            var id = form["client_id"].FirstOrDefault();
            var secret = form["client_secret"].FirstOrDefault();
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogError("Client ID exceeds maximum length.");
                return null;
            }
            if (id.Length > _options.InputLengthRestrictions.ClientId)
            {
                return null;
            }
            if (string.IsNullOrEmpty(secret))
            {
                return null;
            }
            if (secret.Length > _options.InputLengthRestrictions.ClientSecret)
            {
                _logger.LogError("Client secret exceeds maximum length.");
                return null;
            }
            return new ParsedClientSecret(id, ParsedSecretTypes.SharedSecret)
            {
                Credential = secret,
            };
        }
    }
}
