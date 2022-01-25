﻿using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace IdentityServer.Endpoints
{
    internal class DiscoveryGenerator
        : IDiscoveryGenerator
    {
        private readonly IResourceStore _resources;
        private readonly SecretParserCollection _secretParsers;
        private readonly ExtensionGrantValidatorCollection _extensionGrantValidator;
        private readonly ISigningCredentialsStore _credentials;

        public DiscoveryGenerator(
            IResourceStore resources,
            SecretParserCollection secretParsers,
            ISigningCredentialsStore credentials,
            ExtensionGrantValidatorCollection extensionGrantValidator)
        {
            _resources = resources;
            _credentials = credentials;
            _secretParsers = secretParsers;
            _extensionGrantValidator = extensionGrantValidator;
        }

        public async Task<DiscoveryResponse> CreateDiscoveryDocumentAsync(string issuer)
        {
            var configuration = new OpenIdConnectConfiguration();
            configuration.Issuer = issuer;
            configuration.JwksUri = issuer + Constants.EndpointRoutePaths.DiscoveryJwks;
            configuration.AuthorizationEndpoint = issuer + Constants.EndpointRoutePaths.Authorize;
            configuration.TokenEndpoint = issuer + Constants.EndpointRoutePaths.Token;
            configuration.UserInfoEndpoint = issuer + Constants.EndpointRoutePaths.UserInfo;
            var grantTypes = _extensionGrantValidator.GetExtensionGrantTypes();
            foreach (var item in grantTypes)
            {
                configuration.GrantTypesSupported.Add(item);
            }
            configuration.GrantTypesSupported.Add(GrantTypes.ClientCredentials);
            configuration.GrantTypesSupported.Add(GrantTypes.Password);
            configuration.GrantTypesSupported.Add(GrantTypes.RefreshToken);
            var scopes = await _resources.GetShowInDiscoveryDocumentScopesAsync();
            foreach (var item in scopes)
            {
                configuration.ScopesSupported.Add(item);
            }
            var authMethods = _secretParsers.GetAuthenticationMethods(); 
            foreach (var item in authMethods)
            {
                configuration.TokenEndpointAuthMethodsSupported.Add(item);
            }
            var response = new DiscoveryResponse(configuration);
            return response;
        }

        public async Task<JwkDiscoveryResponse> CreateJwkDiscoveryDocumentAsync()
        {
            var jwks = await _credentials.GetJsonWebKeysAsync();
            return new JwkDiscoveryResponse(jwks);
        }
    }
}
