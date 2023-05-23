namespace Authentication.Models.Settings
{

    /// <summary>
    /// Holds information relating to the encryption keys used in the application
    /// </summary>
    public record ApplicationSecurityKeys
    {

        /// <summary>
        /// The private key we use to encrypt access JWT tokens
        /// </summary>
        public string AccessTokenPrivateKey { get; init; }

        /// <summary>
        /// The private key we use to encrypt refresh JWT tokens
        /// </summary>
        public string RefreshTokenPrivateKey { get; init; }

    }
}
