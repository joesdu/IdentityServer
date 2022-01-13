﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer.Services
{
    internal class RefreshTokenService : IRefreshTokenService
    {
        private readonly ISystemClock _clock;
        private readonly IIdGenerator _idGenerator;
        private readonly IRefreshTokenStore _refreshTokenStore;

        public RefreshTokenService(
            ISystemClock clock,
            IIdGenerator idGenerator,
            IRefreshTokenStore refreshTokenStore)
        {
            _clock = clock;
            _idGenerator = idGenerator;
            _refreshTokenStore = refreshTokenStore;
        }

        public async Task<string> CreateAsync(IToken token, int lifetime)
        {
            var id = _idGenerator.GeneratorId();
            var refreshToken = new RefreshToken(id, token, lifetime, _clock.UtcNow.UtcDateTime);
            await _refreshTokenStore.SaveAsync(refreshToken);
            return Base64UrlEncoder.Encode(id);
        }
    }
}
