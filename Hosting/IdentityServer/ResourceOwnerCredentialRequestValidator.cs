﻿using IdentityServer.Models;
using IdentityServer.Validation;

namespace Hosting.Configuration
{
    public class ResourceOwnerCredentialRequestValidator : IResourceOwnerCredentialRequestValidator
    {
        public Task ValidateAsync(ResourceOwnerCredentialRequestValidation context)
        {
            if (context.Username == "test" && context.Password == "test")
            {
                return Task.CompletedTask;
            }
            throw new ValidationException(OpenIdConnectErrors.InvalidGrant, "用户名或密码错误");
        }
    }
}