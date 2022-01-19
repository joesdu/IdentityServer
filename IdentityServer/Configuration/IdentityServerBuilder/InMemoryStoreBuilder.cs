﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer.Configuration
{
    public class InMemoryStoreBuilder
    {
        #region fields
        private readonly List<IClient> Clients = new List<IClient>();
        private readonly List<IResource> Resources = new List<IResource>();
        private readonly List<SigningCredentialsDescriptor> SigningCredentials = new List<SigningCredentialsDescriptor>();
        #endregion

        #region SigningCredentials
        public InMemoryStoreBuilder AddSigningCredential(SigningCredentials credential, string signingAlgorithm)
        {
            SigningCredentials.Add(new SigningCredentialsDescriptor(credential, signingAlgorithm));
            return this;
        }
        public InMemoryStoreBuilder AddSigningCredential(SecurityKey securityKey, string signingAlgorithm = SecurityAlgorithms.RsaSha256)
        {
            var credential = new SigningCredentials(securityKey, signingAlgorithm);
            AddSigningCredential(credential, signingAlgorithm);
            return this;
        }
        public InMemoryStoreBuilder AddSigningCredential(X509Certificate2 certificate, string signingAlgorithm = SecurityAlgorithms.RsaSha256)
        {
            if (!certificate.HasPrivateKey)
            {
                throw new InvalidOperationException("X509 certificate does not have a private key.");
            }
            var securityKey = new X509SecurityKey(certificate);
            securityKey.KeyId += signingAlgorithm;
            var credential = new SigningCredentials(securityKey, signingAlgorithm);
            AddSigningCredential(credential, signingAlgorithm);
            return this;
        }
        public InMemoryStoreBuilder AddDeveloperSigningCredential(bool persistKey = true,string? filename = null,string signingAlgorithm = SecurityAlgorithms.RsaSha256)
        {
            if (filename == null)
            {
                filename = Path.Combine(Directory.GetCurrentDirectory(), "tempkey.jwk");
            }
            if (File.Exists(filename))
            {
                var json = File.ReadAllText(filename);
                var jwk = new JsonWebKey(json);
                AddSigningCredential(jwk, jwk.Alg);
                return this;
            }
            else
            {
                var key = CryptoRandom.CreateRsaSecurityKey();
                var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
                jwk.Alg = signingAlgorithm.ToString();
                if (persistKey)
                {
                    File.WriteAllText(filename, JsonSerializer.Serialize(jwk));
                }
                AddSigningCredential(key, signingAlgorithm);
                return this;
            }
        }
        #endregion

        #region Resource
        public InMemoryStoreBuilder AddResources(IEnumerable<IResource> scopes)
        {
            Resources.AddRange(scopes);
            return this;
        }
        #endregion

        #region Client
        public InMemoryStoreBuilder AddClients(IEnumerable<IClient> clients)
        {
            Clients.AddRange(clients);
            return this;
        }
        #endregion

        #region build
        internal void Build(IIdentityServerBuilder services)
        {
            if (Clients.Any())
            {
                services.AddClientStore(sp =>
                {
                    return new InMemoryClientStore(Clients);
                });
            }
            if (Resources.Any())
            {
                services.AddResourceStore(sp =>
                {
                    return new InMemoryResourceStore(new Resources(Resources));
                });
            }
            if (SigningCredentials.Any())
            {
                services.AddSigningCredentialStore(sp =>
                {
                    return new InMemorySigningCredentialStore(SigningCredentials);
                });
            }
        }
        #endregion
    }
}
