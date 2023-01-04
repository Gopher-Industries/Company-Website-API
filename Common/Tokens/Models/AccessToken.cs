namespace Tokens.Models
{
    public record AccessToken : IAccessToken
    {

        public string UserId { get; init; }

        public string Username { get; init; }

        public string Role { get; init; }

        public string SignedJWT { get; init; }

    }
}
