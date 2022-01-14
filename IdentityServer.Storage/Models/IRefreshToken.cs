﻿namespace IdentityServer.Models
{
    public interface IRefreshToken
    {
        string Id { get; }
        IToken AccessToken { get; }
        int Lifetime { get; }
        DateTime Expiration { get; }
        DateTime CreationTime { get; }
    }
}
