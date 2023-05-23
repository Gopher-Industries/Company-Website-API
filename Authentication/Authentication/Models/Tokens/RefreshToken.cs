using Authentication.Interface.Models.Tokens;
using Google.Cloud.Firestore;

namespace Authentication.Models.Tokens
{

    [FirestoreData]
    public record RefreshTokenDatabaseEntry : IRefreshToken
    {

        /// <summary>
        /// The Id of the refresh token. This does not change between refreshes
        /// </summary>
        [FirestoreDocumentId]
        public string TokenId { get; init; }

        /// <summary>
        /// A secret value of the JWT. This changes each time we refresh. <br/>
        /// Used to authenticate against the database that this is in fact the lastest version of the refresh token. <br/>
        /// An added layer of security.
        /// </summary>
        [FirestoreProperty]
        public string? Secret { get; init; }

        /// <summary>
        /// The time at which the refresh token will expire and no longer be usable
        /// </summary>
        [FirestoreProperty]
        public DateTime ValidUntil { get; init; }

    }


    public record RefreshTokenStateful : RefreshTokenDatabaseEntry
    {

        /// <summary>
        /// The User Id we're authenticating.
        /// </summary>
        public string UserId { get; init; }

        /// <summary>
        /// The username of the user we're authenticating.
        /// </summary>
        public string Username { get; init; }

        /// <summary>
        /// The users role.
        /// </summary>
        public string Role { get; init; }

        /// <summary>
        /// The JWT refresh token itself.
        /// </summary>
        public string SignedJWT { get; init; }

    }
}
