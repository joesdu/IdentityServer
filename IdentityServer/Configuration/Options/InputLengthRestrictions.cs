﻿namespace IdentityServer.Configuration
{
    public class InputLengthRestrictions
    {
        private const int Default = 100;
        
        /// <summary>
        /// Max length for client_id
        /// </summary>
        public int ClientId { get; set; } = Default;

        /// <summary>
        /// Max length for external client secrets
        /// </summary>
        public int ClientSecret { get; set; } = Default;
        /// <summary>
        /// Max length for grant_type
        /// </summary>
        public int GrantType { get; set; } = Default;

        /// <summary>
        /// Max length for username
        /// </summary>
        public int UserName { get; set; } = Default;

        /// <summary>
        /// Max length for password
        /// </summary>
        public int Password { get; set; } = Default;
    }
}
