﻿using Microsoft.AspNetCore.Http;

namespace IdentityServer.Validation
{
    internal class ApiSecretValidator : IApiSecretValidator
    {
        private readonly IResourceStore _resources;
        private readonly SecretListParser _secretParsers;
        private readonly SecretListValidator  _secretValidators;

        public ApiSecretValidator(
            IResourceStore resources,
            SecretListParser secretParsers,
            SecretListValidator  secretValidators)
        {
            _resources = resources;
            _secretParsers = secretParsers;
            _secretValidators = secretValidators;
        }
        
        public async Task<ApiResource> ValidateAsync(HttpContext context)
        {
            var parsedSecret = await _secretParsers.ParseAsync(context);
            if (parsedSecret.Type == ClientSecretTypes.NoSecret)
            {
                throw new ValidationException(OpenIdConnectErrors.InvalidRequest, "Client credentials is missing");
            }
            var apiResources = await _resources.FindApiResourcesByNameAsync(parsedSecret.ClientId);
            if (!apiResources.Any())
            {
                throw new ValidationException(OpenIdConnectErrors.InvalidClient, "No API resource with that name found. aborting");
            }
            if (apiResources.Count() > 1)
            {
                throw new ValidationException(OpenIdConnectErrors.InvalidClient, "More than one API resource with that name found. aborting");
            }
            var apiResource = apiResources.First();
            await _secretValidators.ValidateAsync(parsedSecret, apiResource.ApiSecrets);
            return apiResource;
        }
    }
}