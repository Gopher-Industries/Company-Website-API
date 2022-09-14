namespace ProjectX.WebAPI.Models.RestRequests.Request
{
    public record LoginRequest
    {

        /// <summary>
        /// The user's username
        /// </summary>
        /// <example>Nat</example>
        public string Username { get; init; }

        /// <summary>
        /// The user's plain text password
        /// </summary>
        /// <example>TestingPassword123</example>
        public string Password { get; init; }

    }
}
