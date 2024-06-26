﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer.Services
{
    internal class ClaimService : IClaimService
    {
        private readonly IServerUrl _serverUrl;
        private readonly ISystemClock _systemClock;
        private readonly IdentityServerOptions _options;
        private readonly IRandomGenerator _randomGenerator;
        private readonly IProfileService _profileService;

        public ClaimService(
            IServerUrl serverUrl,
            ISystemClock systemClock,
            IdentityServerOptions options,
            IProfileService profileService,
            IRandomGenerator randomGenerator)
        {
            _options = options;
            _randomGenerator = randomGenerator;
            _serverUrl = serverUrl;
            _systemClock = systemClock;
            _profileService = profileService;
        }

        public async Task<ClaimsPrincipal> GetProfileClaimsAsync(ProfileClaimsRequest request)
        {
            var claims = new List<Claim>();
            var claimTypes = request.Resources.AllowedClaimTypes;
            var profileClaims = await _profileService.GetUserInfoClaimsAsync(new ProfileClaimsRequest(request.Subject, request.Client, request.Resources));
            claims.AddRange(profileClaims.Where(a => claimTypes.Contains(a.Type)));
            return new ClaimsPrincipal(new ClaimsIdentity(claims));
        }

        public async Task<ClaimsPrincipal> GetAccessTokenClaimsAsync(TokenGeneratorRequest request)
        {
            #region Jwt Claims
            //request jwt
            var jwtId = await _randomGenerator.GenerateAsync();
            var issuer = _serverUrl.GetServerIssuer();
            var issuedAt = _systemClock.UtcNow.ToUnixTimeSeconds();
            var expiration = issuedAt + request.Client.AccessTokenLifetime;
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.JwtId, jwtId),
                new Claim(JwtClaimTypes.Issuer, issuer),
                new Claim(JwtClaimTypes.ClientId, request.Client.ClientId),
                new Claim(JwtClaimTypes.IssuedAt, issuedAt.ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtClaimTypes.NotBefore, issuedAt.ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtClaimTypes.Expiration, expiration.ToString(), ClaimValueTypes.Integer64)
            };

            //audience
            foreach (var item in request.Resources.ApiResources)
            {
                claims.Add(new Claim(JwtClaimTypes.Audience, item.Name));
            }
            //scope
            if (_options.EmitScopesAsCommaDelimitedStringInJwt)
            {
                var scope = request.Resources.Scopes.Aggregate((x, y) => $"{x},{y}");
                claims.Add(new Claim(JwtClaimTypes.Scope, scope));
            }
            else
            {
                var scopes = request.Resources.Scopes
                    .Select(scope => new Claim(JwtClaimTypes.Scope, scope));
                claims.AddRange(scopes);
            }
            if (request.Client.OfflineAccess)
            {
                claims.Add(new Claim(JwtClaimTypes.Scope, StandardScopes.OfflineAccess));
            }
            #endregion

            #region Standard Claims
            if (request.Subject.Claims.Any(a => a.Type == JwtClaimTypes.Subject))
            {
                claims.AddRange(GetStandardSubjectClaims(request.GrantType, request.Subject.Claims, request.Resources.AllowedClaimTypes));
            }
            #endregion

            #region Subject Claims
            claims.AddRange(GetFilteredRequestClaims(request.Subject.Claims, request.Resources.AllowedClaimTypes));
            #endregion

            #region Profile Cliams
            var profileDataClaims = await _profileService.GetAccessTokenClaimsAsync(new ProfileClaimsRequest(request.Subject, request.Client, request.Resources));
            claims.AddRange(GetFilteredRequestClaims(profileDataClaims, request.Resources.AllowedClaimTypes));
            #endregion

            return new ClaimsPrincipal(new ClaimsIdentity(claims, request.GrantType));
        }

        public async Task<ClaimsPrincipal> GetIdentityTokenClaimsAsync(TokenGeneratorRequest request)
        {
            #region Jwt Claims
            //request jwt
            var issuer = _serverUrl.GetServerIssuer();
            var issuedAt = _systemClock.UtcNow.ToUnixTimeSeconds();
            var expiration = issuedAt + request.Client.AccessTokenLifetime;
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Issuer, issuer),
                new Claim(JwtClaimTypes.Nonce, request!.Code!.None!),
                new Claim(JwtClaimTypes.Audience, request.Client.ClientId),
                new Claim(JwtClaimTypes.ClientId, request.Client.ClientId),
                new Claim(JwtClaimTypes.IssuedAt, issuedAt.ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtClaimTypes.Expiration, expiration.ToString(), ClaimValueTypes.Integer64)
            };

            #endregion

            #region Standard Claims
            if (request.Subject.Claims.Any(a => a.Type == JwtClaimTypes.Subject))
            {
                claims.AddRange(GetStandardSubjectClaims(request.GrantType, request.Subject.Claims, request.Resources.AllowedClaimTypes));
            }
            #endregion

            #region Subject Claims
            claims.AddRange(GetFilteredRequestClaims(request.Subject.Claims, request.Resources.AllowedClaimTypes));
            #endregion

            #region Profile Cliams
            var profileDataClaims = await _profileService.GetIdentityTokenClaimsAsync(new ProfileClaimsRequest(request.Subject, request.Client, request.Resources));
            claims.AddRange(GetFilteredRequestClaims(profileDataClaims, request.Resources.AllowedClaimTypes));
            #endregion

            return new ClaimsPrincipal(new ClaimsIdentity(claims, request.GrantType));
        }

        private static IEnumerable<Claim> GetFilteredRequestClaims(IEnumerable<Claim> claims, IEnumerable<string> claimTypes)
        {
            return claims.Where(a => claimTypes.Contains(a.Type))
                .Where(a => !ClaimTypeFilters.ClaimsServiceFilterClaimTypes.Contains(a.Type));
        }

        private IEnumerable<Claim> GetStandardSubjectClaims(string grantType, IEnumerable<Claim> claims, IEnumerable<string> allowedClaimTypes)
        {
            if (allowedClaimTypes.Contains(JwtClaimTypes.Subject) && claims.Any(a => a.Type == JwtClaimTypes.Subject))
            {
                yield return claims.Where(a => a.Type == JwtClaimTypes.Subject).First();
                yield return claims.Where(a => a.Type == JwtClaimTypes.AuthenticationMethod).FirstOrDefault()
                    ?? new Claim(JwtClaimTypes.AuthenticationMethod, grantType);

                yield return claims.Where(a => a.Type == JwtClaimTypes.IdentityProvider).FirstOrDefault()
                    ?? new Claim(JwtClaimTypes.IdentityProvider, _options.IdentityProvider);

                yield return claims.Where(a => a.Type == JwtClaimTypes.AuthenticationTime).FirstOrDefault()
                    ?? new Claim(JwtClaimTypes.AuthenticationTime, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64);
            }
        }
    }
}
