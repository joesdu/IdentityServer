﻿namespace IdentityServer.Application
{
    public interface ISecretValidator
    {
        Task<ValidationResult> ValidateAsync(SecretValidationRequest request);
    }
}
