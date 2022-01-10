﻿using static IdentityServer.Protocols.OpenIdConnectConstants;

namespace IdentityServer.Configuration
{
    public class IdentityServerOptions
    {
        public ICollection<string> TokenEndpointAuthMethods { get; set; } = new HashSet<string>()
        { 
            TokenEndpointAuthenticationMethods.PostBody 
        };
        public InputLengthRestrictions InputLengthRestrictions { get; set; } = new InputLengthRestrictions();
        public DiscoveryOptions Discovery { get; set; } = new DiscoveryOptions();
        public EndpointsOptions Endpoints { get; set; } = new EndpointsOptions();
        public string? IssuerUri { get; set; }
        public bool LowerCaseIssuerUri { get; set; } = true;
        public string? AccessTokenJwtType { get; set; } = "at+jwt";
        public bool EmitScopesAsSpaceDelimitedStringInJwt { get; set; } = false;
    }
}
