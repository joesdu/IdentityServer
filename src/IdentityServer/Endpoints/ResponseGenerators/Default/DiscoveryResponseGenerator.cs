﻿using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace IdentityServer.Endpoints
{
    internal class DiscoveryResponseGenerator
        : IDiscoveryResponseGenerator
    {
        private readonly IServerUrl _serverUrl;
        private readonly IResourceStore _resources;
        private readonly ISecretListParser _secretParsers;
        private readonly ISigningCredentialsService _credentials;
        private readonly IExtensionGrantListValidator _extensionGrantValidators;

        public DiscoveryResponseGenerator(
            IServerUrl serverUrl,
            IResourceStore resources,
            ISecretListParser secretParsers,
            ISigningCredentialsService credentials,
            IExtensionGrantListValidator extensionGrantValidators)
        {
            _serverUrl = serverUrl;
            _resources = resources;
            _credentials = credentials;
            _secretParsers = secretParsers;
            _extensionGrantValidators = extensionGrantValidators;
        }

        public async Task<DiscoveryGeneratorResponse> GetDiscoveryDocumentAsync()
        {
            var configuration = new OpenIdConnectConfiguration
            {
                Issuer = _serverUrl.GetServerIssuer(),
                JwksUri = _serverUrl.GetEndpointUri(IdentityServerEndpointNames.DiscoveryJwks),
                TokenEndpoint = _serverUrl.GetEndpointUri(IdentityServerEndpointNames.Token),
                UserInfoEndpoint = _serverUrl.GetEndpointUri(IdentityServerEndpointNames.UserInfo),
                EndSessionEndpoint = _serverUrl.GetEndpointUri(IdentityServerEndpointNames.EndSession),
                AuthorizationEndpoint = _serverUrl.GetEndpointUri(IdentityServerEndpointNames.Authorize),
                IntrospectionEndpoint = _serverUrl.GetEndpointUri(IdentityServerEndpointNames.Introspection),
            };
            configuration.AdditionalData.Add("revocation_endpoint", _serverUrl.GetEndpointUri(IdentityServerEndpointNames.Revocation));
            configuration.GrantTypesSupported.Add(GrantTypes.ClientCredentials);
            configuration.GrantTypesSupported.Add(GrantTypes.Password);
            configuration.GrantTypesSupported.Add(GrantTypes.RefreshToken);
            configuration.GrantTypesSupported.Add(GrantTypes.AuthorizationCode);
            var extensionsGrantTypes = _extensionGrantValidators.GetSupportedGrantTypes();
            foreach (var item in extensionsGrantTypes)
            {
                configuration.GrantTypesSupported.Add(item);
            }
            var scopes = await _resources.GetShowInDiscoveryDocumentScopesAsync();
            foreach (var item in scopes)
            {
                configuration.ScopesSupported.Add(item);
            }
            var supportedAuthenticationMethods = await _secretParsers.GetSupportedAuthenticationMethodsAsync();
            foreach (var item in supportedAuthenticationMethods)
            {
                configuration.TokenEndpointAuthMethodsSupported.Add(item);
            }
            var response = new DiscoveryGeneratorResponse(configuration);
            return response;
        }

        public async Task<JwkDiscoveryGeneratorResponse> CreateJwkDiscoveryDocumentAsync()
        {
            var jwks = await _credentials.GetJsonWebKeysAsync();
            return new JwkDiscoveryGeneratorResponse(jwks);
        }
    }
}
