namespace ProjectX.WebAPI.Models.RestRequests.Request
{
    public record RefreshAccessTokenRequest
    {

        /// <summary>
        /// The refresh token used to generate a new access token
        /// </summary>
        public string RefreshToken { get; init; }

    }
}
