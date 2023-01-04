namespace ProjectX.WebAPI.Models
{
    public record ApplicationIdentitySettings
    {

        /// <summary>
        /// The secret we use to encrypt access JWT tokens
        /// </summary>
        public string AccessJWTSecret { get; init; }

        /// <summary>
        /// The secret we use to encrypt refresh JWT tokens
        /// </summary>
        public string RefreshJWTSecret { get; init; }

    }
}
