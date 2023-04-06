﻿using IdentityServer.Models;

namespace IdentityServer.EntityFramework.Entities
{
    public class ClientEntity: Entity
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = null!;
        public string? ClientName { get; set; }
        public string? Description { get; set; }
        public string? ClientUri { get; set; }
        public bool Enabled { get; set; } = true;
        public int AuthorizeCodeLifetime { get; set; } = 180;
        public int AccessTokenLifetime { get; set; } = 3600;
        public int RefreshTokenLifetime { get; set; } = 3600 * 24 * 30;
        public int IdentityTokenLifetime { get; set; } = 300;
        public bool RequireClientSecret { get; set; } = true;
        public bool OfflineAccess { get; set; } = false;
        public AccessTokenType AccessTokenType { get; set; } = AccessTokenType.Jwt;
        public ICollection<SecretEntity> ClientSecrets { get; set; } = Array.Empty<SecretEntity>();
        public ICollection<StringEntity> AllowedScopes { get; set; } = Array.Empty<StringEntity>();
        public ICollection<StringEntity> AllowedGrantTypes { get; set; } = Array.Empty<StringEntity>();
        public ICollection<StringEntity> AllowedRedirectUris { get; set; } = Array.Empty<StringEntity>();
        public ICollection<StringEntity> AllowedSigningAlgorithms { get; set; } = Array.Empty<StringEntity>();
    }
}