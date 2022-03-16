﻿namespace IdentityServer.Validation
{
    internal class ResourceOwnerCredentialRequestValidator : IResourceOwnerCredentialRequestValidator
    {
        public Task ValidateAsync(ResourceOwnerCredentialRequestValidation context)
        {
            throw new ValidationException(OpenIdConnectErrors.InvalidGrant, "Invalid username or password");
        }
    }
}
