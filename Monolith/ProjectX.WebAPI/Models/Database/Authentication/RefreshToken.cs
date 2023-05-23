using Google.Cloud.Firestore;

namespace ProjectX.WebAPI.Models.Database.Authentication
{

    [FirestoreData]
    public record RefreshTokenDatabaseEntry
    {

        /// <summary>
        /// The Id of the refresh token. This does not change between refreshes
        /// </summary>
        [FirestoreDocumentId]
        public string TokenId { get; init; }

        /// <summary>
        /// A secret value of the JWT. This changes each time we refresh. 
        /// Used to authenticate against the database that this is in fact the lastest version of the refresh token.
        /// </summary>
        [FirestoreProperty]
        public string? Secret { get; init; }

        /// <summary>
        /// 
        /// </summary>
        [FirestoreProperty]
        public DateTime ValidUntil { get; init; }
        
    }


    public record RefreshToken : RefreshTokenDatabaseEntry
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
        /// The JWT refresh token itself.
        /// </summary>
        public string SignedJWT { get; init; }

    }
}
