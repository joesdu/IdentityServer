﻿using IdentityServer.Models;

namespace IdentityServer.Application
{
    public class ExtensionGrantRequest
    {
        public IClient Client { get; }
        
        public ExtensionGrantRequest(IClient client)
        {
            Client = client;
        }
    }
}
