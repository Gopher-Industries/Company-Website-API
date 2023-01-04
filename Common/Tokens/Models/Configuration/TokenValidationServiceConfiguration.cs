namespace Tokens.Models.Configuration
{
    public sealed record TokenValidationServiceConfiguration
    {

        public string AccessTokenPublicKey { get; init; }

        public string TokenIssuer { get; init; }

    }
}
