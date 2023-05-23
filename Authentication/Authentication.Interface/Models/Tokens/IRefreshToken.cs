namespace Authentication.Interface.Models.Tokens
{
    
    public interface IRefreshToken
    {

        /// <summary>
        /// The Id of the refresh token. This does not change between refreshes.
        /// </summary>
        public string TokenId { get; }

        /// <summary>
        /// A secret value of the JWT. This changes each time we refresh. <br/>
        /// Used to authenticate against the database that this is in fact the lastest version of the refresh token. <br/>
        /// An added layer of security.
        /// </summary>
        public string? Secret { get; }

        /// <summary>
        /// The time at which the refresh token will expire and no longer be usable.
        /// </summary>
        public DateTime ValidUntil { get; }

    }

}
