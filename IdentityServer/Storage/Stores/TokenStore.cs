﻿namespace IdentityServer.Storage
{
    internal class TokenStore : ITokenStore
    {
        private readonly ICacheStore _cache;
        private readonly IdentityServerOptions _options;

        public TokenStore(
            ICacheStore cache,
            IdentityServerOptions options)
        {
            _cache = cache;
            _options = options;
        }

        public async Task<Token?> FindAccessTokenAsync(string token)
        {
            var accessToken = await FindTokenAsync(token);
            if (accessToken?.Type != TokenTypes.AccessToken)
            {
                return null;
            }
            return accessToken;
        }

        public async Task<Token?> FindRefreshTokenAsync(string token)
        {
            var accessToken = await FindTokenAsync(token);
            if (accessToken?.Type != TokenTypes.RefreshToken)
            {
                return null;
            }
            return accessToken;
        }

        public async Task SaveTokenAsync(Token token)
        {
            var key = BuildKey(token.Id);
            var span = TimeSpan.FromSeconds(token.Lifetime);
            await _cache.SaveAsync(key, token, span);
        }

        public async Task RevomeTokenAsync(Token token)
        {
            var key = BuildKey(token.Id);
            await _cache.RevomeAsync(key);
        }

        private async Task<Token?> FindTokenAsync(string token)
        {
            var key = BuildKey(token);
            return await _cache.GetAsync<Token>(key);
        }

        private string BuildKey(string id)
        {
            return $"{_options.StorageKeyPrefix}:Token:{id}";
        }

      
    }
}
