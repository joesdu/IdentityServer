﻿namespace IdentityServer.Validation
{
    public interface IResourceOwnerCredentialRequestValidator
    {
        Task ValidateAsync(ResourceOwnerCredentialRequestValidation context);
    }
}
