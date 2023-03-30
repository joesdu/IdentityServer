﻿using IdentityServer.Models;

namespace IdentityServer.Storage
{
    public interface ITokenStore
    {
        Task SaveTokenAsync(Token token);
        Task SetLifetimeAsync(Token token);
        Task RevomeTokenAsync(Token token);
        Task<Token?> FindAccessTokenAsync(string token);
        Task<Token?> FindRefreshTokenAsync(string token);
    }
}
